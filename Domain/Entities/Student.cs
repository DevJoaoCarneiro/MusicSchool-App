using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Student
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string Phone { get; private set; } = string.Empty;

        public string Cpf { get; private set; } = string.Empty;

        public DateTime BirthDate { get; private set; }

        public Address Address { get; private set; } = null!;
        
        public Guardian? Guardian { get; private set; }

        public Student()
        {
        }

        public Student(
            string name,
            string email,
            string phone,
            string cpf,
            DateTime birthDate,
            Address address,
            Guardian? guardian
        )
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Phone = phone;
            Cpf = cpf;
            BirthDate = birthDate;
            Address = address;
            Guardian = guardian;
        }
    }
}
