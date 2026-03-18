using MediatR;
using PetPlatform.Application.Auth.DTOs;

namespace PetPlatform.Application.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string DisplayName,
    string LocationZone
) : IRequest<AuthResponse>;
