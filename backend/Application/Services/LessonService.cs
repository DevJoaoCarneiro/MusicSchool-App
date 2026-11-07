using System.Globalization;
using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class LessonService : ILessonServices
    {
        private const int MinDurationMinutes = 30;
        private const int MaxDurationMinutes = 120;
        private const int MaxRoomLength = 60;
        private const int MinReasonLength = 5;
        private const int MaxReasonLength = 500;
        private const int MaxRangeDays = 42;
        private const int MaxRecurringMonths = 6;

        private readonly ILessonRepository _lessonRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IUnitOfWork _unitOfWork;

        public LessonService(
            ILessonRepository lessonRepository,
            IEnrollmentRepository enrollmentRepository,
            ITeacherRepository teacherRepository,
            IUnitOfWork unitOfWork)
        {
            _lessonRepository = lessonRepository;
            _enrollmentRepository = enrollmentRepository;
            _teacherRepository = teacherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LessonResponseDTO> CreateAsync(
            LessonRequestDTO request)
        {
            if (request == null)
                return Error("Request is required", ResponseStatus.InvalidArgument);

            if (request.EnrollmentId == Guid.Empty)
                return Error("Enrollment is required", ResponseStatus.InvalidArgument);

            if (request.TeacherId == Guid.Empty)
                return Error("Teacher is required", ResponseStatus.InvalidArgument);

            var startsAt = Normalizer.LocalDateTime(request.StartsAt);

            if (startsAt <= DateTime.Now)
                return Error("Lesson date must be in the future", ResponseStatus.InvalidArgument);

            var basicValidation = ValidateDurationAndRoom(request.DurationMinutes, request.Room);

            if (basicValidation != null)
                return basicValidation;

            var room = request.Room.Trim();

            try
            {
                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(request.EnrollmentId);

                if (enrollment == null)
                    return Error("Enrollment not found", ResponseStatus.NotFound);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("Enrollment is inactive", ResponseStatus.Conflict);

                var teacher = await _teacherRepository
                    .GetByIdAsync(request.TeacherId);

                if (teacher == null)
                    return Error("Teacher not found", ResponseStatus.NotFound);

                if (!TeachesInstrument(teacher, enrollment.Instrument.Name))
                {
                    return Error(
                        "Teacher does not teach the instrument of this enrollment",
                        ResponseStatus.InvalidArgument);
                }

                var endsAt = startsAt.AddMinutes(request.DurationMinutes);

                return await _unitOfWork.ExecuteSerializableAsync(async () =>
                {
                    var conflict = await CheckConflictsAsync(
                        teacher.Id,
                        enrollment.StudentId,
                        room,
                        startsAt,
                        endsAt);

                    if (conflict != null)
                        return conflict;

                    var lesson = new Lesson(
                        enrollment,
                        teacher.Id,
                        startsAt,
                        request.DurationMinutes,
                        room);

                    await _lessonRepository.AddAsync(lesson);

                    return new LessonResponseDTO
                    {
                        Message = "Lesson scheduled successfully",
                        Status = ResponseStatus.Success,
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
                });
            }
            catch (ConcurrencyConflictException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, ResponseStatus.InvalidArgument);
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CreateRecurringAsync(
            CreateRecurringLessonDTO request)
        {
            if (request == null)
                return Error("Request is required", ResponseStatus.InvalidArgument);

            if (request.EnrollmentId == Guid.Empty)
                return Error("Enrollment is required", ResponseStatus.InvalidArgument);

            if (request.TeacherId == Guid.Empty)
                return Error("Teacher is required", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Weekday) ||
                int.TryParse(request.Weekday, out _) ||
                !Enum.TryParse<DayOfWeek>(request.Weekday.Trim(), true, out var weekday) ||
                !Enum.IsDefined(weekday))
            {
                return Error("Invalid weekday", ResponseStatus.InvalidArgument);
            }

            if (!TimeSpan.TryParseExact(
                request.Time,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out var lessonTime))
            {
                return Error("Invalid time. Use HH:mm format", ResponseStatus.InvalidArgument);
            }

            var basicValidation = ValidateDurationAndRoom(request.DurationMinutes, request.Room);

            if (basicValidation != null)
                return basicValidation;

            var room = request.Room.Trim();

            var startDate = Normalizer.LocalDateTime(request.StartDate).Date;
            var endDate = Normalizer.LocalDateTime(request.EndDate).Date;

            if (endDate < startDate)
                return Error("End date must be on or after start date", ResponseStatus.InvalidArgument);

            if (endDate > startDate.AddMonths(MaxRecurringMonths))
                return Error("Recurring period cannot exceed six months", ResponseStatus.InvalidArgument);

            try
            {
                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(request.EnrollmentId);

                if (enrollment == null)
                    return Error("Enrollment not found", ResponseStatus.NotFound);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("Enrollment is inactive", ResponseStatus.Conflict);

                var teacher = await _teacherRepository
                    .GetByIdAsync(request.TeacherId);

                if (teacher == null)
                    return Error("Teacher not found", ResponseStatus.NotFound);

                if (!TeachesInstrument(teacher, enrollment.Instrument.Name))
                {
                    return Error(
                        "Teacher does not teach the instrument of this enrollment",
                        ResponseStatus.InvalidArgument);
                }

                var firstDate = startDate.AddDays(
                    ((int)weekday - (int)startDate.DayOfWeek + 7) % 7);

                var slots = new List<(DateTime StartsAt, DateTime EndsAt)>();

                for (var date = firstDate; date <= endDate; date = date.AddDays(7))
                {
                    var startsAt = date.Add(lessonTime);
                    slots.Add((startsAt, startsAt.AddMinutes(request.DurationMinutes)));
                }

                if (slots.Count == 0)
                {
                    return Error(
                        "No lessons found in the selected period",
                        ResponseStatus.InvalidArgument);
                }

                var past = slots.FirstOrDefault(s => s.StartsAt <= DateTime.Now);

                if (past != default)
                {
                    return Error(
                        $"Lesson date {past.StartsAt:yyyy-MM-dd} must be in the future",
                        ResponseStatus.InvalidArgument);
                }

                return await _unitOfWork.ExecuteSerializableAsync(async () =>
                {
                    var existing = await _lessonRepository.GetScheduledSlotsAsync(
                        teacher.Id,
                        enrollment.StudentId,
                        room,
                        slots[0].StartsAt,
                        slots[^1].EndsAt);

                    var conflictingDates = slots
                        .Where(slot => existing.Any(e =>
                            e.Overlaps(slot.StartsAt, slot.EndsAt) &&
                            (e.TeacherId == teacher.Id ||
                             e.StudentId == enrollment.StudentId ||
                             string.Equals(e.Room.Trim(), room, StringComparison.OrdinalIgnoreCase))))
                        .Select(slot => slot.StartsAt.ToString("yyyy-MM-dd"))
                        .ToList();

                    if (conflictingDates.Count > 0)
                    {
                        return new LessonResponseDTO
                        {
                            Message = "Conflicts found in recurring lessons",
                            Status = ResponseStatus.Conflict,
                            Data = new { conflictingDates }
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
                        room);

                    var lessons = slots
                        .Select(slot =>
                        {
                            var lesson = new Lesson(
                                enrollment,
                                teacher.Id,
                                slot.StartsAt,
                                request.DurationMinutes,
                                room);

                            lesson.AssociateWithSeries(series);
                            return lesson;
                        })
                        .ToList();

                    await _lessonRepository.AddRecurringAsync(series, lessons);

                    return new LessonResponseDTO
                    {
                        Message = "Recurring lessons scheduled successfully",
                        Status = ResponseStatus.Success,
                        Data = new
                        {
                            seriesId = series.Id,
                            lessonsCreated = lessons.Count,
                            firstLesson = lessons[0].StartsAt,
                            lastLesson = lessons[^1].StartsAt
                        }
                    };
                });
            }
            catch (ConcurrencyConflictException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, ResponseStatus.InvalidArgument);
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> GetByDateRangeAsync(
            DateTime? from,
            DateTime? to,
            Guid? teacherId,
            Guid? studentId,
            string? teacherEmail)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return Error(
                    "The from and to dates are required",
                    ResponseStatus.InvalidArgument);
            }

            var startDate = Normalizer.LocalDateTime(from.Value).Date;
            var endDate = Normalizer.LocalDateTime(to.Value).Date;

            if (endDate < startDate)
            {
                return Error(
                    "The to date must be on or after the from date",
                    ResponseStatus.InvalidArgument);
            }

            if ((endDate - startDate).TotalDays + 1 > MaxRangeDays)
            {
                return Error(
                    $"The date range cannot exceed {MaxRangeDays} days",
                    ResponseStatus.InvalidArgument);
            }

            if (teacherId == Guid.Empty)
                return Error("Invalid teacherId", ResponseStatus.InvalidArgument);

            if (studentId == Guid.Empty)
                return Error("Invalid studentId", ResponseStatus.InvalidArgument);

            try
            {
                if (teacherEmail != null)
                {
                    var callerTeacher = await _teacherRepository.GetByEmailAsync(teacherEmail);

                    if (callerTeacher == null)
                    {
                        return new LessonResponseDTO
                        {
                            Message = "Lessons retrieved successfully",
                            Status = ResponseStatus.Success,
                            Data = Array.Empty<object>()
                        };
                    }

                    if (teacherId.HasValue && teacherId.Value != callerTeacher.Id)
                    {
                        return Error(
                            "Teachers can only view their own lessons",
                            ResponseStatus.Forbidden);
                    }

                    teacherId = callerTeacher.Id;
                }

                var lessons = await _lessonRepository.GetByDateRangeAsync(
                    startDate,
                    endDate,
                    teacherId,
                    studentId,
                    (lesson, teacher) => new
                    {
                        id = lesson.Id,
                        seriesId = lesson.LessonSeriesId,
                        studentName = lesson.Enrollment.Student.Name,
                        teacherName = teacher.Name,
                        instrument = lesson.Enrollment.Instrument.Name,
                        startsAt = lesson.StartsAt,
                        durationMinutes = lesson.DurationMinutes,
                        room = lesson.Room,
                        status = lesson.Status
                    });

                var results = lessons.Select(lesson => new
                {
                    lesson.id,
                    lesson.seriesId,
                    lesson.studentName,
                    lesson.teacherName,
                    lesson.instrument,
                    lesson.startsAt,
                    endsAt = lesson.startsAt.AddMinutes(lesson.durationMinutes),
                    lesson.room,
                    status = lesson.status.ToString()
                }).ToList();

                return new LessonResponseDTO
                {
                    Message = "Lessons retrieved successfully",
                    Status = ResponseStatus.Success,
                    Data = results
                };
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> GetByIdAsync(
            Guid id,
            string? teacherEmail)
        {
            if (id == Guid.Empty)
                return Error("Invalid lesson ID", ResponseStatus.InvalidArgument);

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(
                    id,
                    (lesson, teacher) => new
                    {
                        id = lesson.Id,
                        seriesId = lesson.LessonSeriesId,
                        enrollmentId = lesson.EnrollmentId,
                        teacherId = teacher.Id,
                        teacherEmail = teacher.Email,
                        teacherName = teacher.Name,
                        studentId = lesson.Enrollment.StudentId,
                        studentName = lesson.Enrollment.Student.Name,
                        instrument = lesson.Enrollment.Instrument.Name,
                        startsAt = lesson.StartsAt,
                        durationMinutes = lesson.DurationMinutes,
                        room = lesson.Room,
                        status = lesson.Status,
                        reason = lesson.Reason,
                        cancelledAt = lesson.CancelledAt
                    });

                if (lesson == null ||
                    (teacherEmail != null &&
                     !string.Equals(lesson.teacherEmail, teacherEmail, StringComparison.OrdinalIgnoreCase)))
                {
                    return Error("Lesson not found", ResponseStatus.NotFound);
                }

                return new LessonResponseDTO
                {
                    Message = "Lesson retrieved successfully",
                    Status = ResponseStatus.Success,
                    Data = new
                    {
                        lesson.id,
                        lesson.seriesId,
                        lesson.enrollmentId,
                        teacher = new { id = lesson.teacherId, name = lesson.teacherName },
                        student = new { id = lesson.studentId, name = lesson.studentName },
                        lesson.instrument,
                        lesson.startsAt,
                        endsAt = lesson.startsAt.AddMinutes(lesson.durationMinutes),
                        lesson.room,
                        status = lesson.status.ToString(),
                        lesson.reason,
                        lesson.cancelledAt
                    }
                };
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CancelAsync(
            Guid id,
            CancelLessonRequestDTO request)
        {
            if (id == Guid.Empty)
                return Error("Invalid lesson ID", ResponseStatus.InvalidArgument);

            if (request == null)
                return Error("Request is required", ResponseStatus.InvalidArgument);

            var reasonValidation = ValidateReason(request.Reason, "Cancellation");

            if (reasonValidation != null)
                return reasonValidation;

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Lesson not found", ResponseStatus.NotFound);

                lesson.Cancel(request.Reason);

                await _lessonRepository.UpdateAsync(lesson);

                return new LessonResponseDTO
                {
                    Message = "Lesson cancelled successfully",
                    Status = ResponseStatus.Success,
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
                return Error(ex.Message, ResponseStatus.InvalidArgument);
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CompleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                return Error("Invalid lesson ID", ResponseStatus.InvalidArgument);

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Lesson not found", ResponseStatus.NotFound);

                lesson.Complete();

                await _lessonRepository.UpdateAsync(lesson);

                return new LessonResponseDTO
                {
                    Message = "Lesson completed successfully",
                    Status = ResponseStatus.Success,
                    Data = new
                    {
                        id = lesson.Id,
                        status = lesson.Status.ToString()
                    }
                };
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CancelSeriesAsync(
            Guid seriesId,
            CancelLessonSeriesRequestDTO request)
        {
            if (seriesId == Guid.Empty)
                return Error("Invalid series ID", ResponseStatus.InvalidArgument);

            if (request == null)
                return Error("Request is required", ResponseStatus.InvalidArgument);

            var reasonValidation = ValidateReason(request.Reason, "Cancellation");

            if (reasonValidation != null)
                return reasonValidation;

            try
            {
                if (!await _lessonRepository.SeriesExistsAsync(seriesId))
                    return Error("Lesson series not found", ResponseStatus.NotFound);

                var lessons = await _lessonRepository
                    .GetFutureScheduledBySeriesAsync(seriesId, DateTime.Now);

                foreach (var lesson in lessons)
                {
                    lesson.Cancel(request.Reason);
                }

                if (lessons.Count > 0)
                {
                    await _lessonRepository.UpdateRangeAsync(lessons);
                }

                return new LessonResponseDTO
                {
                    Message = "Lesson series cancelled successfully",
                    Status = ResponseStatus.Success,
                    Data = new
                    {
                        seriesId,
                        lessonsCancelled = lessons.Count
                    }
                };
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, ResponseStatus.InvalidArgument);
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> RescheduleAsync(
            Guid id,
            RescheduleLessonRequestDTO request)
        {
            if (id == Guid.Empty)
                return Error("Invalid lesson ID", ResponseStatus.InvalidArgument);

            if (request == null)
                return Error("Request is required", ResponseStatus.InvalidArgument);

            var newStartsAt = Normalizer.LocalDateTime(request.NewStartsAt);

            if (newStartsAt <= DateTime.Now)
                return Error("New lesson date must be in the future", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Room))
                return Error("Room is required", ResponseStatus.InvalidArgument);

            var room = request.Room.Trim();

            if (room.Length > MaxRoomLength)
                return Error($"Room cannot exceed {MaxRoomLength} characters", ResponseStatus.InvalidArgument);

            var reasonValidation = ValidateReason(request.Reason, "Rescheduling");

            if (reasonValidation != null)
                return reasonValidation;

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Lesson not found", ResponseStatus.NotFound);

                if (lesson.Status != LessonStatus.Scheduled)
                {
                    return Error(
                        "Only scheduled lessons can be rescheduled",
                        ResponseStatus.Conflict);
                }

                if (lesson.StartsAt <= DateTime.Now)
                {
                    return Error(
                        "A lesson that has already started cannot be rescheduled",
                        ResponseStatus.Conflict);
                }

                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(lesson.EnrollmentId);

                if (enrollment == null)
                    return Error("Enrollment not found", ResponseStatus.NotFound);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("Enrollment is inactive", ResponseStatus.Conflict);

                var newEndsAt = newStartsAt.AddMinutes(lesson.DurationMinutes);

                return await _unitOfWork.ExecuteSerializableAsync(async () =>
                {
                    var conflict = await CheckConflictsAsync(
                        lesson.TeacherId,
                        enrollment.StudentId,
                        room,
                        newStartsAt,
                        newEndsAt,
                        lesson.Id);

                    if (conflict != null)
                        return conflict;

                    var lessonChange = new LessonChange(
                        lesson,
                        lesson.StartsAt,
                        newStartsAt,
                        lesson.Room,
                        room,
                        request.Reason);

                    lesson.Reschedule(newStartsAt, room);

                    await _lessonRepository.UpdateWithHistoryAsync(lesson, lessonChange);

                    return new LessonResponseDTO
                    {
                        Message = "Lesson rescheduled successfully",
                        Status = ResponseStatus.Success,
                        Data = new
                        {
                            id = lesson.Id,
                            startsAt = lesson.StartsAt,
                            endsAt = lesson.EndsAt,
                            room = lesson.Room,
                            status = lesson.Status.ToString()
                        }
                    };
                });
            }
            catch (ConcurrencyConflictException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (ArgumentException ex)
            {
                return Error(ex.Message, ResponseStatus.InvalidArgument);
            }
            catch (InvalidOperationException ex)
            {
                return Error(ex.Message, ResponseStatus.Conflict);
            }
            catch (Exception)
            {
                return Error("An unexpected error occurred", ResponseStatus.Error);
            }
        }

        private async Task<LessonResponseDTO?> CheckConflictsAsync(
            Guid teacherId,
            Guid studentId,
            string room,
            DateTime startsAt,
            DateTime endsAt,
            Guid? excludeLessonId = null)
        {
            if (await _lessonRepository.HasTeacherConflictAsync(
                teacherId, startsAt, endsAt, excludeLessonId))
            {
                return Error(
                    "Conflict: teacher - Teacher already has a lesson at this time",
                    ResponseStatus.Conflict);
            }

            if (await _lessonRepository.HasStudentConflictAsync(
                studentId, startsAt, endsAt, excludeLessonId))
            {
                return Error(
                    "Conflict: student - Student already has a lesson at this time",
                    ResponseStatus.Conflict);
            }

            if (await _lessonRepository.HasRoomConflictAsync(
                room, startsAt, endsAt, excludeLessonId))
            {
                return Error(
                    "Conflict: room - Room is already booked at this time",
                    ResponseStatus.Conflict);
            }

            return null;
        }

        private static bool TeachesInstrument(Teacher teacher, string instrumentName) =>
            teacher.Specialties.Any(s =>
                !string.IsNullOrWhiteSpace(s) &&
                string.Equals(
                    s.Trim(),
                    instrumentName.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        private static LessonResponseDTO? ValidateDurationAndRoom(
            int durationMinutes,
            string? room)
        {
            if (durationMinutes < MinDurationMinutes ||
                durationMinutes > MaxDurationMinutes)
            {
                return Error(
                    $"Lesson duration must be between {MinDurationMinutes} and {MaxDurationMinutes} minutes",
                    ResponseStatus.InvalidArgument);
            }

            if (string.IsNullOrWhiteSpace(room))
                return Error("Room is required", ResponseStatus.InvalidArgument);

            if (room.Trim().Length > MaxRoomLength)
                return Error($"Room cannot exceed {MaxRoomLength} characters", ResponseStatus.InvalidArgument);

            return null;
        }

        private static LessonResponseDTO? ValidateReason(string? reason, string action)
        {
            var trimmed = reason?.Trim() ?? string.Empty;

            if (trimmed.Length < MinReasonLength)
            {
                return Error(
                    $"{action} reason must have at least {MinReasonLength} characters",
                    ResponseStatus.InvalidArgument);
            }

            if (trimmed.Length > MaxReasonLength)
            {
                return Error(
                    $"{action} reason cannot exceed {MaxReasonLength} characters",
                    ResponseStatus.InvalidArgument);
            }

            return null;
        }

        private static LessonResponseDTO Error(
            string message,
            string status) => new()
            {
                Message = message,
                Status = status,
                Data = null
            };
    }
}
