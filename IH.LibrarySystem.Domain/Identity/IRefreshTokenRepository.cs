namespace IH.LibrarySystem.Domain.Identity;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    Task AddAsync(RefreshToken refreshToken);

    Task RevokeAllActiveForUserAsync(Guid userId, string? revokedByIp);
}
