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
                        "Já existe um instrumento com este nome",
                        ResponseStatus.Conflict);
                }

                var instrument = new Instrument(name, description);

                await _repository.AddAsync(instrument);

                return Result(
                    "Instrumento criado com sucesso",
                    ResponseStatus.Success,
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado",
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
                    "Instrumentos listados com sucesso",
                    ResponseStatus.Success,
                    data);
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado",
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
                        "Instrumento não encontrado",
                        ResponseStatus.NotFound);
                }

                return Result(
                    "Instrumento encontrado com sucesso",
                    ResponseStatus.Success,
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado",
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
                        "Instrumento não encontrado",
                        ResponseStatus.NotFound);
                }

                var existing = await _repository.GetByNameAsync(name);

                if (existing != null && existing.Id != id)
                {
                    return Result(
                        "Já existe um instrumento com este nome",
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
                    "Instrumento atualizado com sucesso",
                    ResponseStatus.Success,
                    ToData(instrument));
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado",
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
                        "Instrumento não encontrado",
                        ResponseStatus.NotFound);
                }

                if (await _repository.HasActiveEnrollmentsAsync(id))
                {
                    return Result(
                        "O instrumento possui matrículas ativas",
                        ResponseStatus.Conflict);
                }

                if (await _repository.HasAnyEnrollmentAsync(id))
                {
                    return Result(
                        "O instrumento possui histórico de matrículas e não pode ser excluído",
                        ResponseStatus.Conflict);
                }

                var deleted = await _repository.DeleteAsync(instrument);

                if (!deleted)
                {
                    return Result(
                        "Não foi possível excluir o instrumento",
                        ResponseStatus.Error);
                }

                return Result(
                    "Instrumento excluído com sucesso",
                    ResponseStatus.Success);
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado",
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
                    "Nome do instrumento é obrigatório",
                    ResponseStatus.InvalidArgument);
            }

            name = request.Name.Trim();
            description = request.Description?.Trim() ?? string.Empty;

            if (name.Length > MaxNameLength)
            {
                return Result(
                    $"Nome do instrumento deve ter no máximo {MaxNameLength} caracteres",
                    ResponseStatus.InvalidArgument);
            }

            if (description.Length > MaxDescriptionLength)
            {
                return Result(
                    $"Descrição do instrumento deve ter no máximo {MaxDescriptionLength} caracteres",
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
