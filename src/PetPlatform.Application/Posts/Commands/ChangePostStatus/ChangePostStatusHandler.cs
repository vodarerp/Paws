using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Application.Posts.Commands.ChangePostStatus;

public class ChangePostStatusHandler : IRequestHandler<ChangePostStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public ChangePostStatusHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(ChangePostStatusCommand request, CancellationToken ct)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
            ?? throw new KeyNotFoundException("Objava nije pronađena.");

        if (post.AuthorId != request.UserId)
            throw new UnauthorizedPostAccessException();

        switch (request.Action.ToLower())
        {
            case "resolve":
                var resolution = Enum.Parse<ResolutionType>(request.ResolutionType!, true);
                post.Resolve(resolution);

                if (post.Category == PostCategory.Lost)
                {
                    var author = await _context.Users.FindAsync(new object[] { post.AuthorId }, ct);
                    author?.ApplyTrustScoreChange(TrustScoreActionType.LostAlertResolved);
                }
                break;

            case "extend":
                post.Extend();
                break;

            case "remove":
                post.Remove();
                break;

            default:
                throw new DomainException("Nepoznata akcija.", "INVALID_ACTION");
        }

        await _context.SaveChangesAsync(ct);
    }
}
