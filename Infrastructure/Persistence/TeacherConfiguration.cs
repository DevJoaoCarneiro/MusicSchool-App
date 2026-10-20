using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
    {
        public void Configure(EntityTypeBuilder<Teacher> builder)
        {
            builder.ToTable("Teachers");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Id)
                .ValueGeneratedOnAdd();

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(t => t.Email)
                .IsUnique();

            builder.Property(t => t.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.Cpf)
                .IsRequired()
                .HasMaxLength(11);

            builder.HasIndex(t => t.Cpf)
                .IsUnique();

            builder.Property(t => t.Specialties)
                .HasColumnType("text[]")
                .IsRequired();
        }
    }
}