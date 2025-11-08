using FluentValidation;
using Contracts.Auth;

namespace Validators.Auth;

public class LoginAuthValidator : AbstractValidator<LoginAuthRequest>
{
    public LoginAuthValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
