using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence.Configurations;

public class PetConfiguration : IEntityTypeConfiguration<Pet>
{
    public void Configure(EntityTypeBuilder<Pet> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Breed).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Age).HasMaxLength(50);
        builder.Property(p => p.Color).HasMaxLength(100);
        builder.Property(p => p.SpecialMarks).HasMaxLength(500);
        builder.Property(p => p.ChipNumber).HasMaxLength(50);

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.Status);
    }
}
