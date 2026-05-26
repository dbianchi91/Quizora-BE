using Identity.Domain.Events;
using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace Identity.Domain.Entities;

public sealed class User : BaseEntity<UserId>
{
    public Email Email { get; private set; } = default!;
    public string UserName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsEmailConfirmed { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsAdmin { get; private set; }
    public bool IsCreator { get; private set; }

    public void AssignCreatorRole() => IsCreator = true;
    public void RevokeCreatorRole() => IsCreator = false;

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static Result<User> Create(Email email, string userName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure<User>(Error.Validation("UserName", "Username cannot be empty."));

        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<User>(Error.Validation("PasswordHash", "Password hash cannot be empty."));

        var user = new User
        {
            Id = UserId.New(),
            Email = email,
            UserName = userName.Trim(),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsEmailConfirmed = false
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            user.Id,
            email.Value));

        return Result.Success(user);
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserLoggedInEvent(Guid.NewGuid(), DateTime.UtcNow, Id));
    }

    public RefreshToken AddRefreshToken(string token, DateTime expiresAt, string? ip = null)
    {
        var refreshToken = RefreshToken.Create(Id, token, expiresAt, ip);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }

    public RefreshToken? FindActiveRefreshToken(string token) =>
        _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);

    public void RevokeRefreshToken(string token)
    {
        var rt = FindActiveRefreshToken(token);
        rt?.Revoke();
    }

    public void ConfirmEmail() => IsEmailConfirmed = true;
}
