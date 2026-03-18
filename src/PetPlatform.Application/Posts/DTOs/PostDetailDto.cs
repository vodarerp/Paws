namespace PetPlatform.Application.Posts.DTOs;

public record PostDetailDto(
    Guid Id,
    string Category,
    string Title,
    string Description,
    List<string> MediaUrls,
    string LocationZone,
    double? Latitude,
    double? Longitude,
    DateTime? LastSeenAt,
    string? ContactPreference,
    string Status,
    string? ResolutionType,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    AuthorDto Author,
    PetSummaryDto? Pet,
    int SightingCount,
    int ReportCount
);

public record AuthorDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    string TrustCategory,
    bool IsVerified
);

public record PetSummaryDto(
    Guid Id,
    string Name,
    string Breed,
    string? Age,
    string Gender,
    string Size,
    string? Color,
    string? PrimaryPhotoUrl
);
