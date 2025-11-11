namespace Contracts.Saga.Auth;

public record VerifyEmailRequestDto(
    string Email,
    string Token
);
