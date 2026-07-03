using IH.LibrarySystem.Application.Identity.Dtos;

namespace IH.LibrarySystem.Application.Identity;

public interface IAuthService
{
    /// <summary>
    /// Verifies a Google ID token, finds-or-creates the local <see cref="Domain.Identity.User"/>,
    /// and issues a fresh access/refresh token pair.
    /// </summary>
    Task<AuthTokenResponse> LoginWithGoogleAsync(
        GoogleLoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Validates and rotates a refresh token, returning a new access/refresh token pair.
    /// Detects reuse of an already-rotated token and revokes the entire token family as a
    /// security response.
    /// </summary>
    Task<AuthTokenResponse> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Revokes a single refresh token (logout from one device/session).
    /// </summary>
    Task RevokeAsync(
        RevokeTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default
    );

    Task<CurrentUserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
