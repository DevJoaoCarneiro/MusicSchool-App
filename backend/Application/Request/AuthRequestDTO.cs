using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class AuthRequestDTO
    {
        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(6, ErrorMessage = "E-mail ou senha inválidos")]
public string Password { get; set; } = string.Empty;
    }
}
