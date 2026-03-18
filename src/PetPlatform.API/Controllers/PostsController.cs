using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Posts.Commands.ChangePostStatus;
using PetPlatform.Application.Posts.Commands.CreatePost;
using PetPlatform.Application.Posts.Commands.CreateSighting;
using PetPlatform.Application.Posts.Commands.ReportPost;
using PetPlatform.Application.Posts.Commands.UpdatePost;
using PetPlatform.Application.Posts.Queries.GetFeed;
using PetPlatform.Application.Posts.Queries.GetPostById;
using PetPlatform.Application.Posts.Queries.GetPostSightings;

namespace PetPlatform.API.Controllers;

[ApiController]
[Route("api/v1/posts")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PostsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? locationZone,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetFeedQuery(locationZone, category, page, pageSize);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetPostByIdQuery(id);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new CreatePostCommand(
            userId,
            request.Category,
            request.Title,
            request.Description,
            request.LocationZone,
            request.PetId,
            request.Latitude,
            request.Longitude,
            request.ContactPreference,
            request.MediaIds ?? new List<Guid>());

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.PostId }, result);
    }

    [Authorize]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new ChangePostStatusCommand(id, userId, request.Action, request.ResolutionType);
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new UpdatePostCommand(
            id, userId, request.Title, request.Description, request.LocationZone,
            request.Latitude, request.Longitude, request.ContactPreference);

        await _mediator.Send(command, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/report")]
    public async Task<IActionResult> Report(Guid id, [FromBody] ReportPostRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new ReportPostCommand(id, userId, request.Reason, request.Description);
        await _mediator.Send(command, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("{id:guid}/sightings")]
    public async Task<IActionResult> CreateSighting(Guid id, [FromBody] CreateSightingRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException();

        var command = new CreateSightingCommand(
            id, userId, request.Latitude, request.Longitude,
            request.SeenAt, request.LocationDescription, request.Comment);

        var sightingId = await _mediator.Send(command, ct);
        return Created($"/api/v1/posts/{id}/sightings", new { sightingId });
    }

    [HttpGet("{id:guid}/sightings")]
    public async Task<IActionResult> GetSightings(Guid id, CancellationToken ct)
    {
        var query = new GetPostSightingsQuery(id);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}

public record CreatePostRequest(
    string Category,
    string Title,
    string Description,
    string LocationZone,
    Guid? PetId,
    double? Latitude,
    double? Longitude,
    string? ContactPreference,
    List<Guid>? MediaIds);

public record ChangeStatusRequest(
    string Action,
    string? ResolutionType);

public record UpdatePostRequest(
    string Title,
    string Description,
    string LocationZone,
    double? Latitude,
    double? Longitude,
    string? ContactPreference);

public record ReportPostRequest(
    string Reason,
    string? Description);

public record CreateSightingRequest(
    double Latitude,
    double Longitude,
    DateTime SeenAt,
    string? LocationDescription,
    string? Comment);
