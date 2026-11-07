using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ITuitionRepository
    {
        Task<Tuition?> GetByIdAsync(Guid id);

        Task<bool> RegisterPaymentIfPendingAsync(Tuition tuition);

        Task<HashSet<Guid>> GetEnrollmentIdsByReferenceMonthAsync(
            string referenceMonth);

        Task AddRangeAsync(IEnumerable<Tuition> tuitions);

        Task<(List<Tuition> Items, int Total)> GetAllAsync(
            string? referenceMonth,
            TuitionStatus? status,
            Guid? enrollmentId,
            Guid? studentId,
            int page,
            int pageSize);
    }
}
