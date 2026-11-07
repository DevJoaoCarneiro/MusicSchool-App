
namespace Domain.Entities
{
    public class LessonChange
    {
        public Guid Id { get; private set; }

        public Guid LessonId { get; private set; }
        public Lesson Lesson { get; private set; } = null!;

        public DateTime OldStartsAt { get; private set; }
        public DateTime NewStartsAt { get; private set; }

        public string OldRoom { get; private set; } = string.Empty;
        public string NewRoom { get; private set; } = string.Empty;

        public string Reason { get; private set; } = string.Empty;

        public DateTime ChangedAt { get; private set; }

        // Constructor para o Entity Framework Core
        public LessonChange()
        {
        }

        public LessonChange(
            Lesson lesson,
            DateTime oldStartsAt,
            DateTime newStartsAt,
            string oldRoom,
            string newRoom,
            string reason)
        {
            if (lesson == null)
                throw new ArgumentNullException(nameof(lesson));

            if (string.IsNullOrWhiteSpace(reason) ||
                reason.Trim().Length < 5)
                throw new ArgumentException(
                    "Rescheduling reason must have at least 5 characters",
                    nameof(reason));

            Id = Guid.NewGuid();
            LessonId = lesson.Id;
            Lesson = lesson;

            OldStartsAt = oldStartsAt;
            NewStartsAt = newStartsAt;

            OldRoom = oldRoom;
            NewRoom = newRoom;

            Reason = reason.Trim();
            ChangedAt = DateTime.Now;
        }
    }
}
