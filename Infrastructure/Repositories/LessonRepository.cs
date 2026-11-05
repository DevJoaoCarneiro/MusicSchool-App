
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly AppDbContext _context;

        public LessonRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Lesson> AddAsync(Lesson lesson)
        {
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            return lesson;
        }

        public async Task AddRecurringAsync(
    LessonSeries series,
    List<Lesson> lessons)
        {
            _context.LessonSeries.Add(series);

            _context.Lessons.AddRange(lessons);

            await _context.SaveChangesAsync();
        }

        public async Task<Lesson?> GetByIdAsync(Guid id)
        {
            return await _context.Lessons
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<bool> HasFutureScheduledLessonsByEnrollmentAsync(
    Guid enrollmentId,
    DateTime from)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.EnrollmentId == enrollmentId &&
                l.Status == LessonStatus.Scheduled &&
                l.StartsAt > from);
        }

        public async Task<Lesson> UpdateAsync(Lesson lesson)
        {
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();

            return lesson;
        }

        public async Task<bool> HasTeacherConflictAsync(
    Guid teacherId,
    DateTime startsAt,
    DateTime endsAt,
    Guid? excludeLessonId = null)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.TeacherId == teacherId &&
                l.Status == LessonStatus.Scheduled &&
                (!excludeLessonId.HasValue || l.Id != excludeLessonId.Value) &&
                l.StartsAt < endsAt &&
                l.StartsAt.AddMinutes(l.DurationMinutes) > startsAt);
        }

        public async Task<bool> HasStudentConflictAsync(
    Guid studentId,
    DateTime startsAt,
    DateTime endsAt,
    Guid? excludeLessonId = null)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.Enrollment.StudentId == studentId &&
                l.Status == LessonStatus.Scheduled &&
                (!excludeLessonId.HasValue || l.Id != excludeLessonId.Value) &&
                l.StartsAt < endsAt &&
                l.StartsAt.AddMinutes(l.DurationMinutes) > startsAt);
        }

        public async Task<bool> HasRoomConflictAsync(
    string room,
    DateTime startsAt,
    DateTime endsAt,
    Guid? excludeLessonId = null)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.Room.ToLower() == room.Trim().ToLower() &&
                l.Status == LessonStatus.Scheduled &&
                (!excludeLessonId.HasValue || l.Id != excludeLessonId.Value) &&
                l.StartsAt < endsAt &&
                l.StartsAt.AddMinutes(l.DurationMinutes) > startsAt);
        }


        public async Task<List<TResult>> GetByDateRangeAsync<TResult>(
            DateTime from,
            DateTime to,
            Guid? teacherId,
            Guid? studentId,
            Expression<Func<Lesson, Teacher, TResult>> selector)
        {
            var query = _context.Lessons
                .AsNoTracking()
                .Where(l =>
                    l.StartsAt >= from &&
                    l.StartsAt < to.Date.AddDays(1));

            if (teacherId.HasValue)
                query = query.Where(l => l.TeacherId == teacherId.Value);

            if (studentId.HasValue)
                query = query.Where(l =>
                    l.Enrollment.StudentId == studentId.Value);

            return await query
                .OrderBy(l => l.StartsAt)
                .Join(
                    _context.Teachers.AsNoTracking(),
                    lesson => lesson.TeacherId,
                    teacher => teacher.Id,
                    selector)
                .ToListAsync();
        }


        public async Task<Lesson> UpdateWithHistoryAsync(
    Lesson lesson,
    LessonChange lessonChange)
        {
            _context.Lessons.Update(lesson);
            _context.LessonChanges.Add(lessonChange);

            await _context.SaveChangesAsync();

            return lesson;
        }
    }
}
