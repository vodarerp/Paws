using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Application.Posts.Commands.UpdatePost;

public class UpdatePostHandler : IRequestHandler<UpdatePostCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePostHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdatePostCommand request, CancellationToken ct)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
            ?? throw new KeyNotFoundException("Objava nije pronadjena.");

        if (post.AuthorId != request.UserId)
            throw new UnauthorizedPostAccessException();

        if (post.Status != PostStatus.Active)
            throw new DomainException("Samo aktivne objave se mogu menjati.", "POST_NOT_EDITABLE");

        ContactPreference? contactPref = null;
        if (!string.IsNullOrEmpty(request.ContactPreference))
            contactPref = Enum.Parse<ContactPreference>(request.ContactPreference, true);

        post.Update(request.Title, request.Description, request.LocationZone,
            request.Latitude, request.Longitude, contactPref);

        await _context.SaveChangesAsync(ct);
    }
}
