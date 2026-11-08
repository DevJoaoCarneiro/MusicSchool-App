using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Senha atual é obrigatória")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova senha é obrigatória")]
        [MinLength(6, ErrorMessage = "Nova senha deve ter pelo menos 6 caracteres")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
