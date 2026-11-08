using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task<Enrollment> AddAsync(Enrollment enrollment);

        Task<Enrollment?> GetByIdAsync(Guid id);

        Task UpdateAsync(Enrollment enrollment);

        Task<bool> HasActiveEnrollmentAsync(
            Guid studentId,
            Guid instrumentId);

        Task<bool> HasActiveEnrollmentsByStudentAsync(
            Guid studentId);

        Task<bool> HasAnyEnrollmentByStudentAsync(Guid studentId);

        Task<bool> HasLessonWithTeacherAsync(
            Guid enrollmentId,
            Guid teacherId);

        Task<(List<Enrollment> Items, int Total)> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            EnrollmentStatus? status,
            Guid? teacherId,
            int page,
            int pageSize);

        Task<List<Enrollment>> GetBillableForMonthAsync(
            DateTime monthStart,
            DateTime monthEnd);
    }
}
