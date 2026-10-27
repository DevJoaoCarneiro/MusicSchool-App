
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly AppDbContext _context;

        public EnrollmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Enrollment> AddAsync(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return enrollment;
        }

        public async Task<Enrollment?> GetByIdAsync(Guid id)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Instrument)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> HasActiveEnrollmentAsync(
            Guid studentId,
            Guid instrumentId)
        {
            return await _context.Enrollments.AnyAsync(e =>
                e.StudentId == studentId &&
                e.InstrumentId == instrumentId &&
                e.Status == EnrollmentStatus.Active);
        }

        public async Task<bool> HasActiveEnrollmentsByStudentAsync(
            Guid studentId)
        {
            return await _context.Enrollments.AnyAsync(e =>
                e.StudentId == studentId &&
                e.Status == EnrollmentStatus.Active);
        }

        public async Task<(List<Enrollment> Items, int Total)> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            EnrollmentStatus? status,
            int page,
            int pageSize)
        {
            var query = _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Instrument)
                .AsQueryable();

            if (studentId.HasValue)
            {
                query = query.Where(e => e.StudentId == studentId.Value);
            }

            if (instrumentId.HasValue)
            {
                query = query.Where(e => e.InstrumentId == instrumentId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(e => e.Status == status.Value);
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.StartDate)
                .ThenBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
