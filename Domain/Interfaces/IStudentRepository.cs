using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student> AddAsync(Student student);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByCpfAsync(string cpf);

        Task<Student?> GetByIdAsync(Guid id);

        Task<(List<TResult> Items, int Total)> GetAllAsync<TResult>(
    string? name,
    int page,
    int pageSize,
    Expression<Func<Student, TResult>> selector
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
        Task DeleteAsync(Student student);
    }
}
