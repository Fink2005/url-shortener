namespace Contracts.Saga;

using System;
using System.Collections.Generic;
using Contracts.Url;
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


public record RegisterSagaRequest(string Email, string Password, string Username);

public record RegisterSagaResponse(string Message);

// Verify Email Saga
public record VerifyEmailRequestedEvent(string Email, string Token);
public record EmailVerifiedEvent(Guid CorrelationId, string Email);
public record EmailVerificationFailedEvent(Guid CorrelationId, string Email, string Reason);

// Admin Dashboard - Get User With URLs Saga
public record GetUserWithUrlsRequest(Guid UserId);
public record GetUserWithUrlsResponse(UserWithUrlsDto UserWithUrls);

// Admin Dashboard - Get All Users With URLs Saga
public record GetAllUsersWithUrlsRequest();
public record GetAllUsersWithUrlsResponse(List<UserWithUrlsDto> Users);

// Admin Dashboard - Delete User Saga
public record DeleteUserSagaRequest(Guid UserId);
public record DeleteUserSagaResponse(bool Success, string Message);

public class UserWithUrlsDto
{
    public Guid UserId { get; set; }
    public Guid AuthId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public List<UrlDto> Urls { get; set; } = new();
}