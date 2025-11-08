using FluentValidation;
using Contracts.Users; // nếu bạn tổ chức message trong Contracts/Users/
namespace Validators.Users;

public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
{
    public DeleteUserRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("UserId is required");
    }
}
