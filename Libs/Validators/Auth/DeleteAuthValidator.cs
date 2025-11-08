using FluentValidation;
using Contracts.Auth;

namespace Validators.Auth;

public class DeleteAuthValidator : AbstractValidator<DeleteAuthRequest>
{
    public DeleteAuthValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
