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
    }
}