using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class RescheduleLessonRequestDTO
    {
        [Required(ErrorMessage = "Nova data da aula é obrigatória")]
        public DateTime NewStartsAt { get; set; }

        [Required(ErrorMessage = "Sala é obrigatória")]
        [MaxLength(60, ErrorMessage = "Sala deve ter no máximo 60 caracteres")]
        public string Room { get; set; } = string.Empty;

        [Required(ErrorMessage = "Motivo é obrigatório")]
        [MinLength(5, ErrorMessage = "Motivo deve ter pelo menos 5 caracteres")]
        public string Reason { get; set; } = string.Empty;
    }
}