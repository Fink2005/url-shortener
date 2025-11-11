namespace Contracts.Mail;

public record VerifyEmailRequest(string Email, string Token);
public record VerifyEmailResponse(bool Success, string Message);
public record CheckEmailTokenRequest(string Email);
public record CheckEmailTokenResponse(bool Success, string Message, string? Token);

public record SendConfirmationEmailRequest(string Email);
public record SendConfirmationEmailResponse(bool Success);