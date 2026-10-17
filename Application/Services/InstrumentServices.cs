
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class InstrumentServices : IInstrumentServices
    {
        private readonly IInstrumentRepository _repository;

        public InstrumentServices(IInstrumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<InstrumentResponseDTO> CreateAsync(
            InstrumentRequestDTO request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Name))
                {
                    return Result(
                        "Instrument name is required",
                        "BadRequest");
                }

                var name = request.Name.Trim();
                var description = request.Description?.Trim() ?? string.Empty;

                if (name.Length > 60)
                {
                    return Result(
                        "Instrument name cannot exceed 60 characters",
                        "BadRequest");
                }

                var existing = await _repository.GetByNameAsync(name);

                if (existing != null)
                {
                    return Result(
                        "Instrument name already exists",
                        "Conflict");
                }

                var instrument = new Instrument(name, description);

                await _repository.AddAsync(instrument);

                return Result(
                    "Instrument created successfully",
                    "Success",
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    "Error");
            }
        }

        public async Task<InstrumentResponseDTO> GetAllAsync()
        {
            try
            {
                var instruments = await _repository.GetAllAsync();

                var data = instruments.Select(ToData).ToList();

                return Result(
                    "Instruments retrieved successfully",
                    "Success",
                    data);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    "Error");
            }
        }

        public async Task<InstrumentResponseDTO> GetByIdAsync(Guid id)
        {
            try
            {
                var instrument = await _repository.GetByIdAsync(id);

                if (instrument == null)
                {
                    return Result(
                        "Instrument not found",
                        "NotFound");
                }

                return Result(
                    "Instrument retrieved successfully",
                    "Success",
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    "Error");
            }
        }

        public async Task<InstrumentResponseDTO> UpdateAsync(
            Guid id,
            InstrumentRequestDTO request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Name))
                {
                    return Result(
                        "Instrument name is required",
                        "BadRequest");
                }

                var name = request.Name.Trim();
                var description = request.Description?.Trim() ?? string.Empty;

                if (name.Length > 60)
                {
                    return Result(
                        "Instrument name cannot exceed 60 characters",
                        "BadRequest");
                }

                var instrument = await _repository.GetByIdAsync(id);

                if (instrument == null)
                {
                    return Result(
                        "Instrument not found",
                        "NotFound");
                }

                var existing = await _repository.GetByNameAsync(name);

                if (existing != null && existing.Id != id)
                {
                    return Result(
                        "Instrument name already exists",
                        "Conflict");
                }

                instrument.Update(name, description);

                await _repository.UpdateAsync(instrument);

                return Result(
                    "Instrument updated successfully",
                    "Success",
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    "Error");
            }
        }

        public async Task<InstrumentResponseDTO> DeleteAsync(Guid id)
        {
            try
            {
                var instrument = await _repository.GetByIdAsync(id);

                if (instrument == null)
                {
                    return Result(
                        "Instrument not found",
                        "NotFound");
                }

                if (await _repository.HasActiveEnrollmentsAsync(id))
                {
                    return Result(
                        "Instrument has active enrollments",
                        "Conflict");
                }

                var deleted = await _repository.DeleteAsync(instrument);

                if (!deleted)
                {
                    return Result(
                        "Instrument could not be deleted",
                        "Error");
                }

                return Result(
                    "Instrument deleted successfully",
                    "Success");
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    "Error");
            }
        }

        private static InstrumentData ToData(Instrument instrument)
        {
            return new InstrumentData
            {
                Id = instrument.Id,
                Name = instrument.Name,
                Description = instrument.Description
            };
        }

        private static InstrumentResponseDTO Result(
            string message,
            string status,
            object? data = null)
        {
            return new InstrumentResponseDTO
            {
                Message = message,
                Status = status,
                Data = data
            };
        }
    }
}
