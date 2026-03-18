using FluentValidation;

namespace PetPlatform.Application.Posts.Commands.UpdatePost;

public class UpdatePostValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naslov je obavezan.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Opis je obavezan.")
            .MaximumLength(2000);

        RuleFor(x => x.LocationZone)
            .NotEmpty().WithMessage("Lokacija je obavezna.");
    }
}
