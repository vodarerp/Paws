namespace PetPlatform.Domain.Interfaces.Services;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteImageAsync(string imageUrl, CancellationToken ct = default);
    Task<string> GenerateSasUrlAsync(string blobName, CancellationToken ct = default);
}
