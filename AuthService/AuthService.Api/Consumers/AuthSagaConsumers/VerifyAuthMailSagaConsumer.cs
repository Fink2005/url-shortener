using MassTransit;
using Contracts.Saga;
using AuthService.Application.Commands;
using AuthService.Domain.Repositories;

namespace AuthService.Api.Saga.Consumers;

/// <summary>
/// Consumer lắng nghe EmailVerifiedEvent từ MailService
/// để cập nhật IsEmailVerified = true trong AuthService
/// và tạo User Profile trong UserService
/// </summary>
public class VerifyAuthMailSagaConsumer : IConsumer<EmailVerifiedEvent>
{
    private readonly VerifyEmailAuthHandler _authHandler;
    private readonly IAuthUserRepository _authUserRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public VerifyAuthMailSagaConsumer(
        VerifyEmailAuthHandler authHandler,
        IAuthUserRepository authUserRepository,
        IPublishEndpoint publishEndpoint)
    {
        _authHandler = authHandler;
        _authUserRepository = authUserRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<EmailVerifiedEvent> context)
    {
        try
        {
            Console.WriteLine($"📬 [AuthService] Received EmailVerifiedEvent for {context.Message.Email}");

            // Update user's IsEmailVerified in AuthService
            Console.WriteLine($"🔍 [AuthService] Updating IsEmailVerified for {context.Message.Email}...");
            var authRequest = new VerifyEmailAuthRequest(context.Message.Email, string.Empty);
            var authResponse = await _authHandler.Handle(authRequest);

            if (authResponse.Success)
            {
                Console.WriteLine($"✅ [AuthService] IsEmailVerified updated successfully for {context.Message.Email}");

                // Get user details to create profile in UserService
                var authUser = await _authUserRepository.GetByEmailAsync(context.Message.Email);

                if (authUser != null)
                {
                    Console.WriteLine($"👤 [AuthService] Publishing CreateUserProfileCommand for {authUser.Username}");

                    // Publish command to create user profile in UserService
                    await _publishEndpoint.Publish(new CreateUserProfileCommand(
                        context.Message.CorrelationId,
                        authUser.Id,
                        authUser.Username,
                        authUser.Email
                    ));

                    Console.WriteLine($"✅ [AuthService] CreateUserProfileCommand published successfully");
                }
                else
                {
                    Console.WriteLine($"⚠️ [AuthService] AuthUser not found for email: {context.Message.Email}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ [AuthService] Failed to update IsEmailVerified: {authResponse.Message}");
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"❌ [AuthService] InvalidOperationException: {ex.Message}");
            // Don't throw - just log, vì MailService đã verify thành công rồi
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Unexpected error in VerifyAuthMailSagaConsumer: {ex.Message}");
            // Don't throw - just log
        }
    }
}
