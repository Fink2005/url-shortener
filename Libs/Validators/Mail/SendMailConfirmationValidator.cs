using Contracts.Url;
using FluentValidation;
using Contracts.Mail;

namespace Validators.Mail;

public class SendMailConfirmationValidator : AbstractValidator<SendConfirmationEmailRequest>
{
    public SendMailConfirmationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
