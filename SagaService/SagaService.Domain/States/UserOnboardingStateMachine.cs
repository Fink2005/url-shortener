using MassTransit;
using Contracts.Auth;
using Contracts.Users;

namespace SagaService.Domain.States;

public class UserOnboardingStateMachine : MassTransitStateMachine<UserOnboardingState>
{
    public State AwaitingUserCreation { get; private set; } = null!;
    public State Completed { get; private set; } = null!;

    public Event<RegisterAuthRequest> AuthRegistered { get; private set; } = null!;
    public Event<CreateUserResponse> UserCreated { get; private set; } = null!;

    public UserOnboardingStateMachine()
    {
        // Track instance by state property
        InstanceState(x => x.CurrentState);

        // === Event correlation setup ===
        Event(() => AuthRegistered, x =>
        {
            x.CorrelateBy((state, ctx) => state.Email == ctx.Message.Email);
            x.SelectId(ctx => Guid.NewGuid()); // Create new saga instance
        });

        Event(() => UserCreated, x =>
        {
            x.CorrelateById(ctx => ctx.Message.Id);
        });

        // === Flow logic ===
        Initially(
            When(AuthRegistered)
                .Then(ctx =>
                {
                    ctx.Saga.AuthId = Guid.NewGuid(); // or from ctx.Message if available
                    ctx.Saga.Username = ctx.Message.Username;
                    ctx.Saga.Email = ctx.Message.Email;
                    ctx.Saga.CreatedAt = DateTime.UtcNow;

                    Console.WriteLine($"[Saga] Registered Auth for {ctx.Saga.Email}");

                    // Gửi message sang UserService để tạo profile user
                    ctx.Publish(new CreateUserRequest(
                        ctx.Saga.AuthId.Value,
                        ctx.Saga.Username,
                        ctx.Saga.Email
                    ));
                })
                .TransitionTo(AwaitingUserCreation)
        );

        During(AwaitingUserCreation,
            When(UserCreated)
                .Then(ctx =>
                {
                    ctx.Saga.CompletedAt = DateTime.UtcNow;
                    Console.WriteLine($"[Saga] User Created for {ctx.Saga.Email}");
                })
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }
}
