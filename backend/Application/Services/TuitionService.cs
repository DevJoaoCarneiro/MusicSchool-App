using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class TuitionService : ITuitionService
    {
        private const int MinReferenceYear = 2000;

        private readonly ITuitionRepository _tuitionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public TuitionService(
            ITuitionRepository tuitionRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _tuitionRepository = tuitionRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<GenerateTuitionsResponseDTO> GenerateAsync(
            GenerateTuitionsRequestDTO request)
        {
            try
            {
                if (request == null ||
                    !Tuition.TryParseReferenceMonth(
                        request.ReferenceMonth,
                        out var year,
                        out var month))
                {
                    return Error(
                        "Mês de referência é obrigatório e deve estar no formato yyyy-MM",
                        ResponseStatus.InvalidArgument);
                }

                var monthStart = new DateTime(year, month, 1);
                var today = DateTime.Today;
                var nextMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(1);

                if (year < MinReferenceYear || monthStart > nextMonthStart)
                {
                    return Error(
                        $"Mês de referência deve estar entre 01/{MinReferenceYear} e {nextMonthStart:MM/yyyy}",
                        ResponseStatus.InvalidArgument);
                }

                var referenceMonth = Tuition.FormatReferenceMonth(year, month);

                var monthEnd = new DateTime(
                    year,
                    month,
                    DateTime.DaysInMonth(year, month));

                var enrollments = await _enrollmentRepository
                    .GetBillableForMonthAsync(monthStart, monthEnd);

                var alreadyBilled = await _tuitionRepository
                    .GetEnrollmentIdsByReferenceMonthAsync(referenceMonth);

                var newTuitions = enrollments
                    .Where(e => !alreadyBilled.Contains(e.Id))
                    .Select(e => new Tuition(e, referenceMonth))
                    .ToList();

                if (newTuitions.Count > 0)
                {
                    await _tuitionRepository.AddRangeAsync(newTuitions);
                }

                return new GenerateTuitionsResponseDTO
                {
                    Message = "Mensalidades geradas com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new GenerateTuitionsData
                    {
                        ReferenceMonth = referenceMonth,
                        Created = newTuitions.Count,
                        Skipped = enrollments.Count - newTuitions.Count
                    }
                };
            }
            catch (ConcurrencyConflictException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error(
                    "Ocorreu um erro inesperado ao gerar as mensalidades",
                    ResponseStatus.Error);
            }
        }

        public async Task<RegisterTuitionPaymentResponseDTO> RegisterPaymentAsync(
            Guid id,
            RegisterTuitionPaymentRequestDTO request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return PaymentError(
                        "ID de mensalidade inválido",
                        ResponseStatus.InvalidArgument);
                }

                if (request == null)
                {
                    return PaymentError(
                        "Corpo da requisição é obrigatório",
                        ResponseStatus.InvalidArgument);
                }

                if (request.PaidAt == null)
                {
                    return PaymentError(
                        "Data de pagamento é obrigatória",
                        ResponseStatus.InvalidArgument);
                }

                if (request.PaidAt.Value > DateOnly.FromDateTime(DateTime.Today))
                {
                    return PaymentError(
                        "Data de pagamento não pode estar no futuro",
                        ResponseStatus.InvalidArgument);
                }

                if (request.Amount == null)
                {
                    return PaymentError(
                        "Valor é obrigatório",
                        ResponseStatus.InvalidArgument);
                }

                var amount = Math.Round(
                    request.Amount.Value,
                    2,
                    MidpointRounding.AwayFromZero);

                if (amount <= 0)
                {
                    return PaymentError(
                        "Valor deve ser maior que zero",
                        ResponseStatus.InvalidArgument);
                }

                if (amount > Tuition.MaxAmount)
                {
                    return PaymentError(
                        "Valor acima do máximo permitido",
                        ResponseStatus.InvalidArgument);
                }

                if (!TryParsePaymentMethod(request.Method, out var method))
                {
                    return PaymentError(
                        "Forma de pagamento é obrigatória: Pix, Dinheiro ou Cartão",
                        ResponseStatus.InvalidArgument);
                }

                var tuition = await _tuitionRepository.GetByIdAsync(id);

                if (tuition == null)
                {
                    return PaymentError("Mensalidade não encontrada", ResponseStatus.NotFound);
                }

                if (tuition.Status == TuitionStatus.Paid)
                {
                    return PaymentError(
                        "Mensalidade já está paga",
                        ResponseStatus.Conflict);
                }

                if (amount < tuition.Amount)
                {
                    return PaymentError(
                        $"Valor não pode ser menor que o da mensalidade (R$ {tuition.Amount:0.00})",
                        ResponseStatus.InvalidArgument);
                }

                var paidAt = request.PaidAt.Value.ToDateTime(TimeOnly.MinValue);

                tuition.RegisterPayment(paidAt, amount, method);

                if (!await _tuitionRepository.RegisterPaymentIfPendingAsync(tuition))
                {
                    return PaymentError(
                        "Mensalidade já está paga",
                        ResponseStatus.Conflict);
                }

                return new RegisterTuitionPaymentResponseDTO
                {
                    Message = "Pagamento registrado com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new RegisterTuitionPaymentData
                    {
                        Id = tuition.Id,
                        ReferenceMonth = tuition.ReferenceMonth,
                        Amount = tuition.PaidAmount!.Value,
                        Status = tuition.Status.ToString(),
                        PaidAt = request.PaidAt.Value,
                        Method = tuition.Method!.Value.ToString()
                    }
                };
            }
            catch (Exception)
            {
                return PaymentError(
                    "Ocorreu um erro inesperado ao registrar o pagamento",
                    ResponseStatus.Error);
            }
        }

        public async Task<TuitionListResponseDTO> GetAllAsync(
            string? referenceMonth,
            string? status,
            Guid? enrollmentId,
            Guid? studentId,
            int page,
            int pageSize)
        {
            if (!PaginationLimits.IsValid(page, pageSize))
                return ListError(PaginationLimits.InvalidMessage, ResponseStatus.InvalidArgument);

            string? normalizedMonth = null;

            if (!string.IsNullOrWhiteSpace(referenceMonth))
            {
                if (!Tuition.TryParseReferenceMonth(referenceMonth, out var year, out var month))
                {
                    return ListError(
                        "Mês de referência deve estar no formato yyyy-MM",
                        ResponseStatus.InvalidArgument);
                }

                normalizedMonth = Tuition.FormatReferenceMonth(year, month);
            }

            TuitionStatus? tuitionStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                var name = Enum.GetNames<TuitionStatus>().FirstOrDefault(n =>
                    string.Equals(n, status.Trim(), StringComparison.OrdinalIgnoreCase));

                if (name == null)
                {
                    return ListError(
                        "Status deve ser Pending ou Paid",
                        ResponseStatus.InvalidArgument);
                }

                tuitionStatus = Enum.Parse<TuitionStatus>(name);
            }

            try
            {
                var (items, total) = await _tuitionRepository.GetAllAsync(
                    normalizedMonth,
                    tuitionStatus,
                    enrollmentId,
                    studentId,
                    page,
                    pageSize);

                return new TuitionListResponseDTO
                {
                    Message = "Mensalidades listadas com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new TuitionListData
                    {
                        Items = items.Select(t => new TuitionListItemDTO
                        {
                            Id = t.Id,
                            EnrollmentId = t.EnrollmentId,
                            StudentName = t.Enrollment.Student.Name,
                            InstrumentName = t.Enrollment.Instrument.Name,
                            ReferenceMonth = t.ReferenceMonth,
                            Amount = t.Amount,
                            DueDate = DateOnly.FromDateTime(t.DueDate),
                            Status = t.Status.ToString(),
                            PaidAt = t.PaidAt.HasValue ? DateOnly.FromDateTime(t.PaidAt.Value) : null,
                            PaidAmount = t.PaidAmount,
                            Method = t.Method?.ToString()
                        }).ToList(),
                        Page = page,
                        PageSize = pageSize,
                        Total = total
                    }
                };
            }
            catch (Exception)
            {
                return ListError(
                    "Ocorreu um erro inesperado ao listar as mensalidades",
                    ResponseStatus.Error);
            }
        }

        private static bool TryParsePaymentMethod(
            string? value,
            out PaymentMethod method)
        {
            method = default;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            var name = Enum.GetNames<PaymentMethod>().FirstOrDefault(n =>
                string.Equals(
                    n,
                    value.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (name == null)
                return false;

            method = Enum.Parse<PaymentMethod>(name);
            return true;
        }

        private static RegisterTuitionPaymentResponseDTO PaymentError(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };

        private static GenerateTuitionsResponseDTO Error(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };

        private static TuitionListResponseDTO ListError(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };
    }
}
