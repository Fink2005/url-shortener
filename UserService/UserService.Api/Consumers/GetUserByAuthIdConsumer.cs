using MassTransit;
using Contracts.Users;
using UserService.Domain.Repositories;

namespace UserService.Api.Consumers;

public class GetUserByAuthIdConsumer : IConsumer<GetUserByAuthIdRequest>
{
    private readonly IUserRepository _repository;

    public GetUserByAuthIdConsumer(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<GetUserByAuthIdRequest> context)
    {
        var user = await _repository.FindByAuthIdAsync(context.Message.AuthId);

        if (user == null)
        {
            throw new Exception($"User with AuthId {context.Message.AuthId} not found");
        }

        await context.RespondAsync(new GetUserByAuthIdResponse(
            user.Id,
            user.AuthId,
            user.Username,
            user.Email
        ));
    }
}
