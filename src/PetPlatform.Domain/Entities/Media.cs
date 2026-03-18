using PetPlatform.Domain.Common;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Domain.Entities;

public class Media : BaseEntity
{
    public MediaEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public string Url { get; private set; } = default!;
    public MediaType MediaType { get; private set; }
    public string? ImageHash { get; private set; }
    public ModerationStatus ModerationStatus { get; private set; } = ModerationStatus.Pending;
    public int SortOrder { get; private set; }

    protected Media() { }

    public static Media Create(MediaEntityType entityType, Guid entityId,
        string url, MediaType mediaType, int sortOrder = 0)
    {
        return new Media
        {
            EntityType = entityType,
            EntityId = entityId,
            Url = url,
            MediaType = mediaType,
            SortOrder = sortOrder
        };
    }

    public void SetImageHash(string hash)
    {
        ImageHash = hash;
        SetUpdated();
    }

    public void Approve()
    {
        ModerationStatus = ModerationStatus.Approved;
        SetUpdated();
    }

    public void Reject()
    {
        ModerationStatus = ModerationStatus.Rejected;
        SetUpdated();
    }

    public void AssociateWith(MediaEntityType entityType, Guid entityId)
    {
        EntityType = entityType;
        EntityId = entityId;
        SetUpdated();
    }
}
