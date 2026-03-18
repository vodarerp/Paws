using MediatR;

namespace PetPlatform.Application.Pets.Commands.CreatePet;

public record CreatePetCommand(
    Guid OwnerId,
    string Name,
    string Breed,
    string Gender,
    string Size,
    string? Age,
    string? Color,
    string? SpecialMarks,
    string? ChipNumber,
    bool? IsSterilized,
    List<Guid> MediaIds
) : IRequest<Guid>;
