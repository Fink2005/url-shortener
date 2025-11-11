using MassTransit;
using Contracts.Auth;
using AuthService.Domain.Repositories;

namespace AuthService.Api.Consumers;

public class GetAuthsByIdsConsumer : IConsumer<GetAuthsByIdsRequest>
{
    private readonly IAuthUserRepository _repository;

    public GetAuthsByIdsConsumer(IAuthUserRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetAuthsByIdsRequest> context)
    {
        try
        {
            Console.WriteLine($"📥 [AuthService] Batch GetAuthsByIds for {context.Message.Ids.Count} auth IDs");
            
            var authUsers = await _repository.GetByIdsAsync(context.Message.Ids);
            
            var dtos = authUsers.Select(auth => new AuthByIdDto
            {
                Id = auth.Id,
                Role = auth.Role.ToString(),
                IsEmailVerified = auth.IsEmailVerified
            }).ToList();

            Console.WriteLine($"✅ [AuthService] Found {dtos.Count} auth records");
            
            await context.RespondAsync(new GetAuthsByIdsResponse(dtos));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [AuthService] Error in GetAuthsByIds: {ex.Message}");
            throw;
        }
    }
}
