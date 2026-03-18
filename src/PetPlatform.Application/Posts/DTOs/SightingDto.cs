namespace PetPlatform.Application.Posts.DTOs;

public record SightingDto(
    Guid Id,
    string ReporterDisplayName,
    double Latitude,
    double Longitude,
    string? LocationDescription,
    DateTime SeenAt,
    string? Comment,
    DateTime CreatedAt
);
