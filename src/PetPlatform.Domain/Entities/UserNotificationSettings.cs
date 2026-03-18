using PetPlatform.Domain.Common;

namespace PetPlatform.Domain.Entities;

public class UserNotificationSettings : BaseEntity
{
    public Guid UserId { get; private set; }
    public bool AmberAlertEnabled { get; private set; } = true;
    public bool StatusChangesEnabled { get; private set; } = true;

    public User User { get; private set; } = default!;

    protected UserNotificationSettings() { }

    public static UserNotificationSettings CreateDefault(Guid userId)
    {
        return new UserNotificationSettings { UserId = userId };
    }

    public void Update(bool amberAlert, bool statusChanges)
    {
        AmberAlertEnabled = amberAlert;
        StatusChangesEnabled = statusChanges;
        SetUpdated();
    }
}
