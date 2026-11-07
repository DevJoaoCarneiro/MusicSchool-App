using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class TeacherService : ITeacherServices
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IInstrumentRepository _instrumentRepository;

        public TeacherService(
            ITeacherRepository teacherRepository,
            IInstrumentRepository instrumentRepository)
        {
            _teacherRepository = teacherRepository;
            _instrumentRepository = instrumentRepository;
        }

        public async Task<TeacherResponseDTO> CreateTeacher(
            TeacherRequestDTO teacherRequestDTO)
        {
            try
            {
                var validation = Validate(teacherRequestDTO, out var input);

                if (validation != null)
                    return validation;

                var specialtiesValidation = await ResolveSpecialtiesAsync(input);

                if (specialtiesValidation.Error != null)
                    return specialtiesValidation.Error;

                input = specialtiesValidation.Input!;

                if (await _teacherRepository.ExistsByEmailAsync(input.Email))
                    return Result("E-mail já está em uso", ResponseStatus.Conflict);

                if (await _teacherRepository.ExistsByCpfAsync(input.Cpf))
                    return Result("CPF já está em uso", ResponseStatus.Conflict);

                var newTeacher = new Teacher(
                    input.Name,
                    input.Email,
                    input.Phone,
                    input.Cpf,
                    input.Specialties);

                await _teacherRepository.AddAsync(newTeacher);

                return Result(
                    "Professor cadastrado com sucesso",
                    ResponseStatus.Success,
                    ToData(newTeacher));
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado ao cadastrar o professor",
                    ResponseStatus.Error);
            }
        }

        public async Task<TeacherListResponseDTO> GetTeachers(
            string? name,
            int page,
            int pageSize)
        {
            if (!PaginationLimits.IsValid(page, pageSize))
            {
                return new TeacherListResponseDTO
                {
                    Message = PaginationLimits.InvalidMessage,
                    Status = ResponseStatus.InvalidArgument
                };
            }

            try
            {
                var teachers = await _teacherRepository.GetAllAsync(
                    name,
                    page,
                    pageSize,
                    t => new TeacherListItem
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Email = t.Email,
                        Specialties = t.Specialties
                    });

                return new TeacherListResponseDTO
                {
                    Message = "Professores listados com sucesso",
                    Status = ResponseStatus.Success,
                    Data = new TeacherListData
                    {
                        Items = teachers.Items,
                        Page = page,
                        PageSize = pageSize,
                        Total = teachers.Total
                    }
                };
            }
            catch (Exception)
            {
                return new TeacherListResponseDTO
                {
                    Message = "Ocorreu um erro inesperado ao listar os professores",
                    Status = ResponseStatus.Error
                };
            }
        }

        public async Task<TeacherResponseDTO> GetTeacherById(Guid id)
        {
            if (id == Guid.Empty)
                return Result("ID de professor inválido", ResponseStatus.InvalidArgument);

            try
            {
                var teacher = await _teacherRepository.GetByIdAsync(
                    id,
                    teacher => new TeacherData
                    {
                        Id = teacher.Id,
                        Name = teacher.Name,
                        Email = teacher.Email,
                        Phone = teacher.Phone,
                        Cpf = teacher.Cpf,
                        Specialties = teacher.Specialties
                    });

                if (teacher == null)
                    return Result("Professor não encontrado", ResponseStatus.NotFound);

                return Result(
                    "Professor encontrado com sucesso",
                    ResponseStatus.Success,
                    teacher);
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado ao buscar o professor",
                    ResponseStatus.Error);
            }
        }

        public async Task<TeacherResponseDTO> UpdateTeacher(
            Guid id,
            TeacherRequestDTO teacherRequestDTO)
        {
            if (id == Guid.Empty)
                return Result("ID de professor inválido", ResponseStatus.InvalidArgument);

            try
            {
                var validation = Validate(teacherRequestDTO, out var input);

                if (validation != null)
                    return validation;

                var teacher = await _teacherRepository.GetByIdAsync(id);

                if (teacher == null)
                    return Result("Professor não encontrado", ResponseStatus.NotFound);

                var specialtiesValidation = await ResolveSpecialtiesAsync(input);

                if (specialtiesValidation.Error != null)
                    return specialtiesValidation.Error;

                input = specialtiesValidation.Input!;

                if (await _teacherRepository.ExistsByEmailExceptIdAsync(input.Email, id))
                    return Result("E-mail já está em uso", ResponseStatus.Conflict);

                if (await _teacherRepository.ExistsByCpfExceptIdAsync(input.Cpf, id))
                    return Result("CPF já está em uso", ResponseStatus.Conflict);

                teacher.Update(
                    input.Name,
                    input.Email,
                    input.Phone,
                    input.Cpf,
                    input.Specialties);

                await _teacherRepository.UpdateAsync(teacher);

                return Result(
                    "Professor atualizado com sucesso",
                    ResponseStatus.Success,
                    ToData(teacher));
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado ao atualizar o professor",
                    ResponseStatus.Error);
            }
        }

        public async Task<TeacherResponseDTO> DeleteTeacher(Guid id)
        {
            if (id == Guid.Empty)
                return Result("ID de professor inválido", ResponseStatus.InvalidArgument);

            try
            {
                var teacher = await _teacherRepository.GetByIdAsync(id);

                if (teacher == null)
                    return Result("Professor não encontrado", ResponseStatus.NotFound);

                if (await _teacherRepository.HasActiveLessonsAsync(id))
                {
                    return Result(
                        "O professor possui aulas agendadas",
                        ResponseStatus.Conflict);
                }

                if (await _teacherRepository.HasAnyLessonHistoryAsync(id))
                {
                    return Result(
                        "O professor possui histórico de aulas e não pode ser excluído",
                        ResponseStatus.Conflict);
                }

                await _teacherRepository.DeleteAsync(teacher);

                return Result("Professor excluído com sucesso", ResponseStatus.Success);
            }
            catch (Exception)
            {
                return Result(
                    "Ocorreu um erro inesperado ao excluir o professor",
                    ResponseStatus.Error);
            }
        }

        private sealed record TeacherInput(
            string Name,
            string Email,
            string Phone,
            string Cpf,
            List<string> Specialties);

        private sealed record SpecialtiesResolution(
            TeacherInput? Input,
            TeacherResponseDTO? Error);

        private async Task<SpecialtiesResolution> ResolveSpecialtiesAsync(TeacherInput input)
        {
            var instruments = await _instrumentRepository.GetAllAsync();

            var byName = instruments
                .GroupBy(instrument => instrument.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Name, StringComparer.OrdinalIgnoreCase);

            var unknown = input.Specialties
                .Where(specialty => !byName.ContainsKey(specialty))
                .ToList();

            if (unknown.Count > 0)
            {
                return new SpecialtiesResolution(
                    null,
                    Result(
                        $"As especialidades devem ser instrumentos cadastrados: {string.Join(", ", unknown)}",
                        ResponseStatus.InvalidArgument));
            }

            var canonical = input.Specialties
                .Select(specialty => byName[specialty])
                .ToList();

            return new SpecialtiesResolution(input with { Specialties = canonical }, null);
        }

        private static TeacherResponseDTO? Validate(
            TeacherRequestDTO? request,
            out TeacherInput input)
        {
            input = null!;

            if (request == null)
                return Result("Corpo da requisição é obrigatório", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Cpf))
            {
                return Result(
                    "Nome, e-mail, telefone e CPF são obrigatórios",
                    ResponseStatus.InvalidArgument);
            }

            var specialties = (request.Specialties ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (specialties.Count == 0)
                return Result("Informe ao menos uma especialidade", ResponseStatus.InvalidArgument);

            var cpf = CpfValidator.Normalize(request.Cpf);

            if (!CpfValidator.IsValid(cpf))
                return Result("CPF inválido", ResponseStatus.InvalidArgument);

            input = new TeacherInput(
                request.Name.Trim(),
                Normalizer.Email(request.Email),
                request.Phone.Trim(),
                cpf,
                specialties);

            return null;
        }

        private static TeacherData ToData(Teacher teacher) => new()
        {
            Id = teacher.Id,
            Name = teacher.Name,
            Email = teacher.Email,
            Phone = teacher.Phone,
            Cpf = teacher.Cpf,
            Specialties = teacher.Specialties
        };

        private static TeacherResponseDTO Result(
            string message,
            string status,
            TeacherData? data = null) => new()
            {
                Message = message,
                Status = status,
                Data = data
            };
    }
}
