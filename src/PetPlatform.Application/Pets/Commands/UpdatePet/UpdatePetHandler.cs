using MediatR;
using Microsoft.EntityFrameworkCore;
using PetPlatform.Application.Common.Interfaces;
using PetPlatform.Domain.Enums;
using PetPlatform.Domain.Exceptions;

namespace PetPlatform.Application.Pets.Commands.UpdatePet;

public class UpdatePetHandler : IRequestHandler<UpdatePetCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePetHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(UpdatePetCommand request, CancellationToken ct)
    {
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == request.PetId, ct)
            ?? throw new KeyNotFoundException("Ljubimac nije pronadjen.");

        if (pet.OwnerId != request.OwnerId)
            throw new DomainException("Nemate pristup ovom ljubimcu.", "UNAUTHORIZED_PET_ACTION");

        var gender = Enum.Parse<PetGender>(request.Gender, true);
        var size = Enum.Parse<PetSize>(request.Size, true);

        pet.Update(request.Name, request.Breed, gender, size,
            request.Age, request.Color, request.SpecialMarks, request.ChipNumber, request.IsSterilized);

        await _context.SaveChangesAsync(ct);
    }
}
