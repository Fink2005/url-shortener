using FluentValidation;
using Contracts.Users; // nếu bạn tổ chức message trong Contracts/Users/
namespace Validators.Users;

public class GetUserRequestValidator : AbstractValidator<GetUserRequest>
{
    public GetUserRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("UserId is required");
    }
}


