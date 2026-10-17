using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
