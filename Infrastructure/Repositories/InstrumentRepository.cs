
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly AppDbContext _context;

        public InstrumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Instrument> AddAsync(Instrument instrument)
        {
            _context.Instruments.Add(instrument);
            await _context.SaveChangesAsync();

            return instrument;
        }

        public async Task<List<Instrument>> GetAllAsync()
        {
            return await _context.Instruments
                .AsNoTracking()
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Instrument?> GetByIdAsync(Guid id)
        {
            return await _context.Instruments
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Instrument?> GetByNameAsync(string name)
        {
            return await _context.Instruments
                .FirstOrDefaultAsync(i =>
                    i.Name.ToLower() == name.ToLower());
        }

        public async Task<Instrument> UpdateAsync(Instrument instrument)
        {
            _context.Instruments.Update(instrument);
            await _context.SaveChangesAsync();

            return instrument;
        }

        public async Task<bool> DeleteAsync(Instrument instrument)
        {
            _context.Instruments.Remove(instrument);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> HasActiveEnrollmentsAsync(Guid instrumentId)
        {
            // TODO: implementar quando conhecermos
            // a entidade e as regras de matrícula do projeto.
            throw new NotImplementedException();
        }
    }
}
