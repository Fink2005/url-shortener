using MassTransit;
using AuthService.Application.Commands;
using Contracts.Auth;

namespace AuthService.Api.Consumers;

public class GetAllAuthUsersConsumer : IConsumer<Contracts.Auth.GetAllAuthUsersRequest>
{
    private readonly GetAllAuthUsersHandler _handler;

    public GetAllAuthUsersConsumer(GetAllAuthUsersHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<Contracts.Auth.GetAllAuthUsersRequest> context)
    {
        try
        {
            Console.WriteLine($"📋 [AuthService] Received GetAllAuthUsersRequest");

            var handlerResponse = await _handler.Handle();

            // Convert to contract DTO
            var users = handlerResponse.Users.Select(u => new Contracts.Auth.AuthUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsEmailVerified = u.IsEmailVerified
            }).ToList();

            var response = new Contracts.Auth.GetAllAuthUsersResponse(users);

            Console.WriteLine($"✅ [AuthService] Retrieved {users.Count} users");

            await context.RespondAsync(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error in GetAllAuthUsersConsumer: {ex.Message}");
            throw;
        }
    }
}
