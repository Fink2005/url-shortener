using MassTransit;
using Contracts.Saga;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class CreateAuthUserConsumer : IConsumer<CreateAuthUserCommand>
{
    private readonly RegisterAuthHandler _handler;

    public CreateAuthUserConsumer(RegisterAuthHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<CreateAuthUserCommand> context)
    {
        try
        {
            var request = new Contracts.Auth.RegisterAuthRequest(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            );

            var result = await _handler.Handle(request);

            if (result.Success)
            {
                // Publish event that auth user was created
                await context.Publish(new AuthUserCreated(
                    context.Message.CorrelationId,
                    Guid.NewGuid(), // Should get from handler or DB
                    context.Message.Email
                ));

                Console.WriteLine($"✓ Auth user created for {context.Message.Email}");
            }
            else
            {
                // Publish failure event
                await context.Publish(new AuthUserCreateFailed(
                    context.Message.CorrelationId,
                    "Registration failed"
                ));

                Console.WriteLine($"✗ Failed to create auth user for {context.Message.Email}");
            }
        }
        catch (Exception ex)
        {
            await context.Publish(new AuthUserCreateFailed(
                context.Message.CorrelationId,
                ex.Message
            ));

            Console.WriteLine($"✗ Exception creating auth user: {ex.Message}");
            throw;
        }
    }
}
