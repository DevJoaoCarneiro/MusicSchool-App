using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();

            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == normalized);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<(List<User> Items, int Total)> GetAllAsync(
            int page,
            int pageSize)
        {
            var query = _context.Users.AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task UpdateAsync(User user)
        {
            if (_context.Entry(user).State == EntityState.Detached)
            {
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
        }
    }
}
