using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Posts.DTOs;

namespace PetPlatform.Application.Posts.Queries.GetPostSightings;

public class GetPostSightingsHandler : IRequestHandler<GetPostSightingsQuery, List<SightingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPostSightingsHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<SightingDto>> Handle(GetPostSightingsQuery request, CancellationToken ct)
    {
        return await _context.PostSightings
            .Include(s => s.Reporter)
            .Where(s => s.PostId == request.PostId)
            .OrderByDescending(s => s.SeenAt)
            .Select(s => new SightingDto(
                s.Id,
                s.Reporter.DisplayName,
                s.Latitude,
                s.Longitude,
                s.LocationDescription,
                s.SeenAt,
                s.Comment,
                s.CreatedAt))
            .ToListAsync(ct);
    }
}
