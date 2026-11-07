using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
    public class TeacherRequestDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100,
            ErrorMessage = "Email must be at most 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(20, MinimumLength = 10,
            ErrorMessage = "Phone must be between 10 and 20 characters")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF is required")]
        [StringLength(14, MinimumLength = 11,
            ErrorMessage = "CPF must contain 11 digits, with optional punctuation")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialties are required")]
        [MinLength(1, ErrorMessage = "At least one specialty is required")]
        public List<string> Specialties { get; set; } = new();
    }
}