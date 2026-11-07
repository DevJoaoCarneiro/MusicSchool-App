using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Request
{
    public class GuardianRequestDTO
    {
        [Required(ErrorMessage = "Nome do responsável é obrigatório")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Nome do responsável deve ter entre 2 e 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone do responsável é obrigatório")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail do responsável é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail do responsável em formato inválido")]
        [StringLength(100, ErrorMessage = "E-mail do responsável deve ter no máximo 100 caracteres")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF do responsável é obrigatório")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF do responsável deve ter exatamente 11 dígitos")]
        public string Cpf { get; set; } = string.Empty;
    }
}
