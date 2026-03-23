using NetTopologySuite.Geometries;
using PetPlatform.Domain.Common;
using PetPlatform.Domain.Constants;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.ValueObjects;

namespace PetPlatform.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? AvatarUrl { get; private set; }
    public UserRole Role { get; private set; }
    public int TrustScore { get; private set; }
    public string LocationZone { get; private set; } = default!;
    public Point? Location { get; private set; }
    public bool GpsConsentGiven { get; private set; }
    public string? OrganizationName { get; private set; }
    public string? OrganizationUrl { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsBanned { get; private set; }
    public bool OnboardingCompleted { get; private set; }
    public OnboardingIntent? OnboardingIntent { get; private set; }
    public DateTime? LastActiveAt { get; private set; }

    public ICollection<Pet> Pets { get; private set; } = new List<Pet>();
    public ICollection<Post> Posts { get; private set; } = new List<Post>();

    protected User() { }

    public static User Create(string email, string passwordHash, string displayName,
        string locationZone, UserRole role = UserRole.Individual)
    {
        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            DisplayName = displayName.Trim(),
            LocationZone = locationZone.Trim(),
            Role = role
        };
    }

    public double? LastKnownLatitude => Location?.Y;
    public double? LastKnownLongitude => Location?.X;

    public void UpdateLocation(double latitude, double longitude, bool gpsConsent)
    {
        var validated = LocationValue.Create(latitude, longitude);
        Location = GeoPointFactory.Create(validated.Longitude, validated.Latitude);
        GpsConsentGiven = gpsConsent;
        SetUpdated();
    }

    public void UpdateProfile(string displayName, string? avatarUrl = null)
    {
        DisplayName = displayName.Trim();
        AvatarUrl = avatarUrl;
        SetUpdated();
    }

    public void CompleteOnboarding(OnboardingIntent intent)
    {
        OnboardingCompleted = true;
        OnboardingIntent = intent;
        SetUpdated();
    }

    public void ApplyTrustScoreChange(TrustScoreActionType action)
    {
        var points = TrustScorePoints.GetPoints(action);
        TrustScore = Math.Max(0, TrustScore + points);
        SetUpdated();
    }

    public string GetTrustScoreCategory() => TrustScoreCategory.FromScore(TrustScore);

    public void Verify()
    {
        if (IsVerified) return;
        IsVerified = true;
        ApplyTrustScoreChange(TrustScoreActionType.IdentityVerified);
    }

    public void Ban()
    {
        IsBanned = true;
        SetUpdated();
    }

    public void RecordActivity()
    {
        LastActiveAt = DateTime.UtcNow;
    }
}
