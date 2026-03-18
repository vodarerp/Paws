namespace PetPlatform.Domain.Interfaces.Services;

public interface INotificationService
{
    Task SendPushAsync(Guid userId, string title, string body,
        Dictionary<string, string>? data = null, CancellationToken ct = default);
    Task SendPushToManyAsync(IEnumerable<Guid> userIds, string title, string body,
        Dictionary<string, string>? data = null, CancellationToken ct = default);
}
