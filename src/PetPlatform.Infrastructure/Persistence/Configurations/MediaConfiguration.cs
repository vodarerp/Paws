using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence.Configurations;

public class MediaConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Url).HasMaxLength(500).IsRequired();
        builder.Property(m => m.ImageHash).HasMaxLength(64);

        builder.HasIndex(m => new { m.EntityType, m.EntityId });
        builder.HasIndex(m => m.ImageHash).HasFilter("[ImageHash] IS NOT NULL");
    }
}
