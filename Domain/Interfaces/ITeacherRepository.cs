using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface ITeacherRepository
    {
        Task<Teacher> AddAsync(Teacher teacher);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByCpfAsync(string cpf);

        Task<(List<TResult> Items, int Total)> GetAllAsync<TResult>(
    string? name,
    int page,
    int pageSize,
    Expression<Func<Teacher, TResult>> selector
);

        Task<Teacher?> GetByIdAsync(Guid id);

        Task UpdateAsync(Teacher teacher);

        Task DeleteAsync(Teacher teacher);

        Task<bool> HasActiveLessonsAsync(Guid teacherId);
    }
}