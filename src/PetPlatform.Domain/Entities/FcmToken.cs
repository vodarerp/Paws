using PetPlatform.Domain.Common;

namespace PetPlatform.Domain.Entities;

public class FcmToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public string? Platform { get; private set; }
    public DateTime? LastUsedAt { get; private set; }

    public User User { get; private set; } = default!;

    protected FcmToken() { }

    public static FcmToken Create(Guid userId, string token, string? platform = null)
    {
        return new FcmToken
        {
            UserId = userId,
            Token = token,
            Platform = platform,
            LastUsedAt = DateTime.UtcNow
        };
    }

    public void Touch()
    {
        LastUsedAt = DateTime.UtcNow;
        SetUpdated();
    }
}
