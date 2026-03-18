using MediatR;
using PetPlatform.Application.Posts.DTOs;

namespace PetPlatform.Application.Posts.Queries.GetPostById;

public record GetPostByIdQuery(Guid PostId) : IRequest<PostDetailDto>;
