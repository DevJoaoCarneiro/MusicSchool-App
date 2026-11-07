using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
	public class StudentRequestDTO
	{
		[Required(ErrorMessage = "Name is required")]
		[StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		[StringLength(100, ErrorMessage = "Email must be at most 100 characters")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Phone is required")]
		public string Phone { get; set; } = string.Empty;

		[Required(ErrorMessage = "CPF is required")]
		[RegularExpression(@"^\d{11}$", ErrorMessage = "CPF must contain exactly 11 digits")]
		public string Cpf { get; set; } = string.Empty;

		[Required(ErrorMessage = "Birth date is required")]
		public DateTime? BirthDate { get; set; }

		[Required(ErrorMessage = "Address is required")]
		public AddressRequestDTO? Address { get; set; }

		public GuardianRequestDTO? Guardian { get; set; }
	}
}