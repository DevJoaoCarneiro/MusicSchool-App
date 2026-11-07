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
                return Error("Corpo da requisição é obrigatório", ResponseStatus.InvalidArgument);

            if (request.EnrollmentId == Guid.Empty)
                return Error("Matrícula é obrigatória", ResponseStatus.InvalidArgument);

            if (request.TeacherId == Guid.Empty)
                return Error("Professor é obrigatório", ResponseStatus.InvalidArgument);

            var startsAt = Normalizer.LocalDateTime(request.StartsAt);

            if (startsAt <= DateTime.Now)
                return Error("A data da aula deve estar no futuro", ResponseStatus.InvalidArgument);

            var basicValidation = ValidateDurationAndRoom(request.DurationMinutes, request.Room);

            if (basicValidation != null)
                return basicValidation;

            var room = request.Room.Trim();

            try
            {
                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(request.EnrollmentId);

                if (enrollment == null)
                    return Error("Matrícula não encontrada", ResponseStatus.NotFound);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("A matrícula não está ativa", ResponseStatus.Conflict);

                var teacher = await _teacherRepository
                    .GetByIdAsync(request.TeacherId);

                if (teacher == null)
                    return Error("Professor não encontrado", ResponseStatus.NotFound);

                if (!TeachesInstrument(teacher, enrollment.Instrument.Name))
                {
                    return Error(
                        "O professor não ensina o instrumento desta matrícula",
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
                        Message = "Aula agendada com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CreateRecurringAsync(
            CreateRecurringLessonDTO request)
        {
            if (request == null)
                return Error("Corpo da requisição é obrigatório", ResponseStatus.InvalidArgument);

            if (request.EnrollmentId == Guid.Empty)
                return Error("Matrícula é obrigatória", ResponseStatus.InvalidArgument);

            if (request.TeacherId == Guid.Empty)
                return Error("Professor é obrigatório", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Weekday) ||
                int.TryParse(request.Weekday, out _) ||
                !Enum.TryParse<DayOfWeek>(request.Weekday.Trim(), true, out var weekday) ||
                !Enum.IsDefined(weekday))
            {
                return Error("Dia da semana inválido", ResponseStatus.InvalidArgument);
            }

            if (!TimeSpan.TryParseExact(
                request.Time,
                @"hh\:mm",
                CultureInfo.InvariantCulture,
                out var lessonTime))
            {
                return Error("Horário inválido. Use o formato HH:mm", ResponseStatus.InvalidArgument);
            }

            var basicValidation = ValidateDurationAndRoom(request.DurationMinutes, request.Room);

            if (basicValidation != null)
                return basicValidation;

            var room = request.Room.Trim();

            var startDate = Normalizer.LocalDateTime(request.StartDate).Date;
            var endDate = Normalizer.LocalDateTime(request.EndDate).Date;

            if (endDate < startDate)
                return Error("A data final deve ser igual ou posterior à data inicial", ResponseStatus.InvalidArgument);

            if (endDate > startDate.AddMonths(MaxRecurringMonths))
                return Error("O período da recorrência não pode passar de seis meses", ResponseStatus.InvalidArgument);

            try
            {
                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(request.EnrollmentId);

                if (enrollment == null)
                    return Error("Matrícula não encontrada", ResponseStatus.NotFound);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("A matrícula não está ativa", ResponseStatus.Conflict);

                var teacher = await _teacherRepository
                    .GetByIdAsync(request.TeacherId);

                if (teacher == null)
                    return Error("Professor não encontrado", ResponseStatus.NotFound);

                if (!TeachesInstrument(teacher, enrollment.Instrument.Name))
                {
                    return Error(
                        "O professor não ensina o instrumento desta matrícula",
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
                        "Nenhuma aula no período selecionado",
                        ResponseStatus.InvalidArgument);
                }

                var past = slots.FirstOrDefault(s => s.StartsAt <= DateTime.Now);

                if (past != default)
                {
                    return Error(
                        $"A aula de {past.StartsAt:dd/MM/yyyy} precisa estar no futuro",
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
                            Message = "Há conflitos de horário nas aulas recorrentes",
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
                        Message = "Aulas recorrentes agendadas com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
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
                    "As datas inicial e final são obrigatórias",
                    ResponseStatus.InvalidArgument);
            }

            var startDate = Normalizer.LocalDateTime(from.Value).Date;
            var endDate = Normalizer.LocalDateTime(to.Value).Date;

            if (endDate < startDate)
            {
                return Error(
                    "A data final deve ser igual ou posterior à data inicial",
                    ResponseStatus.InvalidArgument);
            }

            if ((endDate - startDate).TotalDays + 1 > MaxRangeDays)
            {
                return Error(
                    $"O período não pode passar de {MaxRangeDays} dias",
                    ResponseStatus.InvalidArgument);
            }

            if (teacherId == Guid.Empty)
                return Error("Professor inválido", ResponseStatus.InvalidArgument);

            if (studentId == Guid.Empty)
                return Error("Aluno inválido", ResponseStatus.InvalidArgument);

            try
            {
                if (teacherEmail != null)
                {
                    var callerTeacher = await _teacherRepository.GetByEmailAsync(teacherEmail);

                    if (callerTeacher == null)
                    {
                        return new LessonResponseDTO
                        {
                            Message = "Aulas listadas com sucesso",
                            Status = ResponseStatus.Success,
                            Data = Array.Empty<object>()
                        };
                    }

                    if (teacherId.HasValue && teacherId.Value != callerTeacher.Id)
                    {
                        return Error(
                            "Professores só podem ver as próprias aulas",
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
                    Message = "Aulas listadas com sucesso",
                    Status = ResponseStatus.Success,
                    Data = results
                };
            }
            catch (Exception)
            {
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> GetByIdAsync(
            Guid id,
            string? teacherEmail)
        {
            if (id == Guid.Empty)
                return Error("ID de aula inválido", ResponseStatus.InvalidArgument);

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
                    return Error("Aula não encontrada", ResponseStatus.NotFound);
                }

                return new LessonResponseDTO
                {
                    Message = "Aula encontrada com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CancelAsync(
            Guid id,
            CancelLessonRequestDTO request)
        {
            if (id == Guid.Empty)
                return Error("ID de aula inválido", ResponseStatus.InvalidArgument);

            if (request == null)
                return Error("Corpo da requisição é obrigatório", ResponseStatus.InvalidArgument);

            var reasonValidation = ValidateReason(request.Reason, "cancelamento");

            if (reasonValidation != null)
                return reasonValidation;

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Aula não encontrada", ResponseStatus.NotFound);

                lesson.Cancel(request.Reason);

                await _lessonRepository.UpdateAsync(lesson);

                return new LessonResponseDTO
                {
                    Message = "Aula cancelada com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CompleteAsync(Guid id)
        {
            if (id == Guid.Empty)
                return Error("ID de aula inválido", ResponseStatus.InvalidArgument);

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Aula não encontrada", ResponseStatus.NotFound);

                lesson.Complete();

                await _lessonRepository.UpdateAsync(lesson);

                return new LessonResponseDTO
                {
                    Message = "Aula concluída com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> CancelSeriesAsync(
            Guid seriesId,
            CancelLessonSeriesRequestDTO request)
        {
            if (seriesId == Guid.Empty)
                return Error("ID de série inválido", ResponseStatus.InvalidArgument);

            if (request == null)
                return Error("Corpo da requisição é obrigatório", ResponseStatus.InvalidArgument);

            var reasonValidation = ValidateReason(request.Reason, "cancelamento");

            if (reasonValidation != null)
                return reasonValidation;

            try
            {
                if (!await _lessonRepository.SeriesExistsAsync(seriesId))
                    return Error("Série de aulas não encontrada", ResponseStatus.NotFound);

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
                    Message = "Série de aulas cancelada com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
            }
        }

        public async Task<LessonResponseDTO> RescheduleAsync(
            Guid id,
            RescheduleLessonRequestDTO request)
        {
            if (id == Guid.Empty)
                return Error("ID de aula inválido", ResponseStatus.InvalidArgument);

            if (request == null)
                return Error("Corpo da requisição é obrigatório", ResponseStatus.InvalidArgument);

            var newStartsAt = Normalizer.LocalDateTime(request.NewStartsAt);

            if (newStartsAt <= DateTime.Now)
                return Error("A nova data da aula deve estar no futuro", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Room))
                return Error("Sala é obrigatória", ResponseStatus.InvalidArgument);

            var room = request.Room.Trim();

            if (room.Length > MaxRoomLength)
                return Error($"Sala deve ter no máximo {MaxRoomLength} caracteres", ResponseStatus.InvalidArgument);

            var reasonValidation = ValidateReason(request.Reason, "reagendamento");

            if (reasonValidation != null)
                return reasonValidation;

            try
            {
                var lesson = await _lessonRepository.GetByIdAsync(id);

                if (lesson == null)
                    return Error("Aula não encontrada", ResponseStatus.NotFound);

                if (lesson.Status != LessonStatus.Scheduled)
                {
                    return Error(
                        "Somente aulas agendadas podem ser reagendadas",
                        ResponseStatus.Conflict);
                }

                if (lesson.StartsAt <= DateTime.Now)
                {
                    return Error(
                        "Uma aula que já começou não pode ser reagendada",
                        ResponseStatus.Conflict);
                }

                var enrollment = await _enrollmentRepository
                    .GetByIdAsync(lesson.EnrollmentId);

                if (enrollment == null)
                    return Error("Matrícula não encontrada", ResponseStatus.NotFound);

                if (enrollment.Status != EnrollmentStatus.Active)
                    return Error("A matrícula não está ativa", ResponseStatus.Conflict);

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
                        Message = "Aula reagendada com sucesso",
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
                return Error("Ocorreu um erro inesperado", ResponseStatus.Error);
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
                    "Conflito de horário: o professor já tem aula neste horário",
                    ResponseStatus.Conflict);
            }

            if (await _lessonRepository.HasStudentConflictAsync(
                studentId, startsAt, endsAt, excludeLessonId))
            {
                return Error(
                    "Conflito de horário: o aluno já tem aula neste horário",
                    ResponseStatus.Conflict);
            }

            if (await _lessonRepository.HasRoomConflictAsync(
                room, startsAt, endsAt, excludeLessonId))
            {
                return Error(
                    "Conflito de horário: a sala já está ocupada neste horário",
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
                    $"A duração da aula deve estar entre {MinDurationMinutes} e {MaxDurationMinutes} minutos",
                    ResponseStatus.InvalidArgument);
            }

            if (string.IsNullOrWhiteSpace(room))
                return Error("Sala é obrigatória", ResponseStatus.InvalidArgument);

            if (room.Trim().Length > MaxRoomLength)
                return Error($"Sala deve ter no máximo {MaxRoomLength} caracteres", ResponseStatus.InvalidArgument);

            return null;
        }

        private static LessonResponseDTO? ValidateReason(string? reason, string action)
        {
            var trimmed = reason?.Trim() ?? string.Empty;

            if (trimmed.Length < MinReasonLength)
            {
                return Error(
                    $"Motivo do {action} deve ter pelo menos {MinReasonLength} caracteres",
                    ResponseStatus.InvalidArgument);
            }

            if (trimmed.Length > MaxReasonLength)
            {
                return Error(
                    $"Motivo do {action} deve ter no máximo {MaxReasonLength} caracteres",
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
