using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITeacherRepository
    {
        Task<Teacher> AddAsync(Teacher teacher);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByCpfAsync(string cpf);

        Task<(List<Teacher> Items, int Total)> GetAllAsync(
            string? name,
            int page,
            int pageSize);

        Task<Teacher?> GetByIdAsync(Guid id);

        Task UpdateAsync(Teacher teacher);

        Task DeleteAsync(Teacher teacher);

        //Task<bool> HasActiveClassesAsync(Guid teacherId);
    }
}