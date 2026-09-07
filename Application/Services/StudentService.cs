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
                // 1. Validar si el request es null
                if (studentRequestDTO == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Parameters is empty or null",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                // 2. Validar campos principales
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

                // 3. Validar Address
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

                // 4. Verificar Email duplicado
                if (await _studentRepository.ExistsByEmailAsync(studentRequestDTO.Email))
                {
                    return new StudentResponseDTO
                    {
                        Message = "Email is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                // 5. Verificar CPF duplicado
                if (await _studentRepository.ExistsByCpfAsync(studentRequestDTO.Cpf))
                {
                    return new StudentResponseDTO
                    {
                        Message = "CPF is already in use",
                        Status = "conflict",
                        Data = null
                    };
                }

                // 6. Calcular edad
                var today = DateTime.Today;

                int age = today.Year - studentRequestDTO.BirthDate.Value.Year;

                if (studentRequestDTO.BirthDate.Value.Date > today.AddYears(-age))
                {
                    age--;
                }

                // 7. Guardian obligatorio para menores de edad
                if (age < 18 && studentRequestDTO.Guardian == null)
                {
                    return new StudentResponseDTO
                    {
                        Message = "Guardian is required for students under 18 years old",
                        Status = "invalid_argument",
                        Data = null
                    };
                }

                // 8. Crear Address del Domain
                var address = new Address(
                    studentRequestDTO.Address.Street,
                    studentRequestDTO.Address.Number,
                    studentRequestDTO.Address.City,
                    studentRequestDTO.Address.State,
                    studentRequestDTO.Address.ZipCode
                );

                // 9. Crear Guardian si existe
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

                // 10. Crear Student
                var student = new Student(
                    studentRequestDTO.Name,
                    studentRequestDTO.Email,
                    studentRequestDTO.Phone,
                    studentRequestDTO.Cpf,
                    studentRequestDTO.BirthDate.Value,
                    address,
                    guardian
                );

                // 11. Guardar en el repositorio
                await _studentRepository.AddAsync(student);

                // 12. Retornar respuesta
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
    }
}