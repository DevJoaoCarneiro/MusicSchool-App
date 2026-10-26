using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _context;

        public TeacherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Teacher> AddAsync(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return teacher;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Teachers
                .AnyAsync(t => t.Email == email);
        }

        public async Task<bool> ExistsByCpfAsync(string cpf)
        {
            return await _context.Teachers
                .AnyAsync(t => t.Cpf == cpf);
        }

        public async Task<(List<Teacher> Items, int Total)> GetAllAsync(
            string? name,
            int page,
            int pageSize)
        {
            var query = _context.Teachers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(t =>
                EF.Functions.ILike(t.Name, $"%{name.Trim()}%"));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
        public async Task<Teacher?> GetByIdAsync(Guid id)
        {
            return await _context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(Teacher teacher)
        {
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }
}