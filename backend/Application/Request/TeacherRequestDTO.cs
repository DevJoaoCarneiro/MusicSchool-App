using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class TeacherRequestDTO
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail em formato inválido")]
        [StringLength(100,
            ErrorMessage = "E-mail deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [StringLength(20, MinimumLength = 10,
            ErrorMessage = "Telefone deve ter entre 10 e 20 caracteres")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(14, MinimumLength = 11,
            ErrorMessage = "CPF deve ter 11 dígitos, com ou sem pontuação")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Especialidades são obrigatórias")]
        [MinLength(1, ErrorMessage = "Informe ao menos uma especialidade")]
        public List<string> Specialties { get; set; } = new();
    }
}