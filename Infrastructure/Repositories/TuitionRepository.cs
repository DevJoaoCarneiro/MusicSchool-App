using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task<(List<Tuition> Items, int Total)> GetAllAsync(
            TuitionStatus? status,
            string? referenceMonth,
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

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(referenceMonth))
            {
                query = query.Where(
                    t => t.ReferenceMonth == referenceMonth);
            }

            if (studentId.HasValue)
            {
                query = query.Where(
                    t => t.Enrollment.StudentId == studentId.Value);
            }

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