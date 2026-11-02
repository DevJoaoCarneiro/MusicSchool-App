
namespace Domain.Entities
{
    public class LessonSeries
    {
        public Guid Id { get; private set; }

        public Guid EnrollmentId { get; private set; }

        public Guid TeacherId { get; private set; }

        public DayOfWeek Weekday { get; private set; }

        public TimeSpan Time { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        public int DurationMinutes { get; private set; }

        public string Room { get; private set; } = string.Empty;

        public ICollection<Lesson> Lessons { get; private set; }
            = new List<Lesson>();

        // Constructor para Entity Framework Core
        public LessonSeries()
        {
        }

        public LessonSeries(
            Guid enrollmentId,
            Guid teacherId,
            DayOfWeek weekday,
            TimeSpan time,
            DateTime startDate,
            DateTime endDate,
            int durationMinutes,
            string room)
        {
            if (enrollmentId == Guid.Empty)
                throw new ArgumentException("Enrollment is required");

            if (teacherId == Guid.Empty)
                throw new ArgumentException("Teacher is required");

            if (endDate.Date < startDate.Date)
                throw new ArgumentException(
                    "End date must be on or after start date");

            if (durationMinutes < 30 || durationMinutes > 120)
                throw new ArgumentOutOfRangeException(
                    nameof(durationMinutes));

            if (time < TimeSpan.Zero || time >= TimeSpan.FromDays(1))
                throw new ArgumentException("Invalid lesson time");

            if (string.IsNullOrWhiteSpace(room))
                throw new ArgumentException("Room is required");

            if (room.Trim().Length > 60)
                throw new ArgumentException(
                    "Room cannot exceed 60 characters");

            Id = Guid.NewGuid();
            EnrollmentId = enrollmentId;
            TeacherId = teacherId;
            Weekday = weekday;
            Time = time;
            StartDate = startDate.Date;
            EndDate = endDate.Date;
            DurationMinutes = durationMinutes;
            Room = room.Trim();
        }
    }
}
