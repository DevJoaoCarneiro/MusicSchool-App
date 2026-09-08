using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student> AddAsync(Student student);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByCpfAsync(string cpf);

        Task<Student?> GetByIdAsync(Guid id);

        Task<(List<Student> Students, int Total)> GetAllAsync(
            string? name,
            int page,
            int pageSize
        );

        Task<bool> ExistsByEmailExceptIdAsync(
            string email,
            Guid id
        );

        Task<bool> ExistsByCpfExceptIdAsync(
            string cpf,
            Guid id
        );

       
        Task<Student> UpdateAsync(Student student);
        
    }
}
