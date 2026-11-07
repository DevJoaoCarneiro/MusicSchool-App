
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class LessonSeriesConfiguration
        : IEntityTypeConfiguration<LessonSeries>
    {
        public void Configure(EntityTypeBuilder<LessonSeries> builder)
        {
            builder.ToTable("LessonSeries");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedNever();

            builder.Property(s => s.EnrollmentId)
                .IsRequired();

            builder.Property(s => s.TeacherId)
                .IsRequired();

            builder.Property(s => s.Weekday)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(s => s.Time)
                .IsRequired()
                .HasColumnType("time without time zone");

            builder.Property(s => s.StartDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(s => s.EndDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(s => s.DurationMinutes)
                .IsRequired();

            builder.Property(s => s.Room)
                .IsRequired()
                .HasMaxLength(60);

            builder.HasOne<Enrollment>()
                .WithMany()
                .HasForeignKey(s => s.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(s => s.EnrollmentId);
            builder.HasIndex(s => s.TeacherId);
        }
    }
}
