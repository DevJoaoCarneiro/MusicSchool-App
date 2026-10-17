
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IInstrumentServices
    {
        Task<InstrumentResponseDTO> CreateAsync(InstrumentRequestDTO request);

        Task<InstrumentResponseDTO> GetAllAsync();

        Task<InstrumentResponseDTO> GetByIdAsync(Guid id);

        Task<InstrumentResponseDTO> UpdateAsync(
            Guid id,
            InstrumentRequestDTO request);

        Task<InstrumentResponseDTO> DeleteAsync(Guid id);
    }
}
