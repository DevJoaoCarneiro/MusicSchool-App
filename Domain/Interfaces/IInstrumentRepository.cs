
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IInstrumentRepository
    {
        Task<Instrument> AddAsync(Instrument instrument);

        Task<List<Instrument>> GetAllAsync();

        Task<Instrument?> GetByIdAsync(Guid id);

        Task<Instrument?> GetByNameAsync(string name);

        Task<Instrument> UpdateAsync(Instrument instrument);

        Task<bool> DeleteAsync(Instrument instrument);

        Task<bool> HasActiveEnrollmentsAsync(Guid instrumentId);
    }
}
