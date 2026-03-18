namespace PetPlatform.Domain.Interfaces.Services;

public interface IContentModerationService
{
    Task<bool> IsContentSafeAsync(Stream imageStream, CancellationToken ct = default);
}
