using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

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
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<(List<Student> Students, int Total)> GetAllAsync(
              string? name,
              int page,
              int pageSize)
        {
            var query = _context.Students.AsQueryable();

           
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(s =>
                    s.Name.ToLower().Contains(name.ToLower()));
            }

            
            var total = await query.CountAsync();

            
            var students = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (students, total);
        }
    }
}