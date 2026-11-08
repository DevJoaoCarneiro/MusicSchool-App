using Application.Request;
using Application.Response;

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
            Guid? studentId,
            string? teacherEmail);

        Task<LessonResponseDTO> GetByIdAsync(
            Guid id,
            string? teacherEmail);

        Task<LessonResponseDTO> CancelAsync(
            Guid id,
            CancelLessonRequestDTO request);

        Task<LessonResponseDTO> CompleteAsync(Guid id);

        Task<LessonResponseDTO> RescheduleAsync(
            Guid id,
            RescheduleLessonRequestDTO request);

        Task<LessonResponseDTO> CreateRecurringAsync(
            CreateRecurringLessonDTO request);

        Task<LessonResponseDTO> CancelSeriesAsync(
            Guid seriesId,
            CancelLessonSeriesRequestDTO request);
    }
}
