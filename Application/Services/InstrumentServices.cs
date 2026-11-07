using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class InstrumentServices : IInstrumentServices
    {
        private const int MaxNameLength = 60;
        private const int MaxDescriptionLength = 255;

        private readonly IInstrumentRepository _repository;
        private readonly ITeacherRepository _teacherRepository;

        public InstrumentServices(
            IInstrumentRepository repository,
            ITeacherRepository teacherRepository)
        {
            _repository = repository;
            _teacherRepository = teacherRepository;
        }

        public async Task<InstrumentResponseDTO> CreateAsync(
            InstrumentRequestDTO request)
        {
            try
            {
                var validation = Validate(request, out var name, out var description);

                if (validation != null)
                    return validation;

                var existing = await _repository.GetByNameAsync(name);

                if (existing != null)
                {
                    return Result(
                        "Instrument name already exists",
                        ResponseStatus.Conflict);
                }

                var instrument = new Instrument(name, description);

                await _repository.AddAsync(instrument);

                return Result(
                    "Instrument created successfully",
                    ResponseStatus.Success,
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    ResponseStatus.Error);
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
                    ResponseStatus.Success,
                    data);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    ResponseStatus.Error);
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
                        ResponseStatus.NotFound);
                }

                return Result(
                    "Instrument retrieved successfully",
                    ResponseStatus.Success,
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    ResponseStatus.Error);
            }
        }

        public async Task<InstrumentResponseDTO> UpdateAsync(
            Guid id,
            InstrumentRequestDTO request)
        {
            try
            {
                var validation = Validate(request, out var name, out var description);

                if (validation != null)
                    return validation;

                var instrument = await _repository.GetByIdAsync(id);

                if (instrument == null)
                {
                    return Result(
                        "Instrument not found",
                        ResponseStatus.NotFound);
                }

                var existing = await _repository.GetByNameAsync(name);

                if (existing != null && existing.Id != id)
                {
                    return Result(
                        "Instrument name already exists",
                        ResponseStatus.Conflict);
                }

                var oldName = instrument.Name;

                instrument.Update(name, description);

                await _repository.UpdateAsync(instrument);

                if (!string.Equals(oldName, name, StringComparison.Ordinal))
                {
                    await _teacherRepository.RenameSpecialtyAsync(oldName, name);
                }

                return Result(
                    "Instrument updated successfully",
                    ResponseStatus.Success,
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    ResponseStatus.Error);
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
                        ResponseStatus.NotFound);
                }

                if (await _repository.HasActiveEnrollmentsAsync(id))
                {
                    return Result(
                        "Instrument has active enrollments",
                        ResponseStatus.Conflict);
                }

                if (await _repository.HasAnyEnrollmentAsync(id))
                {
                    return Result(
                        "Instrument has enrollment history and cannot be deleted",
                        ResponseStatus.Conflict);
                }

                var deleted = await _repository.DeleteAsync(instrument);

                if (!deleted)
                {
                    return Result(
                        "Instrument could not be deleted",
                        ResponseStatus.Error);
                }

                return Result(
                    "Instrument deleted successfully",
                    ResponseStatus.Success);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred",
                    ResponseStatus.Error);
            }
        }

        private static InstrumentResponseDTO? Validate(
            InstrumentRequestDTO? request,
            out string name,
            out string description)
        {
            name = string.Empty;
            description = string.Empty;

            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                return Result(
                    "Instrument name is required",
                    ResponseStatus.InvalidArgument);
            }

            name = request.Name.Trim();
            description = request.Description?.Trim() ?? string.Empty;

            if (name.Length > MaxNameLength)
            {
                return Result(
                    $"Instrument name cannot exceed {MaxNameLength} characters",
                    ResponseStatus.InvalidArgument);
            }

            if (description.Length > MaxDescriptionLength)
            {
                return Result(
                    $"Instrument description cannot exceed {MaxDescriptionLength} characters",
                    ResponseStatus.InvalidArgument);
            }

            return null;
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
