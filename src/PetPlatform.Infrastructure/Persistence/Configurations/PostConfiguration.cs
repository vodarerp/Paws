using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.LocationZone).HasMaxLength(100).IsRequired();
        builder.Property(p => p.AlertRadiusKm).HasDefaultValue(10);
        builder.Property(p => p.ReportCount).HasDefaultValue(0);
        builder.Property(p => p.IsHidden).HasDefaultValue(false);

        builder.HasIndex(p => new { p.AuthorId, p.CreatedAt });
        builder.HasIndex(p => new { p.Category, p.Status, p.CreatedAt });
        builder.HasIndex(p => new { p.LocationZone, p.Status });
        builder.HasIndex(p => p.ExpiresAt).HasFilter("[ExpiresAt] IS NOT NULL");

        builder.HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Pet)
            .WithMany()
            .HasForeignKey(p => p.PetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
