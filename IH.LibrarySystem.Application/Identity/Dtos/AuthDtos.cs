using System.ComponentModel.DataAnnotations;

namespace IH.LibrarySystem.Application.Identity.Dtos;

/// <summary>
/// Sent by the client after Google Identity Services (on the frontend) produces an ID token.
/// This is the Google-issued JWT, NOT an access token — we verify it server-side and never
/// forward it anywhere else.
/// </summary>
public record GoogleLoginRequest([Required] string IdToken);

/// <summary>
/// Issued after a successful Google login or a successful refresh.
/// </summary>
public record AuthTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt
);

public record RefreshTokenRequest([Required] string RefreshToken);

public record RevokeTokenRequest([Required] string RefreshToken);

public record CurrentUserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string Role
);
