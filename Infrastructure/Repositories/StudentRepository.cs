using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student> AddAsync(Student student)
        {
            _context.Students.Add(student);

            await _context.SaveChangesAsync();

            return student;
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Students
                .AnyAsync(s => s.Email == email);
        }

        public async Task<bool> ExistsByCpfAsync(string cpf)
        {
            return await _context.Students
                .AnyAsync(s => s.Cpf == cpf);
        }

        public async Task<Student?> GetByIdAsync(Guid id)
        {
            return await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<(List<TResult> Items, int Total)> GetAllAsync<TResult>(
    string? name,
    int page,
    int pageSize,
    Expression<Func<Student, TResult>> selector)
        {
            var query = _context.Students
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                var search = name.Trim().ToLower();

                query = query.Where(s =>
                    s.Name.ToLower().Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> ExistsByEmailExceptIdAsync(
            string email,
            Guid id)
        {
            return await _context.Students
                .AnyAsync(s => s.Email == email && s.Id != id);
        }

        
        public async Task<bool> ExistsByCpfExceptIdAsync(
            string cpf,
            Guid id)
        {
            return await _context.Students
                .AnyAsync(s => s.Cpf == cpf && s.Id != id);
        }

       
        public async Task<Student> UpdateAsync(Student student)
        {
            _context.Students.Update(student);

            await _context.SaveChangesAsync();

            return student;
        }

        public async Task DeleteAsync(Student student)
        {
            _context.Students.Remove(student);

            await _context.SaveChangesAsync();
        }
    }
}