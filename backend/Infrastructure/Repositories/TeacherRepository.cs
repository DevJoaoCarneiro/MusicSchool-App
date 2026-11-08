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
            var normalized = email.Trim().ToLower();

            return await _context.Teachers
                .AnyAsync(t => t.Email.ToLower() == normalized);
        }

        public async Task<bool> ExistsByCpfAsync(string cpf)
        {
            return await _context.Teachers
                .AnyAsync(t => t.Cpf == cpf);
        }

        public async Task<bool> ExistsByEmailExceptIdAsync(string email, Guid id)
        {
            var normalized = email.Trim().ToLower();

            return await _context.Teachers
                .AnyAsync(t => t.Email.ToLower() == normalized && t.Id != id);
        }

        public async Task<bool> ExistsByCpfExceptIdAsync(string cpf, Guid id)
        {
            return await _context.Teachers
                .AnyAsync(t => t.Cpf == cpf && t.Id != id);
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
                .ThenBy(t => t.Id)
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

        public async Task<Teacher?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();

            return await _context.Teachers
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Email.ToLower() == normalized);
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

        public async Task<bool> HasAnyLessonHistoryAsync(Guid teacherId)
        {
            return await _context.Lessons.AnyAsync(l => l.TeacherId == teacherId) ||
                   await _context.LessonSeries.AnyAsync(s => s.TeacherId == teacherId);
        }

        public async Task RenameSpecialtyAsync(string oldName, string newName)
        {
            var normalizedOld = oldName.Trim().ToLower();

            var teachers = await _context.Teachers
                .Where(t => t.Specialties.Any(s => s.ToLower() == normalizedOld))
                .ToListAsync();

            foreach (var teacher in teachers)
            {
                var specialties = teacher.Specialties
                    .Select(s => string.Equals(
                        s.Trim(),
                        oldName.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                        ? newName
                        : s)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                teacher.Update(
                    teacher.Name,
                    teacher.Email,
                    teacher.Phone,
                    teacher.Cpf,
                    specialties);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }
}
