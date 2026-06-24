using IH.LibrarySystem.Domain.Identity;
using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(LibraryDbContext context) : IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
        await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task AddAsync(RefreshToken refreshToken) =>
        await context.RefreshTokens.AddAsync(refreshToken);

    public async Task RevokeAllActiveForUserAsync(Guid userId, string? revokedByIp)
    {
        var activeTokens = await context
            .RefreshTokens.Where(t =>
                t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow
            )
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedByIp);
        }
    }
}
