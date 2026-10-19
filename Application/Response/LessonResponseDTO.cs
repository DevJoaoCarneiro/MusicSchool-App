
namespace Application.DTOs
{
    public class LessonResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
