using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task<(List<TResult> Items, int Total)> GetAllAsync<TResult>(
    string? name,
    int page,
    int pageSize,
    Expression<Func<Teacher, TResult>> selector)
        {
            var query = _context.Teachers
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var search = name.Trim();

                query = query.Where(t =>
                    EF.Functions.ILike(t.Name, $"%{search}%"));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            return (items, total);
        }
        public async Task<Teacher?> GetByIdAsync(Guid id)
        {
            return await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TResult?> GetByIdAsync<TResult>(
            Guid id,
            Expression<Func<Teacher, TResult>> selector)
        {
            return await _context.Teachers
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(selector)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Teacher teacher)
        {
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasActiveLessonsAsync(Guid teacherId)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.TeacherId == teacherId &&
                l.Status == LessonStatus.Scheduled);
        }

        public async Task DeleteAsync(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }
}