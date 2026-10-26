using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IEnrollmentRepository
    {
        Task<Enrollment> AddAsync(Enrollment enrollment);

        Task<Enrollment?> GetByIdAsync(Guid id);

        Task<bool> HasActiveEnrollmentAsync(
            Guid studentId,
            Guid instrumentId);

        Task<bool> HasActiveEnrollmentsByStudentAsync(
            Guid studentId);

        Task<(List<Enrollment> Items, int Total)> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            EnrollmentStatus? status,
            int page,
            int pageSize);
    }
}
