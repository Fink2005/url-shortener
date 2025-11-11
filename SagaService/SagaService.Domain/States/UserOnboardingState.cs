using MassTransit;

namespace SagaService.Domain.States;

public class UserOnboardingState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }       // ID để track saga instance
    public string CurrentState { get; set; } = string.Empty;

    public Guid? AuthId { get; set; }
    public Guid? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ConfirmationToken { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    public string AssignedRole { get; set; } = "User"; // default role

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
}
