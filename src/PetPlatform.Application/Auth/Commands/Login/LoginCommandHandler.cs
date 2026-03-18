using MediatR;
using PetPlatform.Application.Auth.DTOs;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Exceptions;
using PetPlatform.Domain.Interfaces.Repositories;

namespace PetPlatform.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IAuthService authService)
    {
        _context = context;
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (user is null)
            throw new DomainException("Neispravni kredencijali.", "INVALID_CREDENTIALS");

        if (user.IsBanned)
            throw new DomainException("Nalog je suspendovan.", "ACCOUNT_BANNED");

        if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
            throw new DomainException("Neispravni kredencijali.", "INVALID_CREDENTIALS");

        user.RecordActivity();
        await _context.SaveChangesAsync(ct);

        var tokens = _authService.GenerateTokens(user);

        return new AuthResponse(user.Id, user.Email, user.DisplayName, tokens.AccessToken, tokens.RefreshToken);
    }
}
