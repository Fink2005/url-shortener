namespace UserService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public Guid AuthId { get; private set; }
    public string Username { get; private set; }
    public string Email { get; private set; }

    private User() { } // EF Core constructor

    public User(Guid authId, string username, string email)
    {
        if (authId == Guid.Empty)
            throw new ArgumentException("AuthId is required");

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        Id = Guid.NewGuid();
        AuthId = authId;
        Username = username;
        Email = email;
    }

    public void UpdateEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email cannot be empty");

        Email = newEmail;
    }
}
