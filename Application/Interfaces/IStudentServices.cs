
using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface IStudentServices
    {
        Task<StudentResponseDTO> CreateStudent(StudentRequestDTO student);

        Task<StudentResponseDTO> GetByIdAsync(Guid id);

        Task<StudentListResponseDTO> GetAllAsync(
             string? name,
             int page,
             int pageSize
         );
    }
}
