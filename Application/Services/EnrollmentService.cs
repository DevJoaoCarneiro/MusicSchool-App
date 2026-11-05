
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class EnrollmentService : IEnrollmentServices
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ILessonRepository _lessonRepository;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            IInstrumentRepository instrumentRepository,
            ILessonRepository lessonRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _instrumentRepository = instrumentRepository;
            _lessonRepository = lessonRepository;
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
                        "invalid_argument");
                }

                if (request.StudentId == Guid.Empty ||
                    request.InstrumentId == Guid.Empty ||
                    request.StartDate == null)
                {
                    return Error(
                        "StudentId, InstrumentId and StartDate are required",
                        "invalid_argument");
                }

                if (request.MonthlyFee <= 0)
                {
                    return Error(
                        "Monthly fee must be greater than zero",
                        "invalid_argument");
                }

                if (request.DueDay < 1 || request.DueDay > 28)
                {
                    return Error(
                        "Due day must be between 1 and 28",
                        "invalid_argument");
                }

                var student = await _studentRepository
                    .GetByIdAsync(request.StudentId);

                if (student == null)
                {
                    return Error("Student not found", "not_found");
                }

                var instrument = await _instrumentRepository
                    .GetByIdAsync(request.InstrumentId);

                if (instrument == null)
                {
                    return Error("Instrument not found", "not_found");
                }

                var alreadyEnrolled = await _enrollmentRepository
                    .HasActiveEnrollmentAsync(
                        request.StudentId,
                        request.InstrumentId);

                if (alreadyEnrolled)
                {
                    return Error(
                        "Student already has an active enrollment for this instrument",
                        "conflict");
                }

                var enrollment = new Enrollment(
                    request.StudentId,
                    request.InstrumentId,
                    request.StartDate.Value,
                    request.MonthlyFee,
                    request.DueDay);

                await _enrollmentRepository.AddAsync(enrollment);

                return new EnrollmentResponseDTO
                {
                    Message = "Enrollment created successfully",
                    Status = "Success",
                    Data = new EnrollmentData
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
                    }
                };
            }
            catch (Exception)
            {
                return Error(
                    "An unexpected error occurred while creating the enrollment",
                    "error");
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
                        "invalid_argument");
                }

                if (request == null)
                {
                    return CloseError(
                        "Request body is required",
                        "invalid_argument");
                }

                if (request.EndDate == null)
                {
                    return CloseError(
                        "End date is required",
                        "invalid_argument");
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return CloseError(
                        "Reason is required",
                        "invalid_argument");
                }

                var reason = request.Reason.Trim();

                if (reason.Length > Enrollment.MaxCloseReasonLength)
                {
                    return CloseError(
                        $"Reason cannot exceed {Enrollment.MaxCloseReasonLength} characters",
                        "invalid_argument");
                }

                var enrollment = await _enrollmentRepository.GetByIdAsync(id);

                if (enrollment == null)
                {
                    return CloseError("Enrollment not found", "not_found");
                }

                var endDate = request.EndDate.Value
                    .ToDateTime(TimeOnly.MinValue);

                if (endDate.Date < enrollment.StartDate.Date)
                {
                    return CloseError(
                        "End date cannot be before the enrollment start date",
                        "invalid_argument");
                }

                if (enrollment.Status == EnrollmentStatus.Closed)
                {
                    return CloseError(
                        "Enrollment is already closed",
                        "conflict");
                }

                if (await _lessonRepository
                    .HasFutureScheduledLessonsByEnrollmentAsync(
                        enrollment.Id,
                        DateTime.Now))
                {
                    return CloseError(
                        "Enrollment has future scheduled lessons",
                        "conflict");
                }

                enrollment.Close(endDate, reason);

                await _enrollmentRepository.UpdateAsync(enrollment);

                return new CloseEnrollmentResponseDTO
                {
                    Message = "Enrollment closed successfully",
                    Status = "Success",
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
                    "error");
            }
        }

        private static CloseEnrollmentResponseDTO CloseError(
            string message,
            string status)
        {
            return new CloseEnrollmentResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }
        public async Task<EnrollmentListResponseDTO> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            string? status,
            int page,
            int pageSize)
        {
            if (page < 1 || pageSize < 1)
            {
                return ListError(
                    "Page and pageSize must be greater than zero",
                    "invalid_argument");
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
                        "invalid_argument");
                }
            }

            try
            {
                var result = await _enrollmentRepository.GetAllAsync(
                    studentId,
                    instrumentId,
                    enrollmentStatus,
                    page,
                    pageSize);

                return new EnrollmentListResponseDTO
                {
                    Message = "Enrollments retrieved successfully",
                    Status = "Success",
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
                    "error");
            }
        }

        private static EnrollmentResponseDTO Error(
            string message,
            string status)
        {
            return new EnrollmentResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }

        private static EnrollmentListResponseDTO ListError(
            string message,
            string status)
        {
            return new EnrollmentListResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }
    }
}
