using MediatR;
using PetPlatform.Application.Posts.DTOs;

namespace PetPlatform.Application.Posts.Queries.GetFeed;

public record GetFeedQuery(
    string? LocationZone,
    string? CategoryFilter,
    int Page = 1,
    int PageSize = 20
) : IRequest<GetFeedResponse>;

public record GetFeedResponse(
    List<PostSummaryDto> Posts,
    int TotalCount,
    bool HasMore
);
