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

                if (teacherRequestDTO == null)
                {
                    return new TeacherResponseDTO
                    {
                        Message = "Request body is required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(teacherRequestDTO.Name) ||
                    string.IsNullOrWhiteSpace(teacherRequestDTO.Email) ||
                    string.IsNullOrWhiteSpace(teacherRequestDTO.Phone) ||
                    string.IsNullOrWhiteSpace(teacherRequestDTO.Cpf))
                {
                    return new TeacherResponseDTO
                    {
                        Message = "Name, email, phone and CPF are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (teacherRequestDTO.Specialties == null ||
                    teacherRequestDTO.Specialties.Count == 0)
                {
                    return new TeacherResponseDTO
                    {
                        Message = "At least one specialty is required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                string cpf = new string(
                    teacherRequestDTO.Cpf
                        .Where(char.IsDigit)
                        .ToArray()
                );

                if (!IsValidCpf(cpf))
                {
                    return new TeacherResponseDTO
                    {
                        Message = "Invalid CPF",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                string email = teacherRequestDTO.Email.Trim();

                if (await _teacherRepository.ExistsByEmailAsync(email))
                {
                    return new TeacherResponseDTO
                    {
                        Message = "Email is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                if (await _teacherRepository.ExistsByCpfAsync(cpf))
                {
                    return new TeacherResponseDTO
                    {
                        Message = "CPF is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                var newTeacher = new Teacher(
                    teacherRequestDTO.Name.Trim(),
                    email,
                    teacherRequestDTO.Phone.Trim(),
                    cpf,
                    teacherRequestDTO.Specialties
                );

                await _teacherRepository.AddAsync(newTeacher);

                return new TeacherResponseDTO
                {
                    Message = "Teacher created successfully",
                    Status = "Success",
                    Data = new TeacherData
                    {
                        Id = newTeacher.Id,
                        Name = newTeacher.Name,
                        Email = newTeacher.Email,
                        Phone = newTeacher.Phone,
                        Cpf = newTeacher.Cpf,
                        Specialties = newTeacher.Specialties
                    }
                };
            }
            catch (Exception)
            {
                return new TeacherResponseDTO
                {
                    Message = "An unexpected error occurred",
                    Status = "error",
                    Data = null
                };
            }
        }
        public async Task<TeacherListResponseDTO> GetTeachers(
            string? name,
            int page,
            int pageSize)
        {
            try
            {
                if (page < 1)
                {
                    return new TeacherListResponseDTO
                    {
                        Message = "Page must be greater than 0",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (pageSize < 1)
                {
                    return new TeacherListResponseDTO
                    {
                        Message = "Page size must be greater than 0",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                var teachers = await _teacherRepository.GetAllAsync(
                    name,
                    page,
                    pageSize);

                return new TeacherListResponseDTO
                {
                    Message = "Teachers retrieved successfully",
                    Status = "Success",
                    Data = new TeacherListData
                    {
                        Items = teachers.Items
                            .Select(t => new TeacherListItem
                            {
                                Id = t.Id,
                                Name = t.Name,
                                Email = t.Email,
                                Specialties = t.Specialties
                            })
                            .ToList(),

                        Page = page,
                        PageSize = pageSize,
                        Total = teachers.Total
                    }
                };
            }
            catch (Exception ex)
            {
                return new TeacherListResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }
        private static bool IsValidCpf(string cpf)
        {

            if (cpf.Length != 11)
                return false;

            if (cpf.Distinct().Count() == 1)
                return false;

            int sum = 0;

            for (int i = 0; i < 9; i++)
                sum += (cpf[i] - '0') * (10 - i);

            int firstDigit = sum % 11 < 2 ? 0 : 11 - sum % 11;

            if (cpf[9] - '0' != firstDigit)
                return false;

            sum = 0;

            for (int i = 0; i < 10; i++)
                sum += (cpf[i] - '0') * (11 - i);

            int secondDigit = sum % 11 < 2 ? 0 : 11 - sum % 11;

            return cpf[10] - '0' == secondDigit;

        }
        public async Task<TeacherResponseDTO> GetTeacherById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new TeacherResponseDTO
                {
                    Message = "Invalid teacher ID",
                    Status = "invalid_argument",
                    Data = null
                };
            }

            var teacher = await _teacherRepository.GetByIdAsync(id);

            if (teacher == null)
            {
                return new TeacherResponseDTO
                {
                    Message = "Teacher not found",
                    Status = "not_found",
                    Data = null
                };
            }

            return new TeacherResponseDTO
            {
                Message = "Teacher found successfully",
                Status = "Success",
                Data = new TeacherData
                {
                    Id = teacher.Id,
                    Name = teacher.Name,
                    Email = teacher.Email,
                    Phone = teacher.Phone,
                    Cpf = teacher.Cpf,
                    Specialties = teacher.Specialties
                }
            };
        }

        public async Task<TeacherResponseDTO> UpdateTeacher(
            Guid id,
            TeacherRequestDTO teacherRequestDTO)
        {
            if (id == Guid.Empty)
            {
                return new TeacherResponseDTO
                {
                    Message = "Invalid teacher ID",
                    Status = "invalid_argument",
                    Data = null
                };
            }

            if (teacherRequestDTO == null ||
                string.IsNullOrWhiteSpace(teacherRequestDTO.Name) ||
                string.IsNullOrWhiteSpace(teacherRequestDTO.Email) ||
                string.IsNullOrWhiteSpace(teacherRequestDTO.Phone) ||
                string.IsNullOrWhiteSpace(teacherRequestDTO.Cpf) ||
                teacherRequestDTO.Specialties == null ||
                teacherRequestDTO.Specialties.Count == 0)
            {
                return new TeacherResponseDTO
                {
                    Message = "All required fields must be provided",
                    Status = "invalid_argument",
                    Data = null
                };
            }

            var teacher = await _teacherRepository.GetByIdAsync(id);

            if (teacher == null)
            {
                return new TeacherResponseDTO
                {
                    Message = "Teacher not found",
                    Status = "not_found",
                    Data = null
                };
            }

            var cpf = new string(
                teacherRequestDTO.Cpf
                    .Where(char.IsDigit)
                    .ToArray());

            if (!IsValidCpf(cpf))
            {
                return new TeacherResponseDTO
                {
                    Message = "Invalid CPF",
                    Status = "invalid_argument",
                    Data = null
                };
            }

            var email = teacherRequestDTO.Email.Trim();

            var emailExists =
                await _teacherRepository.ExistsByEmailAsync(email);

            if (emailExists &&
                !string.Equals(
                    teacher.Email,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                return new TeacherResponseDTO
                {
                    Message = "Email is already registered",
                    Status = "conflict",
                    Data = null
                };
            }

            var cpfExists =
                await _teacherRepository.ExistsByCpfAsync(cpf);

            if (cpfExists && teacher.Cpf != cpf)
            {
                return new TeacherResponseDTO
                {
                    Message = "CPF is already registered",
                    Status = "conflict",
                    Data = null
                };
            }

            teacher.Update(
                teacherRequestDTO.Name.Trim(),
                email,
                teacherRequestDTO.Phone.Trim(),
                cpf,
                teacherRequestDTO.Specialties);

            await _teacherRepository.UpdateAsync(teacher);

            return new TeacherResponseDTO
            {
                Message = "Teacher updated successfully",
                Status = "Success",
                Data = new TeacherData
                {
                    Id = teacher.Id,
                    Name = teacher.Name,
                    Email = teacher.Email,
                    Phone = teacher.Phone,
                    Cpf = teacher.Cpf,
                    Specialties = teacher.Specialties
                }
            };
        }

        public async Task<TeacherResponseDTO> DeleteTeacher(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new TeacherResponseDTO
                {
                    Message = "Invalid teacher ID",
                    Status = "invalid_argument",
                    Data = null
                };
            }

            var teacher = await _teacherRepository.GetByIdAsync(id);

            if (teacher == null)
            {
                return new TeacherResponseDTO
                {
                    Message = "Teacher not found",
                    Status = "not_found",
                    Data = null
                };
            }

            await _teacherRepository.DeleteAsync(teacher);

            return new TeacherResponseDTO
            {
                Message = "Teacher deleted successfully",
                Status = "Success",
                Data = null
            };
        }
    }
}
