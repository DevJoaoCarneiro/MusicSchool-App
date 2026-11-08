
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ILessonChangeRepository
    {
        Task<LessonChange> AddAsync(LessonChange lessonChange);
    }
}
