using MediatR;

namespace PetPlatform.Application.Posts.Commands.UpdatePost;

public record UpdatePostCommand(
    Guid PostId,
    Guid UserId,
    string Title,
    string Description,
    string LocationZone,
    double? Latitude,
    double? Longitude,
    string? ContactPreference
) : IRequest;
