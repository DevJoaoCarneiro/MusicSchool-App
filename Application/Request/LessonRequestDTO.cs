
namespace Application.DTOs
{
    public class LessonRequestDTO
    {
        public Guid EnrollmentId { get; set; }
        public Guid TeacherId { get; set; }
        public DateTime StartsAt { get; set; }
        public int DurationMinutes { get; set; }
        public string Room { get; set; } = string.Empty;
    }
}
