namespace Contracts.Saga;

using System;
using System.Collections.Generic;
// Command khởi động quy trình
public record StartUserOnboarding(Guid CorrelationId, string Username, string Email, string Password);

// Commands sang các service con
public record CreateAuthUserCommand(Guid CorrelationId, string Username, string Email, string Password);
public record DeleteAuthUserCommand(Guid CorrelationId, Guid AuthUserId);
public record CreateUserProfileCommand(Guid CorrelationId, Guid AuthUserId, string Username, string Email);

// Events từ các service con báo về
public record AuthUserCreated(Guid CorrelationId, Guid AuthUserId, string Email);
public record AuthUserCreateFailed(Guid CorrelationId, string Reason);
public record AuthUserDeleted(Guid CorrelationId);

public record SendConfirmationEmailCommand(Guid CorrelationId, string Email, string ConfirmationToken);
public record EmailConfirmationSent(Guid CorrelationId);

public record AssignDefaultRoleCommand(Guid CorrelationId, Guid AuthUserId);
public record DefaultRoleAssigned(Guid CorrelationId, Guid AuthUserId, string Role);

public record UserProfileCreated(Guid CorrelationId, Guid UserId, Guid AuthUserId);
public record UserProfileCreateFailed(Guid CorrelationId, string Reason);

// (Optional) timeout/expiry cho saga
public record ExpireOnboarding(Guid CorrelationId, string Reason = "Timeout");
public record OnboardingFailed(Guid CorrelationId, string Reason);
