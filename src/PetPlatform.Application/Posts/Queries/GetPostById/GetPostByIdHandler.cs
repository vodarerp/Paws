using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Posts.DTOs;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.ValueObjects;

namespace PetPlatform.Application.Posts.Queries.GetPostById;

public class GetPostByIdHandler : IRequestHandler<GetPostByIdQuery, PostDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetPostByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<PostDetailDto> Handle(GetPostByIdQuery request, CancellationToken ct)
    {
        var post = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Pet)
            .Include(p => p.Sightings)
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
            ?? throw new KeyNotFoundException("Objava nije pronađena.");

        var mediaUrls = await _context.Media
            .Where(m => m.EntityType == MediaEntityType.Post && m.EntityId == post.Id)
            .OrderBy(m => m.SortOrder)
            .Select(m => m.Url)
            .ToListAsync(ct);

        PetSummaryDto? petDto = null;
        if (post.Pet is not null)
        {
            var petPhoto = await _context.Media
                .Where(m => m.EntityType == MediaEntityType.Pet && m.EntityId == post.Pet.Id)
                .OrderBy(m => m.SortOrder)
                .Select(m => m.Url)
                .FirstOrDefaultAsync(ct);

            petDto = new PetSummaryDto(
                post.Pet.Id, post.Pet.Name, post.Pet.Breed, post.Pet.Age,
                post.Pet.Gender.ToString(), post.Pet.Size.ToString(),
                post.Pet.Color, petPhoto);
        }

        return new PostDetailDto(
            post.Id,
            post.Category.ToString(),
            post.Title,
            post.Description,
            mediaUrls,
            post.LocationZone,
            post.Latitude,
            post.Longitude,
            post.LastSeenAt,
            post.ContactPreference?.ToString(),
            post.Status.ToString(),
            post.ResolutionType?.ToString(),
            post.CreatedAt,
            post.ExpiresAt,
            new AuthorDto(
                post.Author.Id,
                post.Author.DisplayName,
                post.Author.AvatarUrl,
                TrustScoreCategory.FromScore(post.Author.TrustScore),
                post.Author.IsVerified),
            petDto,
            post.Sightings.Count,
            post.ReportCount);
    }
}
