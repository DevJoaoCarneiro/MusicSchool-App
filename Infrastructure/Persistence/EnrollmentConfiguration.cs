using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Infrastructure.Persistence
{
    public class EnrollmentConfiguration
        : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments", table =>
            {
                table.HasCheckConstraint(
                    "CK_Enrollments_DueDay",
                    "\"DueDay\" >= 1 AND \"DueDay\" <= 28");
            });

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Property(e => e.StudentId)
                .IsRequired();

            builder.Property(e => e.InstrumentId)
                .IsRequired();

            builder.Property(e => e.StartDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(e => e.MonthlyFee)
                .IsRequired()
                .HasPrecision(10, 2);

            builder.Property(e => e.DueDay)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Instrument)
                .WithMany()
                .HasForeignKey(e => e.InstrumentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Impede matr�culas ativas duplicadas para o mesmo
            // aluno e instrumento no PostgreSQL.
            builder.HasIndex(e => new
            {
                e.StudentId,
                e.InstrumentId
            })
                .IsUnique()
                .HasFilter("\"Status\" = 'Active'");
        }
    }
}
