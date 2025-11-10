using Contracts.Url;
using FluentValidation;

namespace Validators.Url;

public class CreateShortUrlValidator : AbstractValidator<CreateShortUrlRequest>
{
    public CreateShortUrlValidator()
    {
        RuleFor(x => x.OriginalUrl)
            .NotEmpty().WithMessage("Original URL is required.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Invalid URL format.");

    }
}
