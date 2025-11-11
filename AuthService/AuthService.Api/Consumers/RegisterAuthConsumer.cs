using MassTransit;
using Contracts.Auth;
using AuthService.Application.Commands;

namespace AuthService.Api.Consumers;

public class RegisterAuthConsumer : IConsumer<RegisterAuthRequest>
{
    private readonly RegisterAuthHandler _handler;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterAuthConsumer(RegisterAuthHandler handler, IPublishEndpoint publishEndpoint)
    {
        _handler = handler;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<RegisterAuthRequest> context)
    {
        try
        {
            var result = await _handler.Handle(context.Message);

            // Publish event để trigger saga TRƯỚC khi respond
            await _publishEndpoint.Publish(context.Message);
            Console.WriteLine($"✓ RegisterAuthRequest published for {context.Message.Email}");

            // Respond to gateway
            await context.RespondAsync(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error in RegisterAuthConsumer: {ex.Message}");
            throw;
        }
    }
}
