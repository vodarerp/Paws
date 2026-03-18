using MediatR;
using PetPlatform.Application.Auth.DTOs;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Exceptions;
using PetPlatform.Domain.Interfaces.Repositories;

namespace PetPlatform.Application.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public RegisterCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IAuthService authService)
    {
        _context = context;
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            throw new DomainException("Korisnik sa ovim email-om već postoji.", "EMAIL_ALREADY_EXISTS");

        var passwordHash = _authService.HashPassword(request.Password);
        var user = User.Create(request.Email, passwordHash, request.DisplayName, request.LocationZone);

        await _userRepository.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);

        var tokens = _authService.GenerateTokens(user);

        return new AuthResponse(user.Id, user.Email, user.DisplayName, tokens.AccessToken, tokens.RefreshToken);
    }
}
