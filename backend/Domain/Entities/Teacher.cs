using System;

namespace Domain.Entities
{
    public class Teacher
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Email { get; private set; } = string.Empty;

        public string Phone { get; private set; } = string.Empty;

        public string Cpf { get; private set; } = string.Empty;

        public List<string> Specialties { get; private set; } = new();

        public Teacher()
        {
        }

        public Teacher(
            string name,
            string email,
            string phone,
            string cpf,
            List<string> specialties)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            Phone = phone;
            Cpf = cpf;
            Specialties = specialties;
        }

        public void Update(
            string name,
            string email,
            string phone,
            string cpf,
            List<string> specialties)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Cpf = cpf;
            Specialties = specialties;
        }
    }
}