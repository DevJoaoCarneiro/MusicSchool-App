using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface ITuitionRepository
    {
        Task<Tuition?> GetByIdAsync(Guid id);

        Task UpdateAsync(Tuition tuition);

        Task<HashSet<Guid>> GetEnrollmentIdsByReferenceMonthAsync(
            string referenceMonth);

        Task AddRangeAsync(IEnumerable<Tuition> tuitions);
    }
}