using Application.Request;
using Application.Response;

namespace Application.Interfaces
{
    public interface ITeacherServices
    {
        Task<TeacherResponseDTO> CreateTeacher(TeacherRequestDTO teacher);

        Task<TeacherListResponseDTO> GetTeachers(
        string? name,
        int page,
        int pageSize);

        Task<TeacherResponseDTO> GetTeacherById(Guid id);

        Task<TeacherResponseDTO> UpdateTeacher(
        Guid id,
        TeacherRequestDTO teacherRequestDTO);

        Task<TeacherResponseDTO> DeleteTeacher(Guid id);
    }
}
