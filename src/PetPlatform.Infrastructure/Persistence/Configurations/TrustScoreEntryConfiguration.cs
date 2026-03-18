using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence.Configurations;

public class TrustScoreEntryConfiguration : IEntityTypeConfiguration<TrustScoreEntry>
{
    public void Configure(EntityTypeBuilder<TrustScoreEntry> builder)
    {
        builder.HasKey(t => t.Id);
        builder.ToTable("TrustScoreHistory");

        builder.Property(t => t.Description).HasMaxLength(200);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.UserId, t.CreatedAt });
    }
}
