using FluentValidation;

namespace PetPlatform.Application.Posts.Commands.CreateSighting;

public class CreateSightingValidator : AbstractValidator<CreateSightingCommand>
{
    public CreateSightingValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Nevalidna geografska sirina.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Nevalidna geografska duzina.");

        RuleFor(x => x.SeenAt)
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5)).WithMessage("Vreme vidjenja ne moze biti u buducnosti.");
    }
}
