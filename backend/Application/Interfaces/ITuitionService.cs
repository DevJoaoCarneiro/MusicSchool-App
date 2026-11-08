using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface ITuitionService
    {
        Task<GenerateTuitionsResponseDTO> GenerateAsync(
            GenerateTuitionsRequestDTO request);

        Task<RegisterTuitionPaymentResponseDTO> RegisterPaymentAsync(
            Guid id,
            RegisterTuitionPaymentRequestDTO request);

        Task<TuitionListResponseDTO> GetAllAsync(
            string? referenceMonth,
            string? status,
            Guid? enrollmentId,
            Guid? studentId,
            int page,
            int pageSize);
    }
}
