
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IStudentServices
    {
        Task<StudentResponseDTO> CreateStudent(StudentRequestDTO student);

        Task<StudentResponseDTO> GetByIdAsync(Guid id);
    }
}
