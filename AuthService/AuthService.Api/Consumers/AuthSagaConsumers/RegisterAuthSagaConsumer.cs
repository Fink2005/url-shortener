using MassTransit;
using Contracts.Saga.Auth;
using AuthService.Application.Commands;
using Contracts.Auth;

namespace AuthService.Api.Saga.Consumers;

public class RegisterAuthSagaConsumer : IConsumer<RegisterRequestedEvent>
{
    private readonly RegisterAuthHandler _handler;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterAuthSagaConsumer(RegisterAuthHandler handler, IPublishEndpoint publishEndpoint)
    {
        _handler = handler;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<RegisterRequestedEvent> context)
    {
        try
        {
            Console.WriteLine($"📬 [AuthService] Received RegisterRequestedEvent for {context.Message.Email}");

            // Convert Saga event to handler request
            var request = new RegisterAuthRequest(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            );

            Console.WriteLine($"🔍 [AuthService] Calling handler to create user...");
            var result = await _handler.Handle(request);
            Console.WriteLine($"🔍 [AuthService] Handler returned Success = {result.Success}");

            if (result.Success)
            {
                Console.WriteLine($"✅ [AuthService] User created successfully: {context.Message.Email}");
                Console.WriteLine($"📤 [AuthService] Publishing RegisterAuthRequest to start Saga...");

                // Publish RegisterAuthRequest to start UserOnboardingStateMachine
                await _publishEndpoint.Publish(new RegisterAuthRequest(
                    context.Message.Username,
                    context.Message.Email,
                    context.Message.Password
                ));

                Console.WriteLine($"📨 [AuthService] Successfully published RegisterAuthRequest to start Saga");
            }
            else
            {
                Console.WriteLine($"⚠️ [AuthService] User creation failed but no exception thrown. NOT publishing event.");
            }

            // Respond back to Gateway
            await context.RespondAsync(new RegisterRequestedEvent(
                context.Message.Username,
                context.Message.Email,
                context.Message.Password
            ));
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi business logic (username/email đã tồn tại) - KHÔNG gửi mail
            Console.WriteLine($"❌ [AuthService] InvalidOperationException: {ex.Message}");
            Console.WriteLine($"🚫 [AuthService] NOT publishing RegisterRequestedEvent - NO EMAIL will be sent!");

            // QUAN TRỌNG: Throw lại exception để Gateway nhận được lỗi
            // Nhưng KHÔNG publish event nên SagaService sẽ KHÔNG gửi mail
            throw;
        }
        catch (Exception ex)
        {
            // Lỗi hệ thống khác
            Console.WriteLine($"❌ [AuthService] Unexpected error in RegisterAuthSagaConsumer: {ex.Message}");
            Console.WriteLine($"🚫 [AuthService] NOT publishing RegisterRequestedEvent due to error!");
            throw;
        }
    }
}
