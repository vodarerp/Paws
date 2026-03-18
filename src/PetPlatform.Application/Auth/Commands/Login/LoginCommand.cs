using MediatR;
using PetPlatform.Application.Auth.DTOs;

namespace PetPlatform.Application.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
