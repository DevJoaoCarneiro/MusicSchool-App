
using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ILessonRepository
    {
        Task<Lesson> AddAsync(Lesson lesson);

        Task<Lesson?> GetByIdAsync(Guid id);

        Task<Lesson> UpdateAsync(Lesson lesson);

        Task<bool> HasTeacherConflictAsync(
    Guid teacherId,
    DateTime startsAt,
    DateTime endsAt,
    Guid? excludeLessonId = null);

        Task<bool> HasStudentConflictAsync(
            Guid studentId,
            DateTime startsAt,
            DateTime endsAt,
            Guid? excludeLessonId = null);

        Task<bool> HasRoomConflictAsync(
            string room,
            DateTime startsAt,
            DateTime endsAt,
            Guid? excludeLessonId = null);

        Task<List<Lesson>> GetByDateRangeAsync(
            DateTime from,
            DateTime to,
            Guid? teacherId,
            Guid? studentId);

        Task<Lesson> UpdateWithHistoryAsync(
    Lesson lesson,
    LessonChange lessonChange);
    }
}
