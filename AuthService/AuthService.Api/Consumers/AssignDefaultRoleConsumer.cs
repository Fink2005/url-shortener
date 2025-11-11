using MassTransit;
using Contracts.Saga;

namespace AuthService.Api.Consumers;

public class AssignDefaultRoleConsumer : IConsumer<AssignDefaultRoleCommand>
{
    public async Task Consume(ConsumeContext<AssignDefaultRoleCommand> context)
    {
        try
        {
            // TODO: Implement role assignment logic in AuthService
            // For now, assign default "User" role
            var defaultRole = "User";

            await context.Publish(new DefaultRoleAssigned(
                context.Message.CorrelationId,
                context.Message.AuthUserId,
                defaultRole
            ));

            Console.WriteLine($"✓ Default role '{defaultRole}' assigned to user {context.Message.AuthUserId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Failed to assign role: {ex.Message}");
            throw;
        }
    }
}
