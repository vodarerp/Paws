using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetPlatform.Application.Auth.Commands.Login;
using PetPlatform.Application.Auth.Commands.Register;

namespace PetPlatform.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.DisplayName, request.LocationZone);
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(null, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public record RegisterRequest(string Email, string Password, string DisplayName, string LocationZone);
public record LoginRequest(string Email, string Password);
