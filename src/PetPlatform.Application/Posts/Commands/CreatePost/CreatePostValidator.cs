using FluentValidation;
using PetPlatform.Domain.Enums;

namespace PetPlatform.Application.Posts.Commands.CreatePost;

public class CreatePostValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Naslov je obavezan.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Opis je obavezan.")
            .MaximumLength(2000);

        RuleFor(x => x.LocationZone)
            .NotEmpty().WithMessage("Lokacija je obavezna.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => Enum.TryParse<PostCategory>(c, true, out _))
            .WithMessage("Nevažeća kategorija. Dozvoljene: Adoption, Lost, Found.");

        RuleFor(x => x.MediaIds)
            .NotEmpty().WithMessage("Potrebna je najmanje jedna fotografija.");
    }
}
