using System;

namespace Contracts.Auth;

public record RegisterAuthRequest(
    string Username,
    string Email,
    string Password
);

public record RegisterAuthResponse(
    bool Success
);

public record LoginAuthRequest(
    string Username,
    string Password
);


public record LoginAuthResponse(
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

public record DeleteAuthRequest(
    Guid Id
);
public record DeleteAuthResponse(
    bool Success
);

public record LogoutRequest(
    string RefreshToken
);


public record LogoutResponse(
    bool Success
);
