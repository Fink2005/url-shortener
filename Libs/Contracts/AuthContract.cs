using System;
using System.Collections.Generic;

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

public record GetAllAuthUsersRequest();

public record GetAllAuthUsersResponse(List<AuthUserDto> Users);

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}

public record PromoteToAdminRequest(Guid UserId);

public record PromoteToAdminResponse(bool Success, string Message);

// Get Auth by Id for saga
public record GetAuthByIdRequest(Guid Id);

public record GetAuthByIdResponse(
    Guid Id,
    string Username,
    string Email,
    string Role,
    bool IsEmailVerified
);

// Batch get auth by multiple Ids
public record GetAuthsByIdsRequest(List<Guid> Ids);

public record GetAuthsByIdsResponse(List<AuthByIdDto> AuthUsers);

public class AuthByIdDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
}
