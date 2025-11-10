using Contracts.Url;
using FluentValidation;

namespace Validators.Url;


public class DeleteShortUrlValidator : AbstractValidator<DeleteShortUrlRequest>
{
    public DeleteShortUrlValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Short URL ID is required.");

    }
}