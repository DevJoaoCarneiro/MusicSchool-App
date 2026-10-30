using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RescheduleLessonRequestDTO
    {
        [Required(ErrorMessage = "New lesson date is required")]
        public DateTime NewStartsAt { get; set; }

        [Required(ErrorMessage = "Room is required")]
        [MaxLength(60, ErrorMessage = "Room cannot exceed 60 characters")]
        public string Room { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reason is required")]
        [MinLength(5, ErrorMessage = "Reason must have at least 5 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}