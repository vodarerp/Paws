using PetPlatform.Domain.Enums;

namespace PetPlatform.Domain.Constants;

public static class TrustScorePoints
{
    private static readonly Dictionary<TrustScoreActionType, int> Points = new()
    {
        { TrustScoreActionType.PostCreated, +2 },
        { TrustScoreActionType.LostAlertResolved, +5 },
        { TrustScoreActionType.IdentityVerified, +20 },
        { TrustScoreActionType.ReportConfirmed, -30 },
        { TrustScoreActionType.SlowResponse, -5 },
    };

    public static int GetPoints(TrustScoreActionType action) =>
        Points.TryGetValue(action, out var p) ? p : 0;
}
