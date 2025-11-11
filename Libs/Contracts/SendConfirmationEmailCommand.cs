namespace Contracts;

public record SendConfirmationEmailCommand(string Email, string Token);
