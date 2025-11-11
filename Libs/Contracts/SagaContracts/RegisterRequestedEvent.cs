namespace Contracts.Saga.Auth;

public record RegisterRequestedEvent(
    string Username,
    string Email,
    string Password
);
