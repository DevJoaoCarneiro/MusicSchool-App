using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IEnrollmentServices
    {
        Task<EnrollmentResponseDTO> CreateEnrollmentAsync(
            EnrollmentRequestDTO request);

        Task<CloseEnrollmentResponseDTO> CloseEnrollmentAsync(
            Guid id,
            CloseEnrollmentRequestDTO request);

        Task<EnrollmentResponseDTO> GetByIdAsync(
            Guid id,
            string? teacherEmail);

        Task<EnrollmentListResponseDTO> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            string? status,
            string? teacherEmail,
            int page,
            int pageSize);
    }
}
