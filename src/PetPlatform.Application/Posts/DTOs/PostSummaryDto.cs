namespace PetPlatform.Application.Posts.DTOs;

public record PostSummaryDto(
    Guid Id,
    string Category,
    string Title,
    string Description,
    string? PrimaryImageUrl,
    string LocationZone,
    string AuthorDisplayName,
    string AuthorTrustCategory,
    bool IsUrgent,
    string Status,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    int SightingCount
);
