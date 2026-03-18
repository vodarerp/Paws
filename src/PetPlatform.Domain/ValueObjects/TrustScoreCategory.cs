namespace PetPlatform.Domain.ValueObjects;

public static class TrustScoreCategory
{
    public const string New = "New";           // 0-19
    public const string Active = "Active";     // 20-49
    public const string Trusted = "Trusted";   // 50-99
    public const string LocalHero = "LocalHero"; // 100+

    public static string FromScore(int score) => score switch
    {
        < 20 => New,
        < 50 => Active,
        < 100 => Trusted,
        _ => LocalHero
    };
}
