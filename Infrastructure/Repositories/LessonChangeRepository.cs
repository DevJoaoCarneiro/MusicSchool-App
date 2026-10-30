using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;

namespace Infrastructure.Repositories
{
    public class LessonChangeRepository : ILessonChangeRepository
    {
        private readonly AppDbContext _context;

        public LessonChangeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LessonChange> AddAsync(LessonChange lessonChange)
        {
            _context.LessonChanges.Add(lessonChange);
            await _context.SaveChangesAsync();

            return lessonChange;
        }
    }
}