
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class LessonService : ILessonServices
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;

        public LessonService(
            ILessonRepository lessonRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository)
        {
            _lessonRepository = lessonRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
        }

        public async Task<LessonResponseDTO> CreateAsync(
            LessonRequestDTO request)
        {
            if (request == null)
            {
                return Error("Request is required", "BadRequest");
            }

            if (request.EnrollmentId == Guid.Empty)
            {
                return Error("Enrollment is required", "BadRequest");
            }

            if (request.TeacherId == Guid.Empty)
            {
                return Error("Teacher is required", "BadRequest");
            }

            if (request.StartsAt <= DateTime.Now)
            {
                return Error("Lesson date must be in the future", "BadRequest");
            }

            if (request.DurationMinutes < 30 ||
                request.DurationMinutes > 120)
            {
                return Error(
                    "Lesson duration must be between 30 and 120 minutes",
                    "BadRequest");
            }

            if (string.IsNullOrWhiteSpace(request.Room))
            {
                return Error("Room is required", "BadRequest");
            }

            if (request.Room.Trim().Length > 60)
            {
                return Error(
                    "Room cannot exceed 60 characters",
                    "BadRequest");
            }

            var enrollment = await _enrollmentRepository
                .GetByIdAsync(request.EnrollmentId);

            if (enrollment == null)
            {
                return Error("Enrollment not found", "NotFound");
            }

            if (enrollment.Status != EnrollmentStatus.Active)
            {
                return Error("Enrollment is inactive", "Conflict");
            }

            var endsAt = request.StartsAt.AddMinutes(
                request.DurationMinutes);

            // Verify teacher exists
            var teacher = await _userRepository.GetByIdAsync(request.TeacherId);
            if (teacher == null)
            {
                return Error("Teacher not found", "NotFound");
            }

            // TODO: verify that the teacher actually teaches the instrument of the enrollment.
            // This check will be implemented by the colleague responsible for teacher domain.

            if (await _lessonRepository.HasTeacherConflictAsync(
                    request.TeacherId, request.StartsAt, endsAt))
            {
                return Error(
                    "Conflict: teacher - Teacher already has a lesson at this time",
                    "Conflict");
            }

            if (await _lessonRepository.HasStudentConflictAsync(
                    enrollment.StudentId, request.StartsAt, endsAt))
            {
                return Error(
                    "Conflict: student - Student already has a lesson at this time",
                    "Conflict");
            }

            if (await _lessonRepository.HasRoomConflictAsync(
                    request.Room, request.StartsAt, endsAt))
            {
                return Error(
                    "Conflict: room - Room is already booked at this time",
                    "Conflict");
            }

            try
            {
                var lesson = new Lesson(
                    enrollment,
                    request.TeacherId,
                    request.StartsAt,
                    request.DurationMinutes,
                    request.Room);

                await _lessonRepository.AddAsync(lesson);

                return new LessonResponseDTO
                {
                    Message = "Lesson scheduled successfully",
                    Status = "Success",
                    Data = new
                    {
                        id = lesson.Id,
                        student = new
                        {
                            id = enrollment.Student.Id,
                            name = enrollment.Student.Name
                        },
                        teacher = new
                        {
                            id = teacher.Id,
                            name = teacher.Name
                        },
                        instrument = enrollment.Instrument.Name,
                        startsAt = lesson.StartsAt,
                        endsAt = lesson.EndsAt,
                        room = lesson.Room,
                        status = lesson.Status.ToString()
                    }
                };
            }

            catch (ArgumentOutOfRangeException ex)
            {
                return Error(ex.Message, "BadRequest");
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, "BadRequest");
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", "Error");
            }

        }

        private static LessonResponseDTO Error(
            string message,
            string status)
        {
            return new LessonResponseDTO
            {
                Message = message,
                Status = status,
                Data = null
            };
        }
    }
}
