
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

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

        public async Task<bool> HasTeacherConflictAsync(
            Guid teacherId,
            DateTime startsAt,
            DateTime endsAt)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.TeacherId == teacherId &&
                l.Status == LessonStatus.Scheduled &&
                l.StartsAt < endsAt &&
                l.StartsAt.AddMinutes(l.DurationMinutes) > startsAt);
        }

        public async Task<bool> HasStudentConflictAsync(
            Guid studentId,
            DateTime startsAt,
            DateTime endsAt)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.Enrollment.StudentId == studentId &&
                l.Status == LessonStatus.Scheduled &&
                l.StartsAt < endsAt &&
                l.StartsAt.AddMinutes(l.DurationMinutes) > startsAt);
        }

        public async Task<bool> HasRoomConflictAsync(
            string room,
            DateTime startsAt,
            DateTime endsAt)
        {
            return await _context.Lessons.AnyAsync(l =>
                l.Room.ToLower() == room.Trim().ToLower() &&
                l.Status == LessonStatus.Scheduled &&
                l.StartsAt < endsAt &&
                l.StartsAt.AddMinutes(l.DurationMinutes) > startsAt);
        }
    }
}
