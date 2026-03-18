using MediatR;

namespace PetPlatform.Application.Posts.Commands.CreatePost;

public record CreatePostCommand(
    Guid AuthorId,
    string Category,
    string Title,
    string Description,
    string LocationZone,
    Guid? PetId,
    double? Latitude,
    double? Longitude,
    string? ContactPreference,
    List<Guid> MediaIds
) : IRequest<CreatePostResponse>;

public record CreatePostResponse(
    Guid PostId,
    string Status,
    bool AmberAlertTriggered,
    DateTime? ExpiresAt
);
