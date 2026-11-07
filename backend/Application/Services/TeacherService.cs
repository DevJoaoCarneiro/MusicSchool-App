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

        public TeacherService(ITeacherRepository teacherRepository)
        {
            _teacherRepository = teacherRepository;
        }

        public async Task<TeacherResponseDTO> CreateTeacher(
            TeacherRequestDTO teacherRequestDTO)
        {
            try
            {
                var validation = Validate(teacherRequestDTO, out var input);

                if (validation != null)
                    return validation;

                if (await _teacherRepository.ExistsByEmailAsync(input.Email))
                    return Result("Email is already in use", ResponseStatus.Conflict);

                if (await _teacherRepository.ExistsByCpfAsync(input.Cpf))
                    return Result("CPF is already in use", ResponseStatus.Conflict);

                var newTeacher = new Teacher(
                    input.Name,
                    input.Email,
                    input.Phone,
                    input.Cpf,
                    input.Specialties);

                await _teacherRepository.AddAsync(newTeacher);

                return Result(
                    "Teacher created successfully",
                    ResponseStatus.Success,
                    ToData(newTeacher));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while creating the teacher",
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
                    Message = "Teachers retrieved successfully",
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
                    Message = "An unexpected error occurred while retrieving teachers",
                    Status = ResponseStatus.Error
                };
            }
        }

        public async Task<TeacherResponseDTO> GetTeacherById(Guid id)
        {
            if (id == Guid.Empty)
                return Result("Invalid teacher ID", ResponseStatus.InvalidArgument);

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
                    return Result("Teacher not found", ResponseStatus.NotFound);

                return Result(
                    "Teacher found successfully",
                    ResponseStatus.Success,
                    teacher);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while retrieving the teacher",
                    ResponseStatus.Error);
            }
        }

        public async Task<TeacherResponseDTO> UpdateTeacher(
            Guid id,
            TeacherRequestDTO teacherRequestDTO)
        {
            if (id == Guid.Empty)
                return Result("Invalid teacher ID", ResponseStatus.InvalidArgument);

            try
            {
                var validation = Validate(teacherRequestDTO, out var input);

                if (validation != null)
                    return validation;

                var teacher = await _teacherRepository.GetByIdAsync(id);

                if (teacher == null)
                    return Result("Teacher not found", ResponseStatus.NotFound);

                if (await _teacherRepository.ExistsByEmailExceptIdAsync(input.Email, id))
                    return Result("Email is already registered", ResponseStatus.Conflict);

                if (await _teacherRepository.ExistsByCpfExceptIdAsync(input.Cpf, id))
                    return Result("CPF is already registered", ResponseStatus.Conflict);

                teacher.Update(
                    input.Name,
                    input.Email,
                    input.Phone,
                    input.Cpf,
                    input.Specialties);

                await _teacherRepository.UpdateAsync(teacher);

                return Result(
                    "Teacher updated successfully",
                    ResponseStatus.Success,
                    ToData(teacher));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while updating the teacher",
                    ResponseStatus.Error);
            }
        }

        public async Task<TeacherResponseDTO> DeleteTeacher(Guid id)
        {
            if (id == Guid.Empty)
                return Result("Invalid teacher ID", ResponseStatus.InvalidArgument);

            try
            {
                var teacher = await _teacherRepository.GetByIdAsync(id);

                if (teacher == null)
                    return Result("Teacher not found", ResponseStatus.NotFound);

                if (await _teacherRepository.HasActiveLessonsAsync(id))
                {
                    return Result(
                        "Teacher has an active lesson linked",
                        ResponseStatus.Conflict);
                }

                if (await _teacherRepository.HasAnyLessonHistoryAsync(id))
                {
                    return Result(
                        "Teacher has lesson history and cannot be deleted",
                        ResponseStatus.Conflict);
                }

                await _teacherRepository.DeleteAsync(teacher);

                return Result("Teacher deleted successfully", ResponseStatus.Success);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while deleting the teacher",
                    ResponseStatus.Error);
            }
        }

        private sealed record TeacherInput(
            string Name,
            string Email,
            string Phone,
            string Cpf,
            List<string> Specialties);

        private static TeacherResponseDTO? Validate(
            TeacherRequestDTO? request,
            out TeacherInput input)
        {
            input = null!;

            if (request == null)
                return Result("Request body is required", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Cpf))
            {
                return Result(
                    "Name, email, phone and CPF are required",
                    ResponseStatus.InvalidArgument);
            }

            var specialties = (request.Specialties ?? new List<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (specialties.Count == 0)
                return Result("At least one specialty is required", ResponseStatus.InvalidArgument);

            var cpf = CpfValidator.Normalize(request.Cpf);

            if (!CpfValidator.IsValid(cpf))
                return Result("Invalid CPF", ResponseStatus.InvalidArgument);

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
