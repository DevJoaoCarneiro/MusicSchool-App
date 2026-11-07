using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Context;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TuitionRepository : ITuitionRepository
    {
        private readonly AppDbContext _context;

        public TuitionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Tuition?> GetByIdAsync(Guid id)
        {
            return await _context.Tuitions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> RegisterPaymentIfPendingAsync(Tuition tuition)
        {
            var affected = await _context.Tuitions
                .Where(t =>
                    t.Id == tuition.Id &&
                    t.Status == TuitionStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.Status, tuition.Status)
                    .SetProperty(t => t.PaidAt, tuition.PaidAt)
                    .SetProperty(t => t.PaidAmount, tuition.PaidAmount)
                    .SetProperty(t => t.Method, tuition.Method));

            return affected == 1;
        }

        public async Task<HashSet<Guid>> GetEnrollmentIdsByReferenceMonthAsync(
            string referenceMonth)
        {
            var ids = await _context.Tuitions
                .AsNoTracking()
                .Where(t => t.ReferenceMonth == referenceMonth)
                .Select(t => t.EnrollmentId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task AddRangeAsync(IEnumerable<Tuition> tuitions)
        {
            _context.Tuitions.AddRange(tuitions);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                PostgresErrors.Has(ex, PostgresErrors.UniqueViolation))
            {
                throw new ConcurrencyConflictException(
                    "As mensalidades deste mês já estão sendo geradas por outra requisição",
                    ex);
            }
        }

        public async Task<(List<Tuition> Items, int Total)> GetAllAsync(
            string? referenceMonth,
            TuitionStatus? status,
            Guid? enrollmentId,
            Guid? studentId,
            int page,
            int pageSize)
        {
            var query = _context.Tuitions
                .AsNoTracking()
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Student)
                .Include(t => t.Enrollment)
                    .ThenInclude(e => e.Instrument)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(referenceMonth))
                query = query.Where(t => t.ReferenceMonth == referenceMonth);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (enrollmentId.HasValue)
                query = query.Where(t => t.EnrollmentId == enrollmentId.Value);

            if (studentId.HasValue)
                query = query.Where(t => t.Enrollment.StudentId == studentId.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
