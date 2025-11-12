using MassTransit;
using Contracts.Saga;
using Contracts.Auth;
using AuthService.Application.Commands;
using AuthService.Domain.Repositories;

namespace AuthService.Api.Saga.Consumers;

/// <summary>
/// Consumer for CreateAuthUserCommand from Saga
/// This consumer ONLY handles user creation requests from the Saga orchestration
/// </summary>
public class CreateAuthUserSagaConsumer : IConsumer<CreateAuthUserCommand>
{
    private readonly RegisterAuthHandler _handler;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IAuthUserRepository _repository;

    public CreateAuthUserSagaConsumer(
        RegisterAuthHandler handler, 
        IPublishEndpoint publishEndpoint,
        IAuthUserRepository repository)
    {
        _handler = handler;
        _publishEndpoint = publishEndpoint;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<CreateAuthUserCommand> context)
    {
        try
        {
            Console.WriteLine($"🔨 [AuthService] Received CreateAuthUserCommand from Saga for {context.Message.Email}");

            // Convert Saga command to handler request
            var request = new RegisterAuthRequest(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            );

            Console.WriteLine($"🔍 [AuthService] Creating user via handler...");
            var result = await _handler.Handle(request);
            Console.WriteLine($"🔍 [AuthService] Handler returned Success = {result.Success}");

            if (result.Success)
            {
                Console.WriteLine($"✅ [AuthService] User created successfully: {context.Message.Email}");
                
                // Get the created user's ID from repository
                var createdUser = await _repository.FindByEmailAsync(context.Message.Email);
                if (createdUser == null)
                {
                    throw new InvalidOperationException("User was created but could not be found");
                }

                Console.WriteLine($"📤 [AuthService] Publishing AuthUserCreated event to Saga...");

                // Publish AuthUserCreated event back to Saga
                await _publishEndpoint.Publish(new AuthUserCreated(
                    context.Message.CorrelationId,
                    createdUser.Id,
                    context.Message.Email
                ));

                Console.WriteLine($"📨 [AuthService] Successfully published AuthUserCreated to Saga");
            }
            else
            {
                Console.WriteLine($"❌ [AuthService] User creation failed - publishing failure event");
                
                // Publish failure event to Saga
                await _publishEndpoint.Publish(new AuthUserCreateFailed(
                    context.Message.CorrelationId,
                    "User creation failed"
                ));
            }
        }
        catch (InvalidOperationException ex)
        {
            // Business logic error (username/email already exists)
            Console.WriteLine($"❌ [AuthService] User creation failed: {ex.Message}");
            
            await _publishEndpoint.Publish(new AuthUserCreateFailed(
                context.Message.CorrelationId,
                ex.Message
            ));
        }
        catch (Exception ex)
        {
            // System error
            Console.WriteLine($"❌ [AuthService] Unexpected error creating user: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            await _publishEndpoint.Publish(new AuthUserCreateFailed(
                context.Message.CorrelationId,
                $"System error: {ex.Message}"
            ));
        }
    }
}
