using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.LocationZone).HasMaxLength(100).IsRequired();
        builder.Property(u => u.OrganizationName).HasMaxLength(200);
        builder.Property(u => u.OrganizationUrl).HasMaxLength(500);
        builder.Property(u => u.Location).HasColumnType("geography");
        builder.Ignore(u => u.LastKnownLatitude);
        builder.Ignore(u => u.LastKnownLongitude);

        builder.Property(u => u.TrustScore).HasDefaultValue(0);
        builder.Property(u => u.IsVerified).HasDefaultValue(false);
        builder.Property(u => u.IsBanned).HasDefaultValue(false);
        builder.Property(u => u.GpsConsentGiven).HasDefaultValue(false);
        builder.Property(u => u.OnboardingCompleted).HasDefaultValue(false);

        builder.HasIndex(u => new { u.LocationZone, u.IsBanned });

        builder.HasMany(u => u.Pets)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId);

        builder.HasMany(u => u.Posts)
            .WithOne(p => p.Author)
            .HasForeignKey(p => p.AuthorId);
    }
}
