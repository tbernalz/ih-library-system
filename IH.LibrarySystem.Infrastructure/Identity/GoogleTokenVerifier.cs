using Google.Apis.Auth;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Infrastructure.Identity;

/// <summary>
/// Verifies Google-issued ID tokens server-side using Google's published certificates
/// (signature, issuer, audience, and expiry — all handled by <see cref="GoogleJsonWebSignature"/>).
/// The frontend uses Google Identity Services to obtain an ID token, and the backend verifies
/// it rather than trusting anything the client claims about the user.
/// </summary>
public sealed class GoogleTokenVerifier(
    IOptions<GoogleAuthSettings> googleAuthSettings,
    ILogger<GoogleTokenVerifier> logger
) : IGoogleTokenVerifier
{
    private readonly GoogleAuthSettings _settings = googleAuthSettings.Value;

    public async Task<GoogleIdentity?> VerifyAsync(
        string idToken,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.ClientId],
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            return new GoogleIdentity(
                Subject: payload.Subject,
                Email: payload.Email,
                EmailVerified: payload.EmailVerified,
                Name: string.IsNullOrWhiteSpace(payload.Name) ? payload.Email : payload.Name,
                Picture: payload.Picture
            );
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Rejected Google ID token: failed signature/claims validation.");
            return null;
        }
    }
}
