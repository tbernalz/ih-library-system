namespace IH.LibrarySystem.Domain.Identity;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        bool readOnly = false,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(RefreshToken refreshToken);

    Task RevokeAllActiveForUserAsync(Guid userId, string? revokedByIp);
}
