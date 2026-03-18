using MediatR;

namespace PetPlatform.Application.Posts.Commands.ChangePostStatus;

public record ChangePostStatusCommand(
    Guid PostId,
    Guid UserId,
    string Action,
    string? ResolutionType
) : IRequest;
