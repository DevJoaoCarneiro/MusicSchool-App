
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

        Task<LessonResponseDTO> CancelAsync(
           Guid id,
           CancelLessonRequestDTO request);

        Task<LessonResponseDTO> RescheduleAsync(
    Guid id,
    RescheduleLessonRequestDTO request);
    }
}
