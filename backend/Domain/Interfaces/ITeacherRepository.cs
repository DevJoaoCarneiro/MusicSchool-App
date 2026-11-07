using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public interface ITeacherRepository
    {
        Task<Teacher> AddAsync(Teacher teacher);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByCpfAsync(string cpf);

        Task<bool> ExistsByEmailExceptIdAsync(string email, Guid id);

        Task<bool> ExistsByCpfExceptIdAsync(string cpf, Guid id);

        Task<(List<TResult> Items, int Total)> GetAllAsync<TResult>(
            string? name,
            int page,
            int pageSize,
            Expression<Func<Teacher, TResult>> selector);

        Task<Teacher?> GetByIdAsync(Guid id);

        Task<Teacher?> GetByEmailAsync(string email);

        Task<TResult?> GetByIdAsync<TResult>(
            Guid id,
            Expression<Func<Teacher, TResult>> selector);

        Task UpdateAsync(Teacher teacher);

        Task DeleteAsync(Teacher teacher);

        Task<bool> HasActiveLessonsAsync(Guid teacherId);

        Task<bool> HasAnyLessonHistoryAsync(Guid teacherId);

        Task RenameSpecialtyAsync(string oldName, string newName);
    }
}
