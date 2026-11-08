using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class TuitionConfiguration : IEntityTypeConfiguration<Tuition>
    {
        public void Configure(EntityTypeBuilder<Tuition> builder)
        {
            builder.ToTable("Tuitions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedNever();

            builder.Property(t => t.EnrollmentId)
                .IsRequired();

            builder.Property(t => t.ReferenceMonth)
                .IsRequired()
                .HasMaxLength(Tuition.ReferenceMonthLength);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(t => t.DueDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(t => t.PaidAt)
                .HasColumnType("date")
                .IsRequired(false);

            builder.Property(t => t.PaidAmount)
                .HasPrecision(10, 2)
                .IsRequired(false);

            builder.Property(t => t.Method)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired(false);

            builder.HasOne(t => t.Enrollment)
                .WithMany()
                .HasForeignKey(t => t.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => new { t.EnrollmentId, t.ReferenceMonth })
                .IsUnique();

            builder.HasIndex(t => t.ReferenceMonth);

            builder.HasIndex(t => new
            {
                t.Status,
                t.ReferenceMonth
            });
        }
    }
}