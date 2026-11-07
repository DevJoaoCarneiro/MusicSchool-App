using Application.Common;
using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class StudentService : IStudentServices
    {
        private const int AdultAge = 18;

        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public StudentService(
            IStudentRepository studentRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<StudentResponseDTO> CreateStudent(
            StudentRequestDTO studentRequestDTO)
        {
            try
            {
                var validation = Validate(studentRequestDTO, out var input);

                if (validation != null)
                    return validation;

                if (await _studentRepository.ExistsByEmailAsync(input.Email))
                    return Result("Email is already in use", ResponseStatus.Conflict);

                if (await _studentRepository.ExistsByCpfAsync(input.Cpf))
                    return Result("CPF is already in use", ResponseStatus.Conflict);

                var student = new Student(
                    input.Name,
                    input.Email,
                    input.Phone,
                    input.Cpf,
                    input.BirthDate,
                    input.Address,
                    input.Guardian);

                await _studentRepository.AddAsync(student);

                return Result(
                    "Student created successfully",
                    ResponseStatus.Success,
                    ToData(student));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while creating the student",
                    ResponseStatus.Error);
            }
        }

        public async Task<StudentResponseDTO> GetByIdAsync(Guid id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(
                    id,
                    student => new StudentData
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
                    });

                if (student == null)
                    return Result("Student not found", ResponseStatus.NotFound);

                return Result(
                    "Student found successfully",
                    ResponseStatus.Success,
                    student);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while retrieving the student",
                    ResponseStatus.Error);
            }
        }

        public async Task<StudentListResponseDTO> GetAllAsync(
            string? name,
            int page,
            int pageSize)
        {
            if (!PaginationLimits.IsValid(page, pageSize))
            {
                return new StudentListResponseDTO
                {
                    Message = PaginationLimits.InvalidMessage,
                    Status = ResponseStatus.InvalidArgument
                };
            }

            try
            {
                var result = await _studentRepository.GetAllAsync(
                    name,
                    page,
                    pageSize,
                    student => new StudentListItem
                    {
                        Id = student.Id,
                        Name = student.Name,
                        Email = student.Email,
                        Phone = student.Phone,
                        BirthDate = student.BirthDate
                    });

                return new StudentListResponseDTO
                {
                    Message = "Students retrieved successfully",
                    Status = ResponseStatus.Success,
                    Data = new StudentListData
                    {
                        Items = result.Items,
                        Page = page,
                        PageSize = pageSize,
                        Total = result.Total
                    }
                };
            }
            catch (Exception)
            {
                return new StudentListResponseDTO
                {
                    Message = "An unexpected error occurred while retrieving students",
                    Status = ResponseStatus.Error
                };
            }
        }

        public async Task<StudentResponseDTO> UpdateStudentAsync(
            Guid id,
            StudentRequestDTO studentRequestDTO)
        {
            try
            {
                var validation = Validate(studentRequestDTO, out var input);

                if (validation != null)
                    return validation;

                var student = await _studentRepository.GetByIdAsync(id);

                if (student == null)
                    return Result("Student not found", ResponseStatus.NotFound);

                if (await _studentRepository.ExistsByEmailExceptIdAsync(input.Email, id))
                    return Result("Email is already in use", ResponseStatus.Conflict);

                if (await _studentRepository.ExistsByCpfExceptIdAsync(input.Cpf, id))
                    return Result("CPF is already in use", ResponseStatus.Conflict);

                student.Update(
                    input.Name,
                    input.Email,
                    input.Phone,
                    input.Cpf,
                    input.BirthDate,
                    input.Address,
                    input.Guardian);

                await _studentRepository.UpdateAsync(student);

                return Result(
                    "Student updated successfully",
                    ResponseStatus.Success,
                    ToData(student));
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while updating the student",
                    ResponseStatus.Error);
            }
        }

        public async Task<StudentResponseDTO> DeleteStudentAsync(Guid id)
        {
            try
            {
                var student = await _studentRepository.GetByIdAsync(id);

                if (student == null)
                    return Result("Student not found", ResponseStatus.NotFound);

                if (await _enrollmentRepository.HasActiveEnrollmentsByStudentAsync(id))
                {
                    return Result(
                        "Cannot delete the student because they have active enrollments",
                        ResponseStatus.Conflict);
                }

                if (await _enrollmentRepository.HasAnyEnrollmentByStudentAsync(id))
                {
                    return Result(
                        "Cannot delete the student because they have enrollment history",
                        ResponseStatus.Conflict);
                }

                await _studentRepository.DeleteAsync(student);

                return Result("Student deleted successfully", ResponseStatus.Success);
            }
            catch (Exception)
            {
                return Result(
                    "An unexpected error occurred while deleting the student",
                    ResponseStatus.Error);
            }
        }

        private sealed record StudentInput(
            string Name,
            string Email,
            string Phone,
            string Cpf,
            DateTime BirthDate,
            Address Address,
            Guardian? Guardian);

        private static StudentResponseDTO? Validate(
            StudentRequestDTO? request,
            out StudentInput input)
        {
            input = null!;

            if (request == null)
                return Result("Parameters is empty or null", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.Cpf) ||
                request.BirthDate == null)
            {
                return Result(
                    "Name, email, phone, CPF and birth date are required",
                    ResponseStatus.InvalidArgument);
            }

            var cpf = CpfValidator.Normalize(request.Cpf);

            if (!CpfValidator.IsValid(cpf))
                return Result("Invalid CPF", ResponseStatus.InvalidArgument);

            var birthDate = request.BirthDate.Value.Date;
            var today = DateTime.Today;

            if (birthDate > today)
                return Result("Birth date cannot be in the future", ResponseStatus.InvalidArgument);

            if (request.Address == null)
                return Result("Address is required", ResponseStatus.InvalidArgument);

            if (string.IsNullOrWhiteSpace(request.Address.Street) ||
                string.IsNullOrWhiteSpace(request.Address.Number) ||
                string.IsNullOrWhiteSpace(request.Address.City) ||
                string.IsNullOrWhiteSpace(request.Address.State) ||
                string.IsNullOrWhiteSpace(request.Address.ZipCode))
            {
                return Result("All address fields are required", ResponseStatus.InvalidArgument);
            }

            var age = today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age))
                age--;

            if (age < AdultAge && request.Guardian == null)
            {
                return Result(
                    "Guardian is required for students under 18 years old",
                    ResponseStatus.InvalidArgument);
            }

            Guardian? guardian = null;

            if (request.Guardian != null)
            {
                if (string.IsNullOrWhiteSpace(request.Guardian.Name) ||
                    string.IsNullOrWhiteSpace(request.Guardian.Phone) ||
                    string.IsNullOrWhiteSpace(request.Guardian.Email) ||
                    string.IsNullOrWhiteSpace(request.Guardian.Cpf))
                {
                    return Result("All guardian fields are required", ResponseStatus.InvalidArgument);
                }

                var guardianCpf = CpfValidator.Normalize(request.Guardian.Cpf);

                if (!CpfValidator.IsValid(guardianCpf))
                    return Result("Invalid guardian CPF", ResponseStatus.InvalidArgument);

                if (guardianCpf == cpf)
                {
                    return Result(
                        "Guardian CPF must be different from the student CPF",
                        ResponseStatus.InvalidArgument);
                }

                guardian = new Guardian(
                    request.Guardian.Name.Trim(),
                    request.Guardian.Phone.Trim(),
                    Normalizer.Email(request.Guardian.Email),
                    guardianCpf);
            }

            var address = new Address(
                request.Address.Street.Trim(),
                request.Address.Number.Trim(),
                request.Address.City.Trim(),
                request.Address.State.Trim().ToUpperInvariant(),
                request.Address.ZipCode.Trim());

            input = new StudentInput(
                request.Name.Trim(),
                Normalizer.Email(request.Email),
                request.Phone.Trim(),
                cpf,
                birthDate,
                address,
                guardian);

            return null;
        }

        private static StudentData ToData(Student student) => new()
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
        };

        private static StudentResponseDTO Result(
            string message,
            string status,
            StudentData? data = null) => new()
            {
                Message = message,
                Status = status,
                Data = data
            };
    }
}
