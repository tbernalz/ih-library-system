using IH.LibrarySystem.Domain.Identity;

namespace IH.LibrarySystem.Application.Identity;

public record GeneratedAccessToken(string Token, DateTime ExpiresAt);

public interface ITokenService
{
    GeneratedAccessToken GenerateAccessToken(User user);

    string GenerateRefreshToken();
}

/// <summary>
/// One-way hashing for refresh tokens at rest. Deliberately separate from <see cref="ITokenService"/>
/// so the hashing algorithm can be swapped without touching token generation/signing.
/// </summary>
public interface IRefreshTokenHasher
{
    string Hash(string rawToken);
}
