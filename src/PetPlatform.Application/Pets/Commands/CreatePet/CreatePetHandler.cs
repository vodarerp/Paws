using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Entities;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Application.Pets.Commands.CreatePet;

public class CreatePetHandler : IRequestHandler<CreatePetCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePetHandler(IApplicationDbContext context) => _context = context;

    public async Task<Guid> Handle(CreatePetCommand request, CancellationToken ct)
    {
        var gender = Enum.Parse<PetGender>(request.Gender, true);
        var size = Enum.Parse<PetSize>(request.Size, true);

        var pet = Pet.Create(request.OwnerId, request.Name, request.Breed, gender, size, request.Age, request.Color);

        if (!string.IsNullOrEmpty(request.SpecialMarks) || !string.IsNullOrEmpty(request.ChipNumber) || request.IsSterilized.HasValue)
        {
            pet.Update(request.Name, request.Breed, gender, size,
                request.Age, request.Color, request.SpecialMarks, request.ChipNumber, request.IsSterilized);
        }

        _context.Pets.Add(pet);

        if (request.MediaIds.Any())
        {
            var mediaItems = await _context.Media
                .Where(m => request.MediaIds.Contains(m.Id))
                .ToListAsync(ct);

            foreach (var media in mediaItems)
                media.AssociateWith(MediaEntityType.Pet, pet.Id);
        }

        await _context.SaveChangesAsync(ct);
        return pet.Id;
    }
}
