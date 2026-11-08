using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class StudentResponseDTO
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public StudentData? Data { get; set; }
    }

    public class StudentData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public AddressData Address { get; set; } = null!;

        public GuardianData? Guardian { get; set; }
    }

    public class AddressData
    {
        public string Street { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;
    }

    public class GuardianData
    {
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;
    }
}
