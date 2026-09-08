using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class StudentService : IStudentServices
    {
        private readonly IStudentRepository _studentRepository;

        public StudentService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        public async Task<StudentResponseDTO> CreateStudent(
            StudentRequestDTO studentRequestDTO)
        {
            try
            {
                
                if (studentRequestDTO == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

               
                if (string.IsNullOrWhiteSpace(studentRequestDTO.Name) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Email) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Phone) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Cpf) ||
                    studentRequestDTO.BirthDate == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Name, email, phone, CPF and birth date are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

               
                if (studentRequestDTO.Address == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Address is required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(studentRequestDTO.Address.Street) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.Number) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.City) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.State) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.ZipCode))
                {
                    return new StudentResponseDTO
                    {
                        Message = "All address fields are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

               
                if (await _studentRepository.ExistsByEmailAsync(studentRequestDTO.Email))
                {
                    return new StudentResponseDTO
                    {
                        Message = "Email is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

               
                if (await _studentRepository.ExistsByCpfAsync(studentRequestDTO.Cpf))
                {
                    return new StudentResponseDTO
                    {
                        Message = "CPF is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                var today = DateTime.Today;

                int age = today.Year - studentRequestDTO.BirthDate.Value.Year;

                if (studentRequestDTO.BirthDate.Value.Date > today.AddYears(-age))
                {
                    age--;
                }

               
                if (age < 18 && studentRequestDTO.Guardian == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Guardian is required for students under 18 years old",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

               
                var address = new Address(
                    studentRequestDTO.Address.Street,
                    studentRequestDTO.Address.Number,
                    studentRequestDTO.Address.City,
                    studentRequestDTO.Address.State,
                    studentRequestDTO.Address.ZipCode
                );

                
                Guardian? guardian = null;

                if (studentRequestDTO.Guardian != null)
                {
                    if (string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Name) ||
                        string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Phone) ||
                        string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Email) ||
                        string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Cpf))
                    {
                        return new StudentResponseDTO
                        {
                            Message = "All guardian fields are required",
                            Status = "invalid_argument",
                            Data = null
                        };
                    }

                    guardian = new Guardian(
                        studentRequestDTO.Guardian.Name,
                        studentRequestDTO.Guardian.Phone,
                        studentRequestDTO.Guardian.Email,
                        studentRequestDTO.Guardian.Cpf
                    );
                }

               
                var student = new Student(
                    studentRequestDTO.Name,
                    studentRequestDTO.Email,
                    studentRequestDTO.Phone,
                    studentRequestDTO.Cpf,
                    studentRequestDTO.BirthDate.Value,
                    address,
                    guardian
                );

              
                await _studentRepository.AddAsync(student);

               
                return new StudentResponseDTO
                {
                    Message = "Student created successfully",
                    Status = "Success",
                    Data = new StudentData
                    {
                        Id = student.Id,
                        Name = student.Name,
                        Email = student.Email,
                        Phone = student.Phone,
                        Cpf = student.Cpf,
                        BirthDate = student.BirthDate,

                        Address = new AddressData
                        {
                            Street = student.Address.Street,
                            Number = student.Address.Number,
                            City = student.Address.City,
                            State = student.Address.State,
                            ZipCode = student.Address.ZipCode
                        },

                        Guardian = student.Guardian == null
                            ? null
                            : new GuardianData
                            {
                                Name = student.Guardian.Name,
                                Phone = student.Guardian.Phone,
                                Email = student.Guardian.Email,
                                Cpf = student.Guardian.Cpf
                            }
                    }
                };
            }
            catch (Exception ex)
            {
                return new StudentResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }

        public async Task<StudentResponseDTO> GetByIdAsync(Guid id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);

                if (student == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Student not found",
                        Status = "not_found",
                        Data = null
                    };
                }

                return new StudentResponseDTO
                {
                    Message = "Student found successfully",
                    Status = "Success",
                    Data = new StudentData
                    {
                        Id = student.Id,
                        Name = student.Name,
                        Email = student.Email,
                        Phone = student.Phone,
                        Cpf = student.Cpf,
                        BirthDate = student.BirthDate,

                        Address = new AddressData
                        {
                            Street = student.Address.Street,
                            Number = student.Address.Number,
                            City = student.Address.City,
                            State = student.Address.State,
                            ZipCode = student.Address.ZipCode
                        },

                        Guardian = student.Guardian == null
                            ? null
                            : new GuardianData
                            {
                                Name = student.Guardian.Name,
                                Phone = student.Guardian.Phone,
                                Email = student.Guardian.Email,
                                Cpf = student.Guardian.Cpf
                            }
                    }
                };
            }
            catch (Exception ex)
            {
                return new StudentResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }

        public async Task<StudentListResponseDTO> GetAllAsync(
                 string? name,
                 int page,
                 int pageSize)
        {
            try
            {
               
                if (page <= 0 || pageSize <= 0)
                {
                    return new StudentListResponseDTO
                    {
                        Message = "Page and pageSize must be greater than zero",
                        Status = "invalid_argument",
                        Data = null!
                    };
                }

                var result = await _studentRepository.GetAllAsync(
                    name,
                    page,
                    pageSize
                );

                var items = result.Students.Select(student =>
                    new StudentListItem
                    {
                        Id = student.Id,
                        Name = student.Name,
                        Email = student.Email,
                        Phone = student.Phone,
                        BirthDate = student.BirthDate
                    }
                ).ToList();

                return new StudentListResponseDTO
                {
                    Message = "Students retrieved successfully",
                    Status = "Success",

                    Data = new StudentListData
                    {
                        Items = items,
                        Page = page,
                        PageSize = pageSize,
                        Total = result.Total
                    }
                };
            }
            catch (Exception ex)
            {
                return new StudentListResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null!
                };
            }
        }

        public async Task<StudentResponseDTO> UpdateStudentAsync(
    Guid id,
    StudentRequestDTO studentRequestDTO)
        {
            try
            {
          
                if (studentRequestDTO == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

              
                var student = await _studentRepository.GetByIdAsync(id);

                if (student == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Student not found",
                        Status = "not_found",
                        Data = null
                    };
                }

                
                if (string.IsNullOrWhiteSpace(studentRequestDTO.Name) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Email) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Phone) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Cpf) ||
                    studentRequestDTO.BirthDate == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Name, email, phone, CPF and birth date are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                
                if (studentRequestDTO.Address == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Address is required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                if (string.IsNullOrWhiteSpace(studentRequestDTO.Address.Street) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.Number) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.City) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.State) ||
                    string.IsNullOrWhiteSpace(studentRequestDTO.Address.ZipCode))
                {
                    return new StudentResponseDTO
                    {
                        Message = "All address fields are required",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

               
                if (await _studentRepository.ExistsByEmailExceptIdAsync(
                    studentRequestDTO.Email,
                    id))
                {
                    return new StudentResponseDTO
                    {
                        Message = "Email is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                
                if (await _studentRepository.ExistsByCpfExceptIdAsync(
                    studentRequestDTO.Cpf,
                    id))
                {
                    return new StudentResponseDTO
                    {
                        Message = "CPF is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

               
                var today = DateTime.Today;

                int age = today.Year - studentRequestDTO.BirthDate.Value.Year;

                if (studentRequestDTO.BirthDate.Value.Date >
                    today.AddYears(-age))
                {
                    age--;
                }

               
                if (age < 18 && studentRequestDTO.Guardian == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Guardian is required for students under 18 years old",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

               
                var address = new Address(
                    studentRequestDTO.Address.Street,
                    studentRequestDTO.Address.Number,
                    studentRequestDTO.Address.City,
                    studentRequestDTO.Address.State,
                    studentRequestDTO.Address.ZipCode
                );

                Guardian? guardian = null;

                if (studentRequestDTO.Guardian != null)
                {
                    if (string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Name) ||
                        string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Phone) ||
                        string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Email) ||
                        string.IsNullOrWhiteSpace(studentRequestDTO.Guardian.Cpf))
                    {
                        return new StudentResponseDTO
                        {
                            Message = "All guardian fields are required",
                            Status = "invalid_argument",
                            Data = null
                        };
                    }

                    guardian = new Guardian(
                        studentRequestDTO.Guardian.Name,
                        studentRequestDTO.Guardian.Phone,
                        studentRequestDTO.Guardian.Email,
                        studentRequestDTO.Guardian.Cpf
                    );
                }

                
                student.Update(
                    studentRequestDTO.Name,
                    studentRequestDTO.Email,
                    studentRequestDTO.Phone,
                    studentRequestDTO.Cpf,
                    studentRequestDTO.BirthDate.Value,
                    address,
                    guardian
                );

               
                await _studentRepository.UpdateAsync(student);

               
                return new StudentResponseDTO
                {
                    Message = "Student updated successfully",
                    Status = "Success",
                    Data = new StudentData
                    {
                        Id = student.Id,
                        Name = student.Name,
                        Email = student.Email,
                        Phone = student.Phone,
                        Cpf = student.Cpf,
                        BirthDate = student.BirthDate,

                        Address = new AddressData
                        {
                            Street = student.Address.Street,
                            Number = student.Address.Number,
                            City = student.Address.City,
                            State = student.Address.State,
                            ZipCode = student.Address.ZipCode
                        },

                        Guardian = student.Guardian == null
                            ? null
                            : new GuardianData
                            {
                                Name = student.Guardian.Name,
                                Phone = student.Guardian.Phone,
                                Email = student.Guardian.Email,
                                Cpf = student.Guardian.Cpf
                            }
                    }
                };
            }
            catch (Exception ex)
            {
                return new StudentResponseDTO
                {
                    Message = $"An error occurred: {ex.Message}",
                    Status = "error",
                    Data = null
                };
            }
        }

    }
}