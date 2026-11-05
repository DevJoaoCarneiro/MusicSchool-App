using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
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
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task UpdateAsync(Tuition tuition)
        {
            _context.Entry(tuition).State = EntityState.Modified;
            await _context.SaveChangesAsync();
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
            await _context.SaveChangesAsync();
        }
    }
}