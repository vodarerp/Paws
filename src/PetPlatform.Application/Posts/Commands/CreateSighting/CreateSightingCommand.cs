using MediatR;

namespace PetPlatform.Application.Posts.Commands.CreateSighting;

public record CreateSightingCommand(
    Guid PostId,
    Guid ReporterId,
    double Latitude,
    double Longitude,
    DateTime SeenAt,
    string? LocationDescription,
    string? Comment
) : IRequest<Guid>;
