using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Pets.Commands.CreatePet;
using PetPlatform.Application.Pets.Commands.UpdatePet;
using PetPlatform.Application.Pets.Queries.GetMyPets;
using PetPlatform.Application.Pets.Queries.GetPetById;

namespace PetPlatform.API.Controllers;

[ApiController]
[Route("api/v1/pets")]
[Authorize]
public class PetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PetsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPets(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var query = new GetMyPetsQuery(userId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetPetByIdQuery(id);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePetRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new CreatePetCommand(
            userId, request.Name, request.Breed, request.Gender, request.Size,
            request.Age, request.Color, request.SpecialMarks, request.ChipNumber,
            request.IsSterilized, request.MediaIds ?? new List<Guid>());

        var petId = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = petId }, new { petId });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePetRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new UpdatePetCommand(
            id, userId, request.Name, request.Breed, request.Gender, request.Size,
            request.Age, request.Color, request.SpecialMarks, request.ChipNumber, request.IsSterilized);

        await _mediator.Send(command, ct);
        return NoContent();
    }
}

public record CreatePetRequest(
    string Name,
    string Breed,
    string Gender,
    string Size,
    string? Age,
    string? Color,
    string? SpecialMarks,
    string? ChipNumber,
    bool? IsSterilized,
    List<Guid>? MediaIds);

public record UpdatePetRequest(
    string Name,
    string Breed,
    string Gender,
    string Size,
    string? Age,
    string? Color,
    string? SpecialMarks,
    string? ChipNumber,
    bool? IsSterilized);
