using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Request
{
    public class GuardianRequestDTO
    {
        [Required(ErrorMessage = "Guardian name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Guardian name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guardian phone is required")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guardian email is required")]
        [EmailAddress(ErrorMessage = "Invalid guardian email format")]
        [StringLength(100, ErrorMessage = "Guardian email must be at most 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Guardian CPF is required")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Guardian CPF must contain exactly 11 digits")]
        public string Cpf { get; set; } = string.Empty;
    }
}
