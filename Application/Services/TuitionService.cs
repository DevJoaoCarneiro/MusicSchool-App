using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class TuitionService : ITuitionService
    {
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
                        "Reference month is required and must be in the format yyyy-MM",
                        "invalid_argument");
                }

                var referenceMonth = Tuition.FormatReferenceMonth(year, month);

                var monthEnd = new DateTime(
                    year,
                    month,
                    DateTime.DaysInMonth(year, month));

                var enrollments = await _enrollmentRepository
                    .GetActiveUntilAsync(monthEnd);

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
                    Message = "Tuitions generated successfully",
                    Status = "Success",
                    Data = new GenerateTuitionsData
                    {
                        ReferenceMonth = referenceMonth,
                        Created = newTuitions.Count,
                        Skipped = enrollments.Count - newTuitions.Count
                    }
                };
            }
            catch (Exception)
            {
                return Error(
                    "An unexpected error occurred while generating tuitions",
                    "error");
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
                        "Invalid tuition ID",
                        "invalid_argument");
                }

                if (request == null)
                {
                    return PaymentError(
                        "Request body is required",
                        "invalid_argument");
                }

                if (request.PaidAt == null)
                {
                    return PaymentError(
                        "Payment date is required",
                        "invalid_argument");
                }

                if (request.Amount == null)
                {
                    return PaymentError(
                        "Amount is required",
                        "invalid_argument");
                }

                var amount = Math.Round(
                    request.Amount.Value,
                    2,
                    MidpointRounding.AwayFromZero);

                if (amount <= 0)
                {
                    return PaymentError(
                        "Amount must be greater than zero",
                        "invalid_argument");
                }

                if (amount > Tuition.MaxAmount)
                {
                    return PaymentError(
                        "Amount exceeds the maximum allowed",
                        "invalid_argument");
                }

                if (!TryParsePaymentMethod(request.Method, out var method))
                {
                    return PaymentError(
                        "Method is required and must be one of: Pix, Cash, Card",
                        "invalid_argument");
                }

                var tuition = await _tuitionRepository.GetByIdAsync(id);

                if (tuition == null)
                {
                    return PaymentError("Tuition not found", "not_found");
                }

                if (tuition.Status == TuitionStatus.Paid)
                {
                    return PaymentError(
                        "Tuition is already paid",
                        "conflict");
                }

                tuition.RegisterPayment(
                    request.PaidAt.Value.ToDateTime(TimeOnly.MinValue),
                    amount,
                    method);

                await _tuitionRepository.UpdateAsync(tuition);

                return new RegisterTuitionPaymentResponseDTO
                {
                    Message = "Payment registered successfully",
                    Status = "Success",
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
                    "An unexpected error occurred while registering the payment",
                    "error");
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
            string status)
        {
            return new RegisterTuitionPaymentResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }

        private static GenerateTuitionsResponseDTO Error(
            string message,
            string status)
        {
            return new GenerateTuitionsResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }
    }
}