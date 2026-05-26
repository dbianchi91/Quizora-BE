using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace Identity.Domain.Entities;

public sealed class RefreshToken : BaseEntity<Guid>
{
    public string Token { get; private set; } = default!;
    public UserId UserId { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public static RefreshToken Create(UserId userId, string token, DateTime expiresAt, string? createdByIp = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };
    }

    public void Revoke() => RevokedAt = DateTime.UtcNow;
}
