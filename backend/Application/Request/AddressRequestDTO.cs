using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Request
{
    public class AddressRequestDTO
    {
        [Required(ErrorMessage = "Rua é obrigatória")]
        [StringLength(100, ErrorMessage = "Rua deve ter no máximo 100 caracteres")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número é obrigatório")]
        [StringLength(20, ErrorMessage = "Número deve ter no máximo 20 caracteres")]
        public string Number { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cidade é obrigatória")]
        [StringLength(100, ErrorMessage = "Cidade deve ter no máximo 100 caracteres")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "UF é obrigatória")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "UF deve ter 2 caracteres")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "CEP é obrigatório")]
        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP em formato inválido")]
        public string ZipCode { get; set; } = string.Empty;
    }
}
