using IH.LibrarySystem.Domain.SharedKernel;

namespace IH.LibrarySystem.Domain.Identity;

public class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsExpired && !IsRevoked;

    private RefreshToken()
        : base(Guid.Empty) { }

    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? createdByIp
    )
        : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(
        Guid id,
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? createdByIp = null
    ) => new(id, userId, tokenHash, expiresAt, createdByIp);

    public void RevokeAndReplace(string newTokenHash, string? revokedByIp)
    {
        if (IsRevoked)
            throw new InvalidOperationException($"RefreshToken {Id} is already revoked.");

        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = newTokenHash;
        RevokedByIp = revokedByIp;
        SetUpdated();
    }

    public void Revoke(string? revokedByIp)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        SetUpdated();
    }
}
