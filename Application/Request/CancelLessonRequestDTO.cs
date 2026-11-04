
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CancelLessonRequestDTO
    {
        [Required(ErrorMessage = "Cancellation reason is required")]
        [MinLength(5, ErrorMessage = "Cancellation reason must have at least 5 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
