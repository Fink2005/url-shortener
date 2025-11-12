using AuthService.Domain.Repositories;
using Contracts.Mail;

namespace AuthService.Application.Commands;

public class VerifyEmailAuthHandler
{
    private readonly IAuthUserRepository _authUserRepository;

    public VerifyEmailAuthHandler(IAuthUserRepository authUserRepository)
    {
        _authUserRepository = authUserRepository;
    }

    public async Task<VerifyEmailAuthResponse> Handle(VerifyEmailAuthRequest request)
    {
        // NOTE: Token validation is handled by MailService (via Redis)
        // This handler is called ONLY AFTER MailService has validated the token
        // and published EmailVerifiedEvent

        // Find user by email
        var user = await _authUserRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email is already verified");
        }

        // Mark email as verified
        user.VerifyEmail();

        // Save changes
        await _authUserRepository.UpdateAsync(user);

        return new VerifyEmailAuthResponse(true, "Email verified successfully");
    }
}

public record VerifyEmailAuthRequest(string Email, string Token);
public record VerifyEmailAuthResponse(bool Success, string Message);
