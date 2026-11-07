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
                        "Request body is required",
                        ResponseStatus.InvalidArgument);
                }

                if (request.StudentId == Guid.Empty ||
                    request.InstrumentId == Guid.Empty ||
                    request.StartDate == null)
                {
                    return Error(
                        "StudentId, InstrumentId and StartDate are required",
                        ResponseStatus.InvalidArgument);
                }

                if (request.MonthlyFee <= 0)
                {
                    return Error(
                        "Monthly fee must be greater than zero",
                        ResponseStatus.InvalidArgument);
                }

                if (request.DueDay < 1 || request.DueDay > 28)
                {
                    return Error(
                        "Due day must be between 1 and 28",
                        ResponseStatus.InvalidArgument);
                }

                var student = await _studentRepository
                    .GetByIdAsync(request.StudentId);

                if (student == null)
                {
                    return Error("Student not found", ResponseStatus.NotFound);
                }

                var instrument = await _instrumentRepository
                    .GetByIdAsync(request.InstrumentId);

                if (instrument == null)
                {
                    return Error("Instrument not found", ResponseStatus.NotFound);
                }

                var alreadyEnrolled = await _enrollmentRepository
                    .HasActiveEnrollmentAsync(
                        request.StudentId,
                        request.InstrumentId);

                if (alreadyEnrolled)
                {
                    return Error(
                        "Student already has an active enrollment for this instrument",
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
                    Message = "Enrollment created successfully",
                    Status = ResponseStatus.Success,
                    Data = ToData(enrollment, student, instrument)
                };
            }
            catch (ConcurrencyConflictException)
            {
                return Error(
                    "Student already has an active enrollment for this instrument",
                    ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error(
                    "An unexpected error occurred while creating the enrollment",
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
                        "Invalid enrollment ID",
                        ResponseStatus.InvalidArgument);
                }

                if (request == null)
                {
                    return CloseError(
                        "Request body is required",
                        ResponseStatus.InvalidArgument);
                }

                if (request.EndDate == null)
                {
                    return CloseError(
                        "End date is required",
                        ResponseStatus.InvalidArgument);
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return CloseError(
                        "Reason is required",
                        ResponseStatus.InvalidArgument);
                }

                var reason = request.Reason.Trim();

                if (reason.Length > Enrollment.MaxCloseReasonLength)
                {
                    return CloseError(
                        $"Reason cannot exceed {Enrollment.MaxCloseReasonLength} characters",
                        ResponseStatus.InvalidArgument);
                }

                var enrollment = await _enrollmentRepository.GetByIdAsync(id);

                if (enrollment == null)
                {
                    return CloseError("Enrollment not found", ResponseStatus.NotFound);
                }

                if (enrollment.Status == EnrollmentStatus.Closed)
                {
                    return CloseError(
                        "Enrollment is already closed",
                        ResponseStatus.Conflict);
                }

                var endDate = request.EndDate.Value
                    .ToDateTime(TimeOnly.MinValue);

                if (endDate.Date < enrollment.StartDate.Date)
                {
                    return CloseError(
                        "End date cannot be before the enrollment start date",
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
                        "Enrollment has scheduled lessons after the end date",
                        ResponseStatus.Conflict);
                }

                enrollment.Close(endDate, reason);

                await _enrollmentRepository.UpdateAsync(enrollment);

                return new CloseEnrollmentResponseDTO
                {
                    Message = "Enrollment closed successfully",
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
                    "An unexpected error occurred while closing the enrollment",
                    ResponseStatus.Error);
            }
        }

        public async Task<EnrollmentResponseDTO> GetByIdAsync(
            Guid id,
            string? teacherEmail)
        {
            if (id == Guid.Empty)
                return Error("Invalid enrollment ID", ResponseStatus.InvalidArgument);

            try
            {
                var enrollment = await _enrollmentRepository.GetByIdAsync(id);

                if (enrollment == null)
                    return Error("Enrollment not found", ResponseStatus.NotFound);

                if (teacherEmail != null)
                {
                    var teacher = await _teacherRepository.GetByEmailAsync(teacherEmail);

                    if (teacher == null ||
                        !await _enrollmentRepository.HasLessonWithTeacherAsync(id, teacher.Id))
                    {
                        return Error("Enrollment not found", ResponseStatus.NotFound);
                    }
                }

                return new EnrollmentResponseDTO
                {
                    Message = "Enrollment retrieved successfully",
                    Status = ResponseStatus.Success,
                    Data = ToData(enrollment, enrollment.Student, enrollment.Instrument)
                };
            }
            catch (Exception)
            {
                return Error(
                    "An unexpected error occurred while retrieving the enrollment",
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
                        "Status must be Active or Closed",
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
                    Message = "Enrollments retrieved successfully",
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
                    "An unexpected error occurred while retrieving enrollments",
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
            Message = "Enrollments retrieved successfully",
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
