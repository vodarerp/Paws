using FluentValidation;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Application.Pets.Commands.CreatePet;

public class CreatePetValidator : AbstractValidator<CreatePetCommand>
{
    public CreatePetValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ime ljubimca je obavezno.")
            .MaximumLength(100);

        RuleFor(x => x.Breed)
            .NotEmpty().WithMessage("Rasa je obavezna.")
            .MaximumLength(100);

        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => Enum.TryParse<PetGender>(g, true, out _))
            .WithMessage("Nevalidan pol. Dozvoljeno: Male, Female, Unknown.");

        RuleFor(x => x.Size)
            .NotEmpty()
            .Must(s => Enum.TryParse<PetSize>(s, true, out _))
            .WithMessage("Nevalidna velicina. Dozvoljeno: Small, Medium, Large.");
    }
}
