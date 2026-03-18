using PetPlatform.Domain.Common;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Domain.Entities;

public class TrustScoreEntry : BaseEntity
{
    public Guid UserId { get; private set; }
    public TrustScoreActionType ActionType { get; private set; }
    public int Points { get; private set; }
    public string? Description { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    public User User { get; private set; } = default!;

    protected TrustScoreEntry() { }

    public static TrustScoreEntry Create(Guid userId, TrustScoreActionType actionType,
        int points, string? description = null, Guid? relatedEntityId = null)
    {
        return new TrustScoreEntry
        {
            UserId = userId,
            ActionType = actionType,
            Points = points,
            Description = description,
            RelatedEntityId = relatedEntityId
        };
    }
}
