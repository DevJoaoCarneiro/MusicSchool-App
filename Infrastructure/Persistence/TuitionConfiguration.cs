using System;
using System.Collections.Generic;
using System.Text;
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
                .ValueGeneratedOnAdd();

            builder.Property(t => t.EnrollmentId)
                .IsRequired();

            builder.Property(t => t.ReferenceMonth)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.DueDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(t => t.Enrollment)
                .WithMany()
                .HasForeignKey(t => t.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => new
            {
                t.EnrollmentId,
                t.ReferenceMonth
            }).IsUnique();
        }
    }
}