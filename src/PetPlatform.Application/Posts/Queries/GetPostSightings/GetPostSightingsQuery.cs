using MediatR;
using PetPlatform.Application.Posts.DTOs;

namespace PetPlatform.Application.Posts.Queries.GetPostSightings;

public record GetPostSightingsQuery(Guid PostId) : IRequest<List<SightingDto>>;
