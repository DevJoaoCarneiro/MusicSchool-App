using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class CancelLessonSeriesRequestDTO
    {
        [Required(ErrorMessage = "Cancellation reason is required")]
        [MinLength(5, ErrorMessage = "Cancellation reason must have at least 5 characters")]
        [MaxLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
