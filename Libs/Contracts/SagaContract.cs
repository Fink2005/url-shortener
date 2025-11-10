namespace Contracts.Saga;

// Command khởi động quy trình
public record StartUserOnboarding(Guid CorrelationId, string Username, string Email, string Password);

// Commands sang các service con
public record CreateAuthUserCommand(Guid CorrelationId, string Username, string Email, string Password);
public record DeleteAuthUserCommand(Guid CorrelationId, Guid AuthUserId);
public record CreateUserProfileCommand(Guid CorrelationId, Guid AuthUserId, string Username, string Email);

// Events từ các service con báo về
public record AuthUserCreated(Guid CorrelationId, Guid AuthUserId);
public record AuthUserCreateFailed(Guid CorrelationId, string Reason);
public record AuthUserDeleted(Guid CorrelationId);

public record UserProfileCreated(Guid CorrelationId, Guid UserId);
public record UserProfileCreateFailed(Guid CorrelationId, string Reason);

// (Optional) timeout/expiry cho saga
public record ExpireOnboarding(Guid CorrelationId, string Reason = "Timeout");
