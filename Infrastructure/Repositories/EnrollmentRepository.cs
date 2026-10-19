using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
