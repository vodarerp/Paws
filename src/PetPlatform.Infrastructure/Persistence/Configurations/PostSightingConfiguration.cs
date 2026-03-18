using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence.Configurations;

public class PostSightingConfiguration : IEntityTypeConfiguration<PostSighting>
{
    public void Configure(EntityTypeBuilder<PostSighting> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.LocationDescription).HasMaxLength(200);
        builder.Property(s => s.Comment).HasMaxLength(500);

        builder.HasOne(s => s.Post)
            .WithMany(p => p.Sightings)
            .HasForeignKey(s => s.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Reporter)
            .WithMany()
            .HasForeignKey(s => s.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.PostId, s.SeenAt });
    }
}
