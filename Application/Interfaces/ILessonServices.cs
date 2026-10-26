
using Application.DTOs;

namespace Application.Interfaces
{
    public interface ILessonServices
    {
        Task<LessonResponseDTO> CreateAsync(
            LessonRequestDTO request);

        Task<LessonResponseDTO> GetByDateRangeAsync(
            DateTime? from,
            DateTime? to,
            Guid? teacherId,
            Guid? studentId);
    }
}
