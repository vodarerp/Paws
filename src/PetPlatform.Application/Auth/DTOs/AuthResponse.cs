namespace PetPlatform.Application.Auth.DTOs;

public record AuthResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    string Token,
    string RefreshToken
);
