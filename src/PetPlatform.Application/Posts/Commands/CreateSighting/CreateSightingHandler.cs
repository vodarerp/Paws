using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Application.Posts.Commands.CreateSighting;

public class CreateSightingHandler : IRequestHandler<CreateSightingCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSightingHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreateSightingCommand request, CancellationToken ct)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
            ?? throw new KeyNotFoundException("Objava nije pronadjena.");

        if (post.Category != PostCategory.Lost)
            throw new DomainException("Vidjenje se moze prijaviti samo za izgubljene ljubimce.", "SIGHTING_LOST_ONLY");

        if (post.Status != PostStatus.Active)
            throw new DomainException("Objava nije aktivna.", "POST_NOT_ACTIVE");

        var sighting = PostSighting.Create(
            request.PostId, request.ReporterId,
            request.Latitude, request.Longitude, request.SeenAt,
            request.LocationDescription, request.Comment);

        _context.PostSightings.Add(sighting);
        await _context.SaveChangesAsync(ct);

        return sighting.Id;
    }
}
