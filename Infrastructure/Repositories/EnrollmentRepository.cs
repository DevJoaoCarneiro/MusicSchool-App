using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Persistence;
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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                PostgresErrors.Has(ex, PostgresErrors.UniqueViolation))
            {
                throw new ConcurrencyConflictException(
                    "Student already has an active enrollment for this instrument",
                    ex);
            }

            return enrollment;
        }

        public async Task<Enrollment?> GetByIdAsync(Guid id)
        {
            return await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Instrument)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(Enrollment enrollment)
        {
            _context.Entry(enrollment).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Enrollment>> GetBillableForMonthAsync(
            DateTime monthStart,
            DateTime monthEnd)
        {
            return await _context.Enrollments
                .AsNoTracking()
                .Where(e =>
                    e.StartDate <= monthEnd &&
                    (e.Status == EnrollmentStatus.Active ||
                     (e.EndDate != null && e.EndDate >= monthStart)))
                .OrderBy(e => e.Id)
                .ToListAsync();
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

        public async Task<bool> HasAnyEnrollmentByStudentAsync(Guid studentId)
        {
            return await _context.Enrollments.AnyAsync(e =>
                e.StudentId == studentId);
        }

        public async Task<bool> HasLessonWithTeacherAsync(
            Guid enrollmentId,
            Guid teacherId)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.EnrollmentId == enrollmentId &&
                l.TeacherId == teacherId);
        }

        public async Task<(List<Enrollment> Items, int Total)> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            EnrollmentStatus? status,
            Guid? teacherId,
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

            if (teacherId.HasValue)
            {
                query = query.Where(e => _context.Lessons.Any(l =>
                    l.EnrollmentId == e.Id &&
                    l.TeacherId == teacherId.Value));
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
