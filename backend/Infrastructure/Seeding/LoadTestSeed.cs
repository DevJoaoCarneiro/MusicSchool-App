using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeding;

public static class LoadTestSeed
{
    private const int StudentCount = 1_000;
    private const int TeacherCount = 100;
    private const int LessonCount = 10_000;
    private const string SeedEmailPrefix = "loadtest.student";

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Students.AnyAsync(student =>
                student.Email.StartsWith(SeedEmailPrefix)))
        {
            return;
        }

        var instruments = await context.Instruments
            .OrderBy(instrument => instrument.Name)
            .ToListAsync();

        if (instruments.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one instrument is required before running the load-test seed.");
        }

        var teachers = Enumerable.Range(1, TeacherCount)
            .Select(index => new Teacher(
                $"Load Test Teacher {index:000}",
                $"loadtest.teacher{index:000}@musicschool.local",
                $"119{index:0000000}",
                BuildCpf(index),
                instruments.Select(instrument => instrument.Name).ToList()))
            .ToList();

        await AddInBatchesAsync(context, teachers);

        var students = Enumerable.Range(1, StudentCount)
            .Select(index => new Student(
                $"Load Test Student {index:0000}",
                $"{SeedEmailPrefix}{index:0000}@musicschool.local",
                $"119{index:0000000}",
                BuildCpf(10_000 + index),
                new DateTime(1990, 1, 1).AddDays(index % 3_000),
                new Address(
                    "Load Test Street",
                    index.ToString(),
                    "São Paulo",
                    "SP",
                    "01000-000"),
                null))
            .ToList();

        await AddInBatchesAsync(context, students);

        var enrollments = students
            .Select((student, index) => new Enrollment(
                student.Id,
                instruments[index % instruments.Count].Id,
                DateTime.UtcNow.Date,
                150m,
                (index % 28) + 1))
            .ToList();

        await AddInBatchesAsync(context, enrollments, clearTracker: false);

        var baseDate = DateTime.Now.Date.AddDays(1).AddHours(8);
        var lessons = Enumerable.Range(0, LessonCount)
            .Select(index =>
            {
                var teacherIndex = index % teachers.Count;
                var enrollment = enrollments[index % enrollments.Count];
                var day = index % 42;
                var slot = (index / TeacherCount) % 10;
                var startsAt = baseDate
                    .AddDays(day)
                    .AddHours(slot);

                return new Lesson(
                    enrollment,
                    teachers[teacherIndex].Id,
                    startsAt,
                    30,
                    $"Room {(teacherIndex % 20) + 1:00}");
            })
            .ToList();

        context.Lessons.AddRange(lessons);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static async Task AddInBatchesAsync<TEntity>(
        AppDbContext context,
        IReadOnlyList<TEntity> entities,
        bool clearTracker = true)
        where TEntity : class
    {
        const int batchSize = 500;

        for (var offset = 0; offset < entities.Count; offset += batchSize)
        {
            var batch = entities
                .Skip(offset)
                .Take(batchSize)
                .ToList();

            context.Set<TEntity>().AddRange(batch);
            await context.SaveChangesAsync();
            if (clearTracker)
            {
                context.ChangeTracker.Clear();
            }
        }
    }

    private static string BuildCpf(int value)
    {
        return value.ToString("00000000000");
    }
}
