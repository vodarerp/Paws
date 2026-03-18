namespace PetPlatform.Domain.Interfaces.Services;

public interface IImageHashingService
{
    Task<string> ComputeHashAsync(Stream imageStream, CancellationToken ct = default);
    Task<bool> IsDuplicateAsync(string hash, CancellationToken ct = default);
}
