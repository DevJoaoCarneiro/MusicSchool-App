using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class CancelLessonSeriesRequestDTO
    {
        [Required(ErrorMessage = "Motivo do cancelamento é obrigatório")]
        [MinLength(5, ErrorMessage = "Motivo do cancelamento deve ter pelo menos 5 caracteres")]
        [MaxLength(500, ErrorMessage = "Motivo do cancelamento deve ter no máximo 500 caracteres")]
        public string Reason { get; set; } = string.Empty;
    }
}
