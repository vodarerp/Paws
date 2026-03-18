using PetPlatform.Domain.Common;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Domain.Entities;

public class InAppNotification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }

    public User User { get; private set; } = default!;

    protected InAppNotification() { }

    public static InAppNotification Create(Guid userId, NotificationType type,
        string title, string body)
    {
        return new InAppNotification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
        SetUpdated();
    }
}
