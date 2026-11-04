namespace Application.DTOs
{
    public class CreateRecurringLessonDTO
    {
        public Guid EnrollmentId { get; set; }

        public Guid TeacherId { get; set; }

        public string Weekday { get; set; } = string.Empty;

        public string Time { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        public string Room { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}