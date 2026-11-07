
using System;

namespace Domain.Entities
{
    public enum LessonStatus
    {
        Scheduled = 1,
        Completed = 2,
        Cancelled = 3
    }

    public class Lesson
    {
        public Guid Id { get; private set; }

        public Guid EnrollmentId { get; private set; }
        public Enrollment Enrollment { get; private set; } = null!;
        public Guid? LessonSeriesId { get; private set; }

        public LessonSeries? LessonSeries { get; private set; }

        public Guid TeacherId { get; private set; }

        public DateTime StartsAt { get; private set; }

        public int DurationMinutes { get; private set; }

        public DateTime EndsAt => StartsAt.AddMinutes(DurationMinutes);

        public string Room { get; private set; } = string.Empty;

        public LessonStatus Status { get; private set; }

        public string? Reason { get; private set; }

        public DateTime? CancelledAt { get; private set; }

        // Constructor para o Entity Framework Core
        public Lesson()
        {
        }

        public Lesson(
            Enrollment enrollment,
            Guid teacherId,
            DateTime startsAt,
            int durationMinutes,
            string room)
        {
            if (enrollment == null)
                throw new ArgumentNullException(nameof(enrollment));

            if (enrollment.Status != EnrollmentStatus.Active)
                throw new ArgumentException(
                    "A matrícula não está ativa",
                    nameof(enrollment));

            if (teacherId == Guid.Empty)
                throw new ArgumentException(
                    "Professor é obrigatório",
                    nameof(teacherId));

            if (startsAt <= DateTime.Now)
                throw new ArgumentException(
                    "A data da aula deve estar no futuro",
                    nameof(startsAt));

            if (durationMinutes < 30 || durationMinutes > 120)
                throw new ArgumentOutOfRangeException(
                    nameof(durationMinutes),
                    "A duração da aula deve estar entre 30 e 120 minutos");

            if (string.IsNullOrWhiteSpace(room))
                throw new ArgumentException(
                    "Sala é obrigatória",
                    nameof(room));

            Id = Guid.NewGuid();
            EnrollmentId = enrollment.Id;
            Enrollment = enrollment;
            TeacherId = teacherId;
            StartsAt = startsAt;
            DurationMinutes = durationMinutes;
            Room = room.Trim();
            Status = LessonStatus.Scheduled;
        }

        public void Cancel(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
                throw new ArgumentException(
                    "Motivo do cancelamento deve ter pelo menos 5 caracteres",
                    nameof(reason));

            if (Status != LessonStatus.Scheduled)
                throw new InvalidOperationException(
                    "Somente aulas agendadas podem ser canceladas");

            if (StartsAt <= DateTime.Now)
                throw new InvalidOperationException(
                    "Uma aula que já começou não pode ser cancelada");

            Status = LessonStatus.Cancelled;
            Reason = reason.Trim();
            CancelledAt = DateTime.Now;
        }


        public void Complete()
        {
            if (Status != LessonStatus.Scheduled)
                throw new InvalidOperationException(
                    "Somente aulas agendadas podem ser concluídas");

            if (StartsAt > DateTime.Now)
                throw new InvalidOperationException(
                    "Uma aula que ainda não começou não pode ser concluída");

            Status = LessonStatus.Completed;
        }

        public void Reschedule(DateTime newStartsAt, string newRoom)
        {
            if (Status != LessonStatus.Scheduled)
                throw new InvalidOperationException(
                    "Somente aulas agendadas podem ser reagendadas");

            if (StartsAt <= DateTime.Now)
                throw new InvalidOperationException(
                    "Uma aula que já começou não pode ser reagendada");

            if (newStartsAt <= DateTime.Now)
                throw new ArgumentException(
                    "A nova data da aula deve estar no futuro",
                    nameof(newStartsAt));

            if (string.IsNullOrWhiteSpace(newRoom))
                throw new ArgumentException(
                    "Sala é obrigatória",
                    nameof(newRoom));

            if (newRoom.Trim().Length > 60)
                throw new ArgumentException(
                    "Sala deve ter no máximo 60 caracteres",
                    nameof(newRoom));

            StartsAt = newStartsAt;
            Room = newRoom.Trim();
        }

        public void AssociateWithSeries(LessonSeries series)
        {
            if (series == null)
                throw new ArgumentNullException(nameof(series));

            if (series.EnrollmentId != EnrollmentId)
                throw new ArgumentException(
                    "A matrícula não corresponde à série de aulas");

            if (series.TeacherId != TeacherId)
                throw new ArgumentException(
                    "O professor não corresponde à série de aulas");

            LessonSeriesId = series.Id;
            LessonSeries = series;
        }

    }
}
