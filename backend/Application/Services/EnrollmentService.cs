using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class EnrollmentService : IEnrollmentServices
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ITeacherRepository _teacherRepository;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            IInstrumentRepository instrumentRepository,
            ILessonRepository lessonRepository,
            ITeacherRepository teacherRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _instrumentRepository = instrumentRepository;
            _lessonRepository = lessonRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<EnrollmentResponseDTO> CreateEnrollmentAsync(
            EnrollmentRequestDTO request)
        {
            try
            {
                if (request == null)
                {
                    return Error(
                        "Corpo da requisição é obrigatório",
                        ResponseStatus.InvalidArgument);
                }

                if (request.StudentId == Guid.Empty ||
                    request.InstrumentId == Guid.Empty ||
                    request.StartDate == null)
                {
                    return Error(
                        "Aluno, instrumento e data de início são obrigatórios",
                        ResponseStatus.InvalidArgument);
                }

                if (request.MonthlyFee <= 0)
                {
                    return Error(
                        "Mensalidade deve ser maior que zero",
                        ResponseStatus.InvalidArgument);
                }

                if (request.DueDay < 1 || request.DueDay > 28)
                {
                    return Error(
                        "Dia de vencimento deve estar entre 1 e 28",
                        ResponseStatus.InvalidArgument);
                }

                var student = await _studentRepository
                    .GetByIdAsync(request.StudentId);

                if (student == null)
                {
                    return Error("Aluno não encontrado", ResponseStatus.NotFound);
                }

                var instrument = await _instrumentRepository
                    .GetByIdAsync(request.InstrumentId);

                if (instrument == null)
                {
                    return Error("Instrumento não encontrado", ResponseStatus.NotFound);
                }

                var alreadyEnrolled = await _enrollmentRepository
                    .HasActiveEnrollmentAsync(
                        request.StudentId,
                        request.InstrumentId);

                if (alreadyEnrolled)
                {
                    return Error(
                        "Aluno já possui matrícula ativa neste instrumento",
                        ResponseStatus.Conflict);
                }

                var enrollment = new Enrollment(
                    request.StudentId,
                    request.InstrumentId,
                    DateTime.SpecifyKind(request.StartDate.Value.Date, DateTimeKind.Unspecified),
                    Math.Round(request.MonthlyFee, 2, MidpointRounding.AwayFromZero),
                    request.DueDay);

                await _enrollmentRepository.AddAsync(enrollment);

                return new EnrollmentResponseDTO
                {
                    Message = "Matrícula criada com sucesso",
                    Status = ResponseStatus.Success,
                    Data = ToData(enrollment, student, instrument)
                };
            }
            catch (ConcurrencyConflictException)
            {
                return Error(
                    "Aluno já possui matrícula ativa neste instrumento",
                    ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error(
                    "Ocorreu um erro inesperado ao criar a matrícula",
                    ResponseStatus.Error);
            }
        }

        public async Task<CloseEnrollmentResponseDTO> CloseEnrollmentAsync(
            Guid id,
            CloseEnrollmentRequestDTO request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return CloseError(
                        "ID de matrícula inválido",
                        ResponseStatus.InvalidArgument);
                }

                if (request == null)
                {
                    return CloseError(
                        "Corpo da requisição é obrigatório",
                        ResponseStatus.InvalidArgument);
                }

                if (request.EndDate == null)
                {
                    return CloseError(
                        "Data de término é obrigatória",
                        ResponseStatus.InvalidArgument);
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return CloseError(
                        "Motivo é obrigatório",
                        ResponseStatus.InvalidArgument);
                }

                var reason = request.Reason.Trim();

                if (reason.Length > Enrollment.MaxCloseReasonLength)
                {
                    return CloseError(
                        $"Motivo deve ter no máximo {Enrollment.MaxCloseReasonLength} caracteres",
                        ResponseStatus.InvalidArgument);
                }

                var enrollment = await _enrollmentRepository.GetByIdAsync(id);

                if (enrollment == null)
                {
                    return CloseError("Matrícula não encontrada", ResponseStatus.NotFound);
                }

                if (enrollment.Status == EnrollmentStatus.Closed)
                {
                    return CloseError(
                        "Matrícula já está encerrada",
                        ResponseStatus.Conflict);
                }

                var endDate = request.EndDate.Value
                    .ToDateTime(TimeOnly.MinValue);

                if (endDate.Date < enrollment.StartDate.Date)
                {
                    return CloseError(
                        "Data de término não pode ser anterior ao início da matrícula",
                        ResponseStatus.InvalidArgument);
                }

                var lastValidMoment = endDate.Date.AddDays(1).AddTicks(-1);
                var blockingFrom = lastValidMoment > DateTime.Now
                    ? lastValidMoment
                    : DateTime.Now;

                if (await _lessonRepository
                    .HasFutureScheduledLessonsByEnrollmentAsync(
                        enrollment.Id,
                        blockingFrom))
                {
                    return CloseError(
                        "A matrícula possui aulas agendadas após a data de término",
                        ResponseStatus.Conflict);
                }

                enrollment.Close(endDate, reason);

                await _enrollmentRepository.UpdateAsync(enrollment);

                return new CloseEnrollmentResponseDTO
                {
                    Message = "Matrícula encerrada com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new CloseEnrollmentData
                    {
                        Id = enrollment.Id,
                        Status = enrollment.Status.ToString(),
                        EndDate = request.EndDate.Value
                    }
                };
            }
            catch (Exception)
            {
                return CloseError(
                    "Ocorreu um erro inesperado ao encerrar a matrícula",
                    ResponseStatus.Error);
            }
        }

        public async Task<EnrollmentResponseDTO> GetByIdAsync(
            Guid id,
            string? teacherEmail)
        {
            if (id == Guid.Empty)
                return Error("ID de matrícula inválido", ResponseStatus.InvalidArgument);

            try
            {
                var enrollment = await _enrollmentRepository.GetByIdAsync(id);

                if (enrollment == null)
                    return Error("Matrícula não encontrada", ResponseStatus.NotFound);

                if (teacherEmail != null)
                {
                    var teacher = await _teacherRepository.GetByEmailAsync(teacherEmail);

                    if (teacher == null ||
                        !await _enrollmentRepository.HasLessonWithTeacherAsync(id, teacher.Id))
                    {
                        return Error("Matrícula não encontrada", ResponseStatus.NotFound);
                    }
                }

                return new EnrollmentResponseDTO
                {
                    Message = "Matrícula encontrada com sucesso",
                    Status = ResponseStatus.Success,
                    Data = ToData(enrollment, enrollment.Student, enrollment.Instrument)
                };
            }
            catch (Exception)
            {
                return Error(
                    "Ocorreu um erro inesperado ao buscar a matrícula",
                    ResponseStatus.Error);
            }
        }

        public async Task<EnrollmentListResponseDTO> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            string? status,
            string? teacherEmail,
            int page,
            int pageSize)
        {
            if (!PaginationLimits.IsValid(page, pageSize))
            {
                return ListError(
                    PaginationLimits.InvalidMessage,
                    ResponseStatus.InvalidArgument);
            }

            EnrollmentStatus? enrollmentStatus = null;

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    enrollmentStatus = EnrollmentStatus.Active;
                }
                else if (string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase))
                {
                    enrollmentStatus = EnrollmentStatus.Closed;
                }
                else
                {
                    return ListError(
                        "Status deve ser Active ou Closed",
                        ResponseStatus.InvalidArgument);
                }
            }

            try
            {
                Guid? teacherId = null;

                if (teacherEmail != null)
                {
                    var teacher = await _teacherRepository.GetByEmailAsync(teacherEmail);

                    if (teacher == null)
                        return EmptyList(page, pageSize);

                    teacherId = teacher.Id;
                }

                var result = await _enrollmentRepository.GetAllAsync(
                    studentId,
                    instrumentId,
                    enrollmentStatus,
                    teacherId,
                    page,
                    pageSize);

                return new EnrollmentListResponseDTO
                {
                    Message = "Matrículas listadas com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new EnrollmentListData
                    {
                        Items = result.Items.Select(enrollment =>
                            new EnrollmentListItemDTO
                            {
                                Id = enrollment.Id,
                                StudentName = enrollment.Student.Name,
                                InstrumentName = enrollment.Instrument.Name,
                                StartDate = enrollment.StartDate,
                                Status = enrollment.Status.ToString()
                            }).ToList(),
                        Page = page,
                        PageSize = pageSize,
                        Total = result.Total
                    }
                };
            }
            catch (Exception)
            {
                return ListError(
                    "Ocorreu um erro inesperado ao listar as matrículas",
                    ResponseStatus.Error);
            }
        }

        private static EnrollmentData ToData(
            Enrollment enrollment,
            Student student,
            Instrument instrument) => new()
            {
                Id = enrollment.Id,
                Student = new StudentEnrollmentData
                {
                    Id = student.Id,
                    Name = student.Name
                },
                Instrument = new InstrumentEnrollmentData
                {
                    Id = instrument.Id,
                    Name = instrument.Name
                },
                StartDate = enrollment.StartDate,
                MonthlyFee = enrollment.MonthlyFee,
                DueDay = enrollment.DueDay,
                Status = enrollment.Status.ToString()
            };

        private static EnrollmentListResponseDTO EmptyList(int page, int pageSize) => new()
        {
            Message = "Matrículas listadas com sucesso",
            Status = ResponseStatus.Success,
            Data = new EnrollmentListData
            {
                Page = page,
                PageSize = pageSize,
                Total = 0
            }
        };

        private static CloseEnrollmentResponseDTO CloseError(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };

        private static EnrollmentResponseDTO Error(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };

        private static EnrollmentListResponseDTO ListError(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };
    }
}
