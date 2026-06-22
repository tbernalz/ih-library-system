namespace IH.LibrarySystem.Application.Identity;

/// <summary>
/// Verified claims extracted from a Google ID token, after signature, issuer, audience, and
/// expiry validation has already happened.
/// </summary>
public record GoogleIdentity(
    string Subject,
    string Email,
    bool EmailVerified,
    string Name,
    string? Picture
);

/// <summary>
/// Abstraction over Google's ID token verification so the Application layer never depends on
/// the Google SDK directly — only Infrastructure does (Dependency Inversion).
/// </summary>
public interface IGoogleTokenVerifier
{
    Task<GoogleIdentity?> VerifyAsync(
        string idToken,
        CancellationToken cancellationToken = default
    );
}
