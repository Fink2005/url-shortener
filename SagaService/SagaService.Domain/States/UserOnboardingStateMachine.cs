using MassTransit;
using Contracts.Auth;
using Contracts.Users;
using Contracts.Saga;

namespace SagaService.Domain.States;

public class UserOnboardingStateMachine : MassTransitStateMachine<UserOnboardingState>
{
    public State AwaitingAuthCreation { get; private set; } = null!;
    public State AwaitingEmailConfirmation { get; private set; } = null!;
    public State AwaitingRoleAssignment { get; private set; } = null!;
    public State AwaitingUserCreation { get; private set; } = null!;
    public State Completed { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    // Events
    public Event<RegisterAuthRequest> OnboardingStarted { get; private set; } = null!;
    public Event<AuthUserCreated> AuthCreated { get; private set; } = null!;
    public Event<AuthUserCreateFailed> AuthCreateFailed { get; private set; } = null!;
    public Event<EmailConfirmationSent> EmailSent { get; private set; } = null!;
    public Event<DefaultRoleAssigned> RoleAssigned { get; private set; } = null!;
    public Event<UserProfileCreated> UserCreated { get; private set; } = null!;
    public Event<UserProfileCreateFailed> UserCreateFailed { get; private set; } = null!;

    public UserOnboardingStateMachine()
    {
        // Track instance by state property
        InstanceState(x => x.CurrentState);

        // === Event correlation setup ===
        Event(() => OnboardingStarted, x =>
        {
            x.CorrelateBy((state, ctx) => state.Email == ctx.Message.Email);
            x.SelectId(ctx => Guid.NewGuid()); // Generate new CorrelationId for saga instance
        });

        Event(() => AuthCreated, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        Event(() => AuthCreateFailed, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        Event(() => EmailSent, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        Event(() => RoleAssigned, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        Event(() => UserCreated, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        Event(() => UserCreateFailed, x =>
        {
            x.CorrelateById(ctx => ctx.Message.CorrelationId);
        });

        // === Flow logic ===
        Initially(
            When(OnboardingStarted)
                .Then(ctx =>
                {
                    ctx.Saga.Username = ctx.Message.Username;
                    ctx.Saga.Email = ctx.Message.Email;
                    ctx.Saga.ConfirmationToken = GenerateConfirmationToken();
                    ctx.Saga.CreatedAt = DateTime.UtcNow;

                    Console.WriteLine($"[Saga] Starting onboarding for {ctx.Saga.Email}");

                    // Step 1: Send message to AuthService to create auth user
                    ctx.Publish(new CreateAuthUserCommand(
                        ctx.Saga.CorrelationId,
                        ctx.Saga.Username,
                        ctx.Saga.Email,
                        ctx.Message.Password
                    ));
                })
                .TransitionTo(AwaitingAuthCreation)
        );

        During(AwaitingAuthCreation,
            When(AuthCreated)
                .Then(ctx =>
                {
                    ctx.Saga.AuthId = ctx.Message.AuthUserId;
                    Console.WriteLine($"[Saga] Auth user created: {ctx.Saga.AuthId}");

                    // Step 2: Send confirmation email
                    ctx.Publish(new SendConfirmationEmailCommand(
                        ctx.Saga.CorrelationId,
                        ctx.Saga.Email,
                        ctx.Saga.ConfirmationToken
                    ));
                })
                .TransitionTo(AwaitingEmailConfirmation),
            When(AuthCreateFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"Auth creation failed: {ctx.Message.Reason}";
                    Console.WriteLine($"[Saga] {ctx.Saga.FailureReason}");
                })
                .TransitionTo(Failed)
        );

        During(AwaitingEmailConfirmation,
            When(EmailSent)
                .Then(ctx =>
                {
                    ctx.Saga.EmailConfirmed = true;
                    Console.WriteLine($"[Saga] Confirmation email sent to {ctx.Saga.Email}");

                    // Step 3: Assign default role
                    ctx.Publish(new AssignDefaultRoleCommand(
                        ctx.Saga.CorrelationId,
                        ctx.Saga.AuthId.Value
                    ));
                })
                .TransitionTo(AwaitingRoleAssignment)
        );

        During(AwaitingRoleAssignment,
            When(RoleAssigned)
                .Then(ctx =>
                {
                    ctx.Saga.AssignedRole = ctx.Message.Role;
                    Console.WriteLine($"[Saga] Role '{ctx.Message.Role}' assigned to {ctx.Saga.Email}");

                    // Step 4: Create user profile
                    ctx.Publish(new CreateUserProfileCommand(
                        ctx.Saga.CorrelationId,
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
                    ctx.Saga.UserId = ctx.Message.UserId;
                    ctx.Saga.CompletedAt = DateTime.UtcNow;
                    Console.WriteLine($"[Saga] User profile created: {ctx.Saga.UserId}");
                    Console.WriteLine($"[Saga] ✓ Onboarding completed for {ctx.Saga.Email} in {(ctx.Saga.CompletedAt - ctx.Saga.CreatedAt)?.TotalSeconds}s");
                })
                .Finalize(),
            When(UserCreateFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = $"User profile creation failed: {ctx.Message.Reason}";
                    Console.WriteLine($"[Saga] {ctx.Saga.FailureReason}");
                })
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }

    private static string GenerateConfirmationToken()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
    }
}
