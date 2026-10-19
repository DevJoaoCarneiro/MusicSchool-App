using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Guardian
    {
        public string Name { get; private set; } = string.Empty;

        public string Phone { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string Cpf { get; private set; } = string.Empty;

        public Guardian()
        {
        }

        public Guardian(
            string name,
            string phone,
            string email,
            string cpf
        )
        {
            Name = name;
            Phone = phone;
            Email = email;
            Cpf = cpf;
        }
    }
}
