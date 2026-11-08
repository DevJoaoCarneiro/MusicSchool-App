using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Request
{
	public class StudentRequestDTO
	{
		[Required(ErrorMessage = "Nome é obrigatório")]
		[StringLength(100, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 100 caracteres")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "E-mail é obrigatório")]
		[EmailAddress(ErrorMessage = "E-mail em formato inválido")]
		[StringLength(100, ErrorMessage = "E-mail deve ter no máximo 100 caracteres")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Telefone é obrigatório")]
		public string Phone { get; set; } = string.Empty;

		[Required(ErrorMessage = "CPF é obrigatório")]
		[RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve ter exatamente 11 dígitos")]
		public string Cpf { get; set; } = string.Empty;

		[Required(ErrorMessage = "Data de nascimento é obrigatória")]
		public DateTime? BirthDate { get; set; }

		[Required(ErrorMessage = "Endereço é obrigatório")]
		public AddressRequestDTO? Address { get; set; }

		public GuardianRequestDTO? Guardian { get; set; }
	}
}