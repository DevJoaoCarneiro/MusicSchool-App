
using Application.DTOs;

namespace Application.Interfaces
{
    public interface ILessonServices
    {
        Task<LessonResponseDTO> CreateAsync(LessonRequestDTO request);
    }
}
