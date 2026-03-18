using PetPlatform.Application.Auth.DTOs;

namespace PetPlatform.Application.Common.Interfaces;

public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    AuthTokens GenerateTokens(Domain.Entities.User user);
}

public record AuthTokens(string AccessToken, string RefreshToken);
