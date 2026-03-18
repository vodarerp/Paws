using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Application.Pets.DTOs;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Application.Pets.Queries.GetPetById;

public class GetPetByIdHandler : IRequestHandler<GetPetByIdQuery, PetDto>
{
    private readonly IApplicationDbContext _context;

    public GetPetByIdHandler(IApplicationDbContext context) => _context = context;

    public async Task<PetDto> Handle(GetPetByIdQuery request, CancellationToken ct)
    {
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == request.PetId, ct)
            ?? throw new KeyNotFoundException("Ljubimac nije pronadjen.");

        var photoUrl = await _context.Media
            .Where(m => m.EntityType == MediaEntityType.Pet && m.EntityId == pet.Id)
            .OrderBy(m => m.SortOrder)
            .Select(m => m.Url)
            .FirstOrDefaultAsync(ct);

        return new PetDto(
            pet.Id, pet.Name, pet.Breed, pet.Age,
            pet.Gender.ToString(), pet.Size.ToString(),
            pet.Color, pet.SpecialMarks, pet.ChipNumber, pet.IsSterilized,
            pet.Status.ToString(), photoUrl, pet.CreatedAt);
    }
}
