namespace PetPlatform.Application.Pets.DTOs;

public record PetDto(
    Guid Id,
    string Name,
    string Breed,
    string? Age,
    string Gender,
    string Size,
    string? Color,
    string? SpecialMarks,
    string? ChipNumber,
    bool? IsSterilized,
    string Status,
    string? PhotoUrl,
    DateTime CreatedAt
);
