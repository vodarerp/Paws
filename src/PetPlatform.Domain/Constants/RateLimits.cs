namespace PetPlatform.Domain.Constants;

public static class RateLimits
{
    public const int MaxPostsPerDay = 3;
    public const int MaxLostAlertsPerDay = 1;
    public const int AmberAlertRadiusMeters = 10_000;
    public const int LostPostExpirationDays = 7;
}
