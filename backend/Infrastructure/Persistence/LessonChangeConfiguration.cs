
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class LessonChangeConfiguration
        : IEntityTypeConfiguration<LessonChange>
    {
        public void Configure(EntityTypeBuilder<LessonChange> builder)
        {
            builder.ToTable("LessonChanges");

            builder.HasKey(lc => lc.Id);

            builder.Property(lc => lc.Id)
                .ValueGeneratedNever();

            builder.Property(lc => lc.LessonId)
                .IsRequired();

            builder.Property(lc => lc.OldStartsAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            builder.Property(lc => lc.NewStartsAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            builder.Property(lc => lc.OldRoom)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(lc => lc.NewRoom)
                .HasMaxLength(60)
                .IsRequired();

            builder.Property(lc => lc.Reason)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(lc => lc.ChangedAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired();

            builder.HasOne(lc => lc.Lesson)
                .WithMany()
                .HasForeignKey(lc => lc.LessonId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
