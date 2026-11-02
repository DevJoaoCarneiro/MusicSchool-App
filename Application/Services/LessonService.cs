
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


        public async Task<LessonResponseDTO> CreateRecurringAsync(
            CreateRecurringLessonDTO request)
        {
            if (request == null)
                return Error("Request is required", "BadRequest");

            if (request.EnrollmentId == Guid.Empty)
                return Error("Enrollment is required", "BadRequest");

            if (request.TeacherId == Guid.Empty)
                return Error("Teacher is required", "BadRequest");

            if (!Enum.TryParse<DayOfWeek>(
                request.Weekday,
                true,
                out var weekday))
            {
                return Error("Invalid weekday", "BadRequest");
            }

            if (!TimeSpan.TryParseExact(
                request.Time,
                @"hh\:mm",
                System.Globalization.CultureInfo.InvariantCulture,
                out var lessonTime))
            {
                return Error("Invalid time. Use HH:mm format", "BadRequest");
            }

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

            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date;

            if (endDate < startDate)
                return Error("End date must be on or after start date", "BadRequest");

            if (endDate > startDate.AddMonths(6))
                return Error("Recurring period cannot exceed six months", "BadRequest");

            try
            {
                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(request.EnrollmentId);

                if (enrollment == null)
                    return Error("Enrollment not found", "NotFound");

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("Enrollment is inactive", "Conflict");

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

                // Encontra a primeira data que corresponde ao dia da semana.
                var firstDate = startDate.AddDays(
                    ((int)weekday - (int)startDate.DayOfWeek + 7) % 7);

                var dates = new List<DateTime>();

                for (var date = firstDate;
                     date <= endDate;
                     date = date.AddDays(7))
                {
                    dates.Add(date);
                }

                if (dates.Count == 0)
                    return Error(
                        "No lessons found in the selected period",
                        "BadRequest");

                var lessons = new List<Lesson>();
                var conflictingDates = new List<string>();

                foreach (var date in dates)
                {
                    var startsAt = date.Add(lessonTime);
                    var endsAt = startsAt.AddMinutes(request.DurationMinutes);

                    if (startsAt <= DateTime.Now)
                    {
                        return Error(
                            $"Lesson date {date:yyyy-MM-dd} must be in the future",
                            "BadRequest");
                    }

                    var teacherConflict =
                        await _lessonRepository.HasTeacherConflictAsync(
                            request.TeacherId, startsAt, endsAt);

                    var studentConflict =
                        await _lessonRepository.HasStudentConflictAsync(
                            enrollment.StudentId, startsAt, endsAt);

                    var roomConflict =
                        await _lessonRepository.HasRoomConflictAsync(
                            request.Room, startsAt, endsAt);

                    if (teacherConflict || studentConflict || roomConflict)
                    {
                        conflictingDates.Add(date.ToString("yyyy-MM-dd"));
                        continue;
                    }

                    var lesson = new Lesson(
                        enrollment,
                        request.TeacherId,
                        startsAt,
                        request.DurationMinutes,
                        request.Room);

                    lessons.Add(lesson);
                }

                // Nenhuma aula será salva se houver qualquer conflito.
                if (conflictingDates.Count > 0)
                {
                    return new LessonResponseDTO
                    {
                        Message = "Conflicts found in recurring lessons",
                        Status = "Conflict",
                        Data = new
                        {
                            conflictingDates
                        }
                    };
                }

                var series = new LessonSeries(
                    enrollment.Id,
                    teacher.Id,
                    weekday,
                    lessonTime,
                    startDate,
                    endDate,
                    request.DurationMinutes,
                    request.Room);

                foreach (var lesson in lessons)
                {
                    lesson.AssociateWithSeries(series);
                }

                await _lessonRepository.AddRecurringAsync(series, lessons);

                return new LessonResponseDTO
                {
                    Message = "Recurring lessons scheduled successfully",
                    Status = "Success",
                    Data = new
                    {
                        seriesId = series.Id,
                        lessonsCreated = lessons.Count,
                        firstLesson = lessons.First().StartsAt,
                        lastLesson = lessons.Last().StartsAt
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
                    studentId,
                    (lesson, teacher) => new
                    {
                        id = lesson.Id,
                        studentName = lesson.Enrollment.Student.Name,
                        teacherName = teacher.Name,
                        instrument = lesson.Enrollment.Instrument.Name,
                        startsAt = lesson.StartsAt,
                        endsAt = lesson.StartsAt.AddMinutes(
                            lesson.DurationMinutes),
                        room = lesson.Room,
                        status = lesson.Status
                    });

                var results = lessons.Select(lesson => new
                {
                    lesson.id,
                    lesson.studentName,
                    lesson.teacherName,
                    lesson.instrument,
                    lesson.startsAt,
                    lesson.endsAt,
                    lesson.room,
                    status = lesson.status.ToString()
                }).ToList();

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



        public async Task<LessonResponseDTO> CancelAsync(
            Guid id,
            CancelLessonRequestDTO request)
        {
            if (id == Guid.Empty)
                return Error("Invalid lesson ID", "BadRequest");

            if (request == null)
                return Error("Request is required", "BadRequest");

            if (string.IsNullOrWhiteSpace(request.Reason) ||
                request.Reason.Trim().Length < 5)
            {
                return Error(
                    "Cancellation reason must have at least 5 characters",
                    "BadRequest");
            }

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Lesson not found", "NotFound");

                lesson.Cancel(request.Reason);

                await _lessonRepository.UpdateAsync(lesson);

                return new LessonResponseDTO
                {
                    Message = "Lesson cancelled successfully",
                    Status = "Success",
                    Data = new
                    {
                        id = lesson.Id,
                        status = lesson.Status.ToString(),
                        reason = lesson.Reason
                    }
                };
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, "BadRequest");
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, "Conflict");
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", "Error");
            }
        }


        public async Task<LessonResponseDTO> RescheduleAsync(
            Guid id,
            RescheduleLessonRequestDTO request)
        {
            if (id == Guid.Empty)
                return Error("Invalid lesson ID", "BadRequest");

            if (request == null)
                return Error("Request is required", "BadRequest");

            if (request.NewStartsAt <= DateTime.Now)
                return Error("New lesson date must be in the future", "BadRequest");

            if (string.IsNullOrWhiteSpace(request.Room))
                return Error("Room is required", "BadRequest");

            if (request.Room.Trim().Length > 60)
                return Error("Room cannot exceed 60 characters", "BadRequest");

            if (string.IsNullOrWhiteSpace(request.Reason) ||
                request.Reason.Trim().Length < 5)
            {
                return Error(
                    "Rescheduling reason must have at least 5 characters",
                    "BadRequest");
            }

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Lesson not found", "NotFound");

                if (lesson.Status != LessonStatus.Scheduled)
                    return Error(
                        "Only scheduled lessons can be rescheduled",
                        "Conflict");

                if (lesson.StartsAt <= DateTime.Now)
                    return Error(
                        "A lesson that has already started cannot be rescheduled",
                        "Conflict");

                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(lesson.EnrollmentId);

                if (enrollment == null)
                    return Error("Enrollment not found", "NotFound");

                var newEndsAt = request.NewStartsAt.AddMinutes(
                    lesson.DurationMinutes);

                if (await _lessonRepository.HasTeacherConflictAsync(
                    lesson.TeacherId,
                    request.NewStartsAt,
                    newEndsAt,
                    lesson.Id))
                {
                    return Error(
                        "Conflict: teacher - Teacher already has a lesson at this time",
                        "Conflict");
                }

                if (await _lessonRepository.HasStudentConflictAsync(
                    enrollment.StudentId,
                    request.NewStartsAt,
                    newEndsAt,
                    lesson.Id))
                {
                    return Error(
                        "Conflict: student - Student already has a lesson at this time",
                        "Conflict");
                }

                if (await _lessonRepository.HasRoomConflictAsync(
                    request.Room,
                    request.NewStartsAt,
                    newEndsAt,
                    lesson.Id))
                {
                    return Error(
                        "Conflict: room - Room is already booked at this time",
                        "Conflict");
                }

                // Save the original information for the history
                var oldStartsAt = lesson.StartsAt;
                var oldRoom = lesson.Room;

                var lessonChange = new LessonChange(
                    lesson,
                    oldStartsAt,
                    request.NewStartsAt,
                    oldRoom,
                    request.Room.Trim(),
                    request.Reason);

                // Update the lesson
                lesson.Reschedule(
                    request.NewStartsAt,
                    request.Room);

                // Save the lesson and history together
                await _lessonRepository.UpdateWithHistoryAsync(
                    lesson,
                    lessonChange);

                return new LessonResponseDTO
                {
                    Message = "Lesson rescheduled successfully",
                    Status = "Success",
                    Data = new
                    {
                        id = lesson.Id,
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
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, "Conflict");
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
