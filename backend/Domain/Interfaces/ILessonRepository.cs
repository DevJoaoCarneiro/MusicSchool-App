using Domain.Entities;
using System.Linq.Expressions;

namespace Domain.Interfaces
{
    public record LessonSlot(
        Guid Id,
        Guid TeacherId,
        Guid StudentId,
        string Room,
        DateTime StartsAt,
        int DurationMinutes)
    {
        public DateTime EndsAt => StartsAt.AddMinutes(DurationMinutes);

        public bool Overlaps(DateTime startsAt, DateTime endsAt) =>
            StartsAt < endsAt && EndsAt > startsAt;
    }

    public interface ILessonRepository
    {
        Task<Lesson> AddAsync(Lesson lesson);

        Task<Lesson?> GetByIdAsync(Guid id);

        Task<TResult?> GetByIdAsync<TResult>(
            Guid id,
            Expression<Func<Lesson, Teacher, TResult>> selector);

        Task<bool> HasFutureScheduledLessonsByEnrollmentAsync(
            Guid enrollmentId,
            DateTime from);

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

        Task<List<LessonSlot>> GetScheduledSlotsAsync(
            Guid teacherId,
            Guid studentId,
            string room,
            DateTime from,
            DateTime to);

        Task<List<TResult>> GetByDateRangeAsync<TResult>(
            DateTime from,
            DateTime to,
            Guid? teacherId,
            Guid? studentId,
            Expression<Func<Lesson, Teacher, TResult>> selector);

        Task<Lesson> UpdateWithHistoryAsync(
            Lesson lesson,
            LessonChange lessonChange);

        Task AddRecurringAsync(
            LessonSeries series,
            List<Lesson> lessons);

        Task<bool> SeriesExistsAsync(Guid seriesId);

        Task<List<Lesson>> GetFutureScheduledBySeriesAsync(
            Guid seriesId,
            DateTime from);

        Task UpdateRangeAsync(IEnumerable<Lesson> lessons);
    }
}
