using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .ValueGeneratedOnAdd();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(s => s.Email)
                .IsUnique();

            builder.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Cpf)
                .IsRequired()
                .HasMaxLength(11);

            builder.HasIndex(s => s.Cpf)
                .IsUnique();

            builder.Property(s => s.BirthDate)
                .IsRequired()
                .HasColumnType("date");

            builder.OwnsOne(s => s.Address, address =>
            {
                address.Property(a => a.Street)
                    .HasColumnName("Street")
                    .IsRequired()
                    .HasMaxLength(100);

                address.Property(a => a.Number)
                    .HasColumnName("Number")
                    .IsRequired()
                    .HasMaxLength(20);

                address.Property(a => a.City)
                    .HasColumnName("City")
                    .IsRequired()
                    .HasMaxLength(100);

                address.Property(a => a.State)
                    .HasColumnName("State")
                    .IsRequired()
                    .HasMaxLength(2);

                address.Property(a => a.ZipCode)
                    .HasColumnName("ZipCode")
                    .IsRequired()
                    .HasMaxLength(10);
            });

          
            builder.OwnsOne(s => s.Guardian, guardian =>
            {
                guardian.Property(g => g.Name)
                    .HasColumnName("GuardianName")
                    .HasMaxLength(100);

                guardian.Property(g => g.Phone)
                    .HasColumnName("GuardianPhone")
                    .HasMaxLength(20);

                guardian.Property(g => g.Email)
                    .HasColumnName("GuardianEmail")
                    .HasMaxLength(100);

                guardian.Property(g => g.Cpf)
                    .HasColumnName("GuardianCpf")
                    .HasMaxLength(11);
            });
        }
    }
}