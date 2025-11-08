namespace Contracts.Auth;


public record RegisterUserRequest(
    string Username,
    string Email,
    string Password
);


public record RegisterUserResponse(
    bool Success
);


public record LoginUserRequest(
    string Username,
    string Password
);


public record LoginUserResponse(
    string AccessToken,
    string RefreshToken
);


public record RefreshTokenRequest(
    string RefreshToken
);

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);


public record LogoutRequest(
    string RefreshToken
);


public record LogoutResponse(
    bool Success
);
