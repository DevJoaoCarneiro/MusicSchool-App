using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface ITuitionRepository
    {
        Task<(List<Tuition> Items, int Total)> GetAllAsync(
            TuitionStatus? status,
            string? referenceMonth,
            Guid? studentId,
            int page,
            int pageSize);
    }
}
