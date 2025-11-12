using MassTransit;
using Contracts.Saga;
using Contracts.Saga.Auth;
using FluentValidation;
using Contracts.Auth;
using Validators.Auth;
using AuthService.Domain.Repositories;

namespace AuthService.Api.Saga.Consumers;

public class RegisterAuthSagaConsumer : IConsumer<RegisterRequestedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<RegisterAuthRequest> _validator;
    private readonly IAuthUserRepository _repository;

    public RegisterAuthSagaConsumer(
        IPublishEndpoint publishEndpoint,
        IValidator<RegisterAuthRequest> validator,
        IAuthUserRepository repository)
    {
        _publishEndpoint = publishEndpoint;
        _validator = validator;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<RegisterRequestedEvent> context)
    {
        try
        {
            Console.WriteLine($"📬 [AuthService] Received RegisterRequestedEvent for {context.Message.Email}");

            // Validate input
            var request = new RegisterAuthRequest(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            );

            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                Console.WriteLine($"❌ [AuthService] Validation failed: {errors}");
                throw new ValidationException($"Validation failed: {errors}");
            }

            // Check if username already exists
            var existingUser = await _repository.FindByUsernameAsync(context.Message.Username);
            if (existingUser != null)
            {
                Console.WriteLine($"❌ [AuthService] Username already exists: {context.Message.Username}");
                throw new InvalidOperationException("Username already exists");
            }

            // Check if email already exists
            var existingEmail = await _repository.FindByEmailAsync(context.Message.Email);
            if (existingEmail != null)
            {
                Console.WriteLine($"❌ [AuthService] Email already exists: {context.Message.Email}");
                throw new InvalidOperationException("Email already exists");
            }

            Console.WriteLine($"✅ [AuthService] Validation passed for {context.Message.Email}");
            Console.WriteLine($"📤 [AuthService] Publishing StartUserOnboarding to start Saga...");

            // Publish StartUserOnboarding to start UserOnboardingStateMachine
            // Saga will handle: create user → send email → create profile
            await _publishEndpoint.Publish(new StartUserOnboarding(
                Guid.NewGuid(),  // Generate new CorrelationId for Saga
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            ));

            Console.WriteLine($"📨 [AuthService] Successfully published StartUserOnboarding to start Saga");

            // Respond back to Gateway
            await context.RespondAsync(new RegisterRequestedEvent(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            ));
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"❌ [AuthService] Validation error: {ex.Message}");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi business logic (username/email đã tồn tại) - KHÔNG gửi mail
            Console.WriteLine($"❌ [AuthService] Business logic error: {ex.Message}");
            Console.WriteLine($"🚫 [AuthService] NOT publishing RegisterAuthRequest - NO EMAIL will be sent!");
            throw;
        }
        catch (Exception ex)
        {
            // Lỗi hệ thống khác
            Console.WriteLine($"❌ [AuthService] Unexpected error in RegisterAuthSagaConsumer: {ex.Message}");
            Console.WriteLine($"🚫 [AuthService] NOT publishing RegisterAuthRequest due to error!");
            throw;
        }
    }
}
