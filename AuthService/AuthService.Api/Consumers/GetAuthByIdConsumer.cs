using MassTransit;
using Contracts.Auth;
using AuthService.Domain.Repositories;

namespace AuthService.Api.Consumers;

public class GetAuthByIdConsumer : IConsumer<GetAuthByIdRequest>
{
    private readonly IAuthUserRepository _repository;

    public GetAuthByIdConsumer(IAuthUserRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetAuthByIdRequest> context)
    {
        try
        {
            Console.WriteLine($"📥 [AuthService] GetAuthById request for AuthId: {context.Message.Id}");

            var authUser = await _repository.FindByIdAsync(context.Message.Id);

            if (authUser == null)
            {
                Console.WriteLine($"❌ [AuthService] Auth user not found: {context.Message.Id}");
                throw new InvalidOperationException($"Auth user not found with ID: {context.Message.Id}");
            }

            var response = new GetAuthByIdResponse(
                authUser.Id,
                authUser.Username,
                authUser.Email,
                authUser.Role.ToString(),
                authUser.IsEmailVerified
            );

            Console.WriteLine($"✅ [AuthService] Auth info retrieved for: {authUser.Username}");
            await context.RespondAsync(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error getting auth by ID: {ex.Message}");
            throw;
        }
    }
}
