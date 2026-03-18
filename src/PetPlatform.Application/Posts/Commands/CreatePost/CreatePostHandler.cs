using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Constants;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Application.Posts.Commands.CreatePost;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, CreatePostResponse>
{
    private readonly IApplicationDbContext _context;

    public CreatePostHandler(IApplicationDbContext context) => _context = context;

    public async Task<CreatePostResponse> Handle(CreatePostCommand request, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var dailyCount = await _context.Posts
            .CountAsync(p => p.AuthorId == request.AuthorId && p.CreatedAt >= today, ct);

        if (dailyCount >= RateLimits.MaxPostsPerDay)
            throw new PostLimitExceededException();

        var category = Enum.Parse<PostCategory>(request.Category, true);

        if (category == PostCategory.Lost)
        {
            var hasActiveLost = await _context.Posts
                .AnyAsync(p => p.AuthorId == request.AuthorId
                    && p.Category == PostCategory.Lost
                    && p.Status == PostStatus.Active, ct);

            if (hasActiveLost)
                throw new AlertCooldownException();
        }

        ContactPreference? contactPref = null;
        if (!string.IsNullOrEmpty(request.ContactPreference))
            contactPref = Enum.Parse<ContactPreference>(request.ContactPreference, true);

        var post = Post.Create(
            request.AuthorId, category, request.Title, request.Description,
            request.LocationZone, request.PetId, request.Latitude, request.Longitude, contactPref);

        _context.Posts.Add(post);

        // Poveži Media sa Post-om
        if (request.MediaIds.Any())
        {
            var mediaItems = await _context.Media
                .Where(m => request.MediaIds.Contains(m.Id))
                .ToListAsync(ct);

            for (var i = 0; i < mediaItems.Count; i++)
                mediaItems[i].AssociateWith(MediaEntityType.Post, post.Id);
        }

        // Trust Score: +2 za kreiranje objave
        var author = await _context.Users.FindAsync(new object[] { request.AuthorId }, ct);
        if (author is not null)
        {
            author.ApplyTrustScoreChange(TrustScoreActionType.PostCreated);

            var scoreEntry = TrustScoreEntry.Create(
                request.AuthorId,
                TrustScoreActionType.PostCreated,
                TrustScorePoints.GetPoints(TrustScoreActionType.PostCreated),
                "Objavljen oglas",
                post.Id);
            _context.TrustScoreHistory.Add(scoreEntry);
        }

        await _context.SaveChangesAsync(ct);

        return new CreatePostResponse(
            post.Id,
            post.Status.ToString(),
            category == PostCategory.Lost,
            post.ExpiresAt);
    }
}
