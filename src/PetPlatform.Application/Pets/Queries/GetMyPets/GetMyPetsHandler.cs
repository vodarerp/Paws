using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Pets.DTOs;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Application.Pets.Queries.GetMyPets;

public class GetMyPetsHandler : IRequestHandler<GetMyPetsQuery, List<PetDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMyPetsHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<PetDto>> Handle(GetMyPetsQuery request, CancellationToken ct)
    {
        return await _context.Pets
            .Where(p => p.OwnerId == request.OwnerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PetDto(
                p.Id, p.Name, p.Breed, p.Age,
                p.Gender.ToString(), p.Size.ToString(),
                p.Color, p.SpecialMarks, p.ChipNumber, p.IsSterilized,
                p.Status.ToString(),
                _context.Media
                    .Where(m => m.EntityType == MediaEntityType.Pet && m.EntityId == p.Id)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => m.Url)
                    .FirstOrDefault(),
                p.CreatedAt))
            .ToListAsync(ct);
    }
}
