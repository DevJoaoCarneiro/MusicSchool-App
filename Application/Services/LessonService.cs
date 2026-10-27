
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
        private readonly ITeacherRepository _teacherRepository;

        public LessonService(
            ILessonRepository lessonRepository,
            IEnrollmentRepository enrollmentRepository,
            ITeacherRepository teacherRepository)
        {
            _lessonRepository = lessonRepository;
            _enrollmentRepository = enrollmentRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<LessonResponseDTO> CreateAsync(
            LessonRequestDTO request)
        {
            if (request == null)
                return Error("Request is required", "BadRequest");

            if (request.EnrollmentId == Guid.Empty)
                return Error("Enrollment is required", "BadRequest");

            if (request.TeacherId == Guid.Empty)
                return Error("Teacher is required", "BadRequest");

            if (request.StartsAt <= DateTime.Now)
                return Error("Lesson date must be in the future", "BadRequest");

            if (request.DurationMinutes < 30 ||
                request.DurationMinutes > 120)
            {
                return Error(
                    "Lesson duration must be between 30 and 120 minutes",
                    "BadRequest");
            }

            if (string.IsNullOrWhiteSpace(request.Room))
                return Error("Room is required", "BadRequest");

            if (request.Room.Trim().Length > 60)
                return Error("Room cannot exceed 60 characters", "BadRequest");

            try
            {
                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(request.EnrollmentId);

                if (enrollment == null)
                    return Error("Enrollment not found", "NotFound");

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("Enrollment is inactive", "Conflict");

                var endsAt = request.StartsAt.AddMinutes(
                    request.DurationMinutes);

                var teacher = await _teacherRepository
                    .GetByIdAsync(request.TeacherId);

                if (teacher == null)
                    return Error("Teacher not found", "NotFound");

                var instrumentName = enrollment.Instrument.Name;

                var teachesInstrument = teacher.Specialties.Any(s =>
                    !string.IsNullOrWhiteSpace(s) &&
                    string.Equals(
                        s.Trim(),
                        instrumentName.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (!teachesInstrument)
                {
                    return Error(
                        "Teacher does not teach the instrument of this enrollment",
                        "BadRequest");
                }

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
            catch (ArgumentException ex)
            {
                return Error(ex.Message, "BadRequest");
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", "Error");
            }
        }

        public async Task<LessonResponseDTO> GetByDateRangeAsync(
            DateTime? from,
            DateTime? to,
            Guid? teacherId,
            Guid? studentId)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return Error(
                    "The from and to dates are required",
                    "BadRequest");
            }

            var startDate = from.Value.Date;
            var endDate = to.Value.Date;

            if (endDate < startDate)
            {
                return Error(
                    "The to date must be on or after the from date",
                    "BadRequest");
            }

            var numberOfDays = (endDate - startDate).TotalDays + 1;

            if (numberOfDays > 42)
            {
                return Error(
                    "The date range cannot exceed 42 days",
                    "BadRequest");
            }

            if (teacherId == Guid.Empty)
                return Error("Invalid teacherId", "BadRequest");

            if (studentId == Guid.Empty)
                return Error("Invalid studentId", "BadRequest");

            try
            {
                var lessons = await _lessonRepository.GetByDateRangeAsync(
                    startDate,
                    endDate,
                    teacherId,
                    studentId);

                var results = new List<object>();

                foreach (var lesson in lessons)
                {
                    var teacher = await _teacherRepository.GetByIdAsync(
                        lesson.TeacherId);

                    results.Add(new
                    {
                        id = lesson.Id,
                        studentName = lesson.Enrollment.Student.Name,
                        teacherName = teacher?.Name,
                        instrument = lesson.Enrollment.Instrument.Name,
                        startsAt = lesson.StartsAt,
                        endsAt = lesson.EndsAt,
                        room = lesson.Room,
                        status = lesson.Status.ToString()
                    });
                }

                return new LessonResponseDTO
                {
                    Message = "Lessons retrieved successfully",
                    Status = "Success",
                    Data = results
                };
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
