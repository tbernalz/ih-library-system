using System.Security.Cryptography;
using System.Text;
using IH.LibrarySystem.Application.Identity;

namespace IH.LibrarySystem.Infrastructure.Identity;

/// <summary>
/// Provides secure, fast SHA-256 one-way hashing for high-entropy refresh tokens at rest.
/// Because tokens are randomly generated and fixed-length, a fast cryptographic hash is preferred
/// over a slow KDF (like Argon2), protecting the database from leak vectors without unnecessary overhead.
/// </summary>
public sealed class Sha256RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
