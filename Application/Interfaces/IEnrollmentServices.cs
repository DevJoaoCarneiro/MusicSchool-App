
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IEnrollmentServices
    {
        Task<EnrollmentResponseDTO> CreateEnrollmentAsync(
            EnrollmentRequestDTO request);

        Task<EnrollmentListResponseDTO> GetAllAsync(
            Guid? studentId,
            Guid? instrumentId,
            string? status,
            int page,
            int pageSize);
    }
}
