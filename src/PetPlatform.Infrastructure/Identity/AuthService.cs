using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Entities;

namespace PetPlatform.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IJwtTokenGenerator jwtTokenGenerator, IPasswordHasher passwordHasher)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public string HashPassword(string password) => _passwordHasher.Hash(password);
    public bool VerifyPassword(string password, string hash) => _passwordHasher.Verify(password, hash);

    public AuthTokens GenerateTokens(User user)
    {
        var accessToken = _jwtTokenGenerator.GenerateToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        return new AuthTokens(accessToken, refreshToken);
    }
}
