using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence
{
    public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
    {
        public void Configure(EntityTypeBuilder<Instrument> builder)
        {
            builder.ToTable("Instruments");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Name)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(i => i.Description)
                .IsRequired()
                .HasMaxLength(255);

            // Seed inicial de instrumentos
            builder.HasData(
                new
                {
                    Id = Guid.Parse("a1f5d9c2-3e4b-4b7a-8f1e-1a2b3c4d5e01"),
                    Name = "Violão",
                    Description = "Violão popular e clássico"
                },
                new
                {
                    Id = Guid.Parse("b2f6d9c3-3e4b-4b7a-8f1e-1a2b3c4d5e02"),
                    Name = "Guitarra",
                    Description = "Guitarra elétrica e acústica"
                },
                new
                {
                    Id = Guid.Parse("c3f7d9c4-3e4b-4b7a-8f1e-1a2b3c4d5e03"),
                    Name = "Bateria",
                    Description = "Bateria acústica"
                }
            );
        }
    }
}
