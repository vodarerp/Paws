using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Posts.DTOs;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.ValueObjects;

namespace PetPlatform.Application.Posts.Queries.GetFeed;

public class GetFeedHandler : IRequestHandler<GetFeedQuery, GetFeedResponse>
{
    private readonly IApplicationDbContext _context;

    public GetFeedHandler(IApplicationDbContext context) => _context = context;

    public async Task<GetFeedResponse> Handle(GetFeedQuery request, CancellationToken ct)
    {
        var query = _context.Posts
            .Include(p => p.Author)
            .Where(p => p.Status == PostStatus.Active && !p.IsHidden);

        if (!string.IsNullOrEmpty(request.LocationZone))
            query = query.Where(p => p.LocationZone == request.LocationZone);

        if (!string.IsNullOrEmpty(request.CategoryFilter)
            && Enum.TryParse<PostCategory>(request.CategoryFilter, true, out var cat))
            query = query.Where(p => p.Category == cat);

        var totalCount = await query.CountAsync(ct);

        // Lost objave uvek na vrhu
        var posts = await query
            .OrderByDescending(p => p.Category == PostCategory.Lost)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PostSummaryDto(
                p.Id,
                p.Category.ToString(),
                p.Title,
                p.Description.Length > 200 ? p.Description.Substring(0, 200) + "..." : p.Description,
                _context.Media
                    .Where(m => m.EntityType == MediaEntityType.Post && m.EntityId == p.Id)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.Url)
                    .FirstOrDefault(),
                p.LocationZone,
                p.Author.DisplayName,
                TrustScoreCategory.FromScore(p.Author.TrustScore),
                p.Category == PostCategory.Lost,
                p.Status.ToString(),
                p.CreatedAt,
                p.ExpiresAt,
                p.Sightings.Count
            ))
            .ToListAsync(ct);

        return new GetFeedResponse(posts, totalCount, totalCount > request.Page * request.PageSize);
    }
}
