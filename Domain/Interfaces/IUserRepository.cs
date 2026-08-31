using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);

        Task<bool> ExistsByEmailAsync(string email);
    }
}
