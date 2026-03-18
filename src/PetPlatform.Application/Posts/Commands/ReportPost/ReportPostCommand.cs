using MediatR;

namespace PetPlatform.Application.Posts.Commands.ReportPost;

public record ReportPostCommand(
    Guid PostId,
    Guid ReporterId,
    string Reason,
    string? Description
) : IRequest;
