namespace Contracts.Users;

// ===== GET USER =====
public record GetUserRequest
{
    public Guid Id { get; init; }

    public GetUserRequest() { }
    public GetUserRequest(Guid id) => Id = id;
}

public record GetUserResponse
{
    public Guid Id { get; init; }

    public Guid AuthId { get; init; }

    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public GetUserResponse() { }
    public GetUserResponse(Guid authId, Guid id, string username, string email)
    {
        Id = id;
        AuthId = authId;
        Username = username;
        Email = email;
    }
}

// ===== CREATE USER =====
public record CreateUserRequest
{
    public Guid AuthId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public CreateUserRequest() { }
    public CreateUserRequest(Guid authId, string username, string email)
    {
        AuthId = authId;
        Username = username;
        Email = email;
    }
}

public record CreateUserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public CreateUserResponse() { }
    public CreateUserResponse(Guid id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
    }
}



public record DeleteUserRequest
{
    public Guid Id { get; init; }

    public DeleteUserRequest() { }
    public DeleteUserRequest(Guid id) => Id = id;
}


public record DeleteUserResponse(bool Success);