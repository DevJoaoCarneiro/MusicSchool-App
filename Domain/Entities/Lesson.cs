
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

        public Guid TeacherId { get; private set; }

        public DateTime StartsAt { get; private set; }

        public int DurationMinutes { get; private set; }

        public DateTime EndsAt => StartsAt.AddMinutes(DurationMinutes);

        public string Room { get; private set; } = string.Empty;

        public LessonStatus Status { get; private set; }

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
                    "Enrollment is not active",
                    nameof(enrollment));

            if (teacherId == Guid.Empty)
                throw new ArgumentException(
                    "Teacher is required",
                    nameof(teacherId));

            if (startsAt <= DateTime.Now)
                throw new ArgumentException(
                    "Lesson date must be in the future",
                    nameof(startsAt));

            if (durationMinutes < 30 || durationMinutes > 120)
                throw new ArgumentOutOfRangeException(
                    nameof(durationMinutes),
                    "Lesson duration must be between 30 and 120 minutes");

            if (string.IsNullOrWhiteSpace(room))
                throw new ArgumentException(
                    "Room is required",
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
    }
}
