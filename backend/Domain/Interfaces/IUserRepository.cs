using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);

        Task<bool> ExistsByEmailAsync(string email);

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByIdAsync(Guid id);

        Task<(List<User> Items, int Total)> GetAllAsync(int page, int pageSize);

        Task UpdateAsync(User user);
    }
}
