using FluentValidation;

namespace PetPlatform.Application.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email je obavezan.")
            .EmailAddress().WithMessage("Email format nije validan.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Lozinka je obavezna.")
            .MinimumLength(8).WithMessage("Lozinka mora imati najmanje 8 karaktera.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Ime je obavezno.")
            .MaximumLength(100);

        RuleFor(x => x.LocationZone)
            .NotEmpty().WithMessage("Lokacija je obavezna.");
    }
}
