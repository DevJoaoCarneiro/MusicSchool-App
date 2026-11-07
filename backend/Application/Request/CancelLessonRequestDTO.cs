
using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class CancelLessonRequestDTO
    {
        [Required(ErrorMessage = "Motivo do cancelamento é obrigatório")]
        [MinLength(5, ErrorMessage = "Motivo do cancelamento deve ter pelo menos 5 caracteres")]
        public string Reason { get; set; } = string.Empty;
    }
}
