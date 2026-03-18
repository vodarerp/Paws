using MediatR;

namespace PetPlatform.Application.Pets.Commands.UpdatePet;

public record UpdatePetCommand(
    Guid PetId,
    Guid OwnerId,
    string Name,
    string Breed,
    string Gender,
    string Size,
    string? Age,
    string? Color,
    string? SpecialMarks,
    string? ChipNumber,
    bool? IsSterilized
) : IRequest;
