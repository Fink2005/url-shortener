using MassTransit;
using Contracts.Saga;
using UserService.Application.Commands;

namespace UserService.Api.Consumers;

public class CreateUserFromSagaConsumer : IConsumer<CreateUserProfileCommand>
{
    private readonly CreateUserHandler _handler;

    public CreateUserFromSagaConsumer(CreateUserHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<CreateUserProfileCommand> context)
    {
        try
        {
            var request = new Contracts.Users.CreateUserRequest(
                context.Message.AuthUserId,
                context.Message.Username,
                context.Message.Email
            );

            var result = await _handler.Handle(request);

            if (result != null && result.Id != Guid.Empty)
            {
                // Publish event that user profile was created
                await context.Publish(new UserProfileCreated(
                    context.Message.CorrelationId,
                    result.Id,
                    context.Message.AuthUserId
                ));

                Console.WriteLine($"✓ User profile created: {result.Id} for {context.Message.Email}");
            }
            else
            {
                throw new Exception("User creation returned null or invalid result");
            }
        }
        catch (Exception ex)
        {
            await context.Publish(new UserProfileCreateFailed(
                context.Message.CorrelationId,
                ex.Message
            ));

            Console.WriteLine($"✗ Failed to create user profile for {context.Message.Email}: {ex.Message}");
            throw;
        }
    }
}
