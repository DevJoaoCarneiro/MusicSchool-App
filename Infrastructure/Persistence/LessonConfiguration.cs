
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            builder.ToTable("Lessons", table =>
            {
                table.HasCheckConstraint(
                    "CK_Lessons_DurationMinutes",
                    "\"DurationMinutes\" >= 30 AND \"DurationMinutes\" <= 120");

                table.HasCheckConstraint(
                    "CK_Lessons_Room",
                    "length(trim(\"Room\")) > 0");
            });

            builder.HasKey(l => l.Id);

            // O domínio gera o Guid no construtor.
            builder.Property(l => l.Id)
                .ValueGeneratedNever();

            builder.Property(l => l.EnrollmentId)
                .IsRequired();

            builder.Property(l => l.TeacherId)
                .IsRequired();

            // O contrato atual utiliza horário local sem offset.
            builder.Property(l => l.StartsAt)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            builder.Ignore(l => l.EndsAt);

            builder.Property(l => l.DurationMinutes)
                .IsRequired();

            builder.Property(l => l.Room)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(l => l.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(l => l.Reason)
    .HasMaxLength(500)
    .IsRequired(false);

            builder.Property(l => l.CancelledAt)
                .HasColumnType("timestamp without time zone")
                .IsRequired(false);

            builder.HasOne(l => l.Enrollment)
                .WithMany()
                .HasForeignKey(l => l.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Teacher>()
                .WithMany()
                .HasForeignKey(l => l.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.StartsAt);
            builder.HasIndex(l => l.TeacherId);
        }
    }
}
