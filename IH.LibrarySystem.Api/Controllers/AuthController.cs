using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Identity;
using IH.LibrarySystem.Application.Identity.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IClientRequestContext clientRequest)
    : ControllerBase
{
    /// <summary>
    /// Exchanges a Google ID token (obtained client-side via Google Identity Services) for
    /// this API's own access/refresh token pair. Creates the local user on first login.
    /// </summary>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokenResponse>> LoginWithGoogle(
        GoogleLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.LoginWithGoogleAsync(
            request,
            clientRequest.ClientIpAddress,
            cancellationToken
        );
        return Ok(result);
    }

    /// <summary>
    /// Rotates a refresh token: the old one is revoked and a brand-new access/refresh pair is
    /// issued. Reuse of an already-rotated token revokes the whole session family.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await authService.RefreshAsync(
            request,
            clientRequest.ClientIpAddress,
            cancellationToken
        );
        return Ok(result);
    }

    /// <summary>
    /// Logs out a single session by revoking its refresh token. The short-lived access token
    /// already in the client's hands will simply expire on its own (by design — see README).
    /// </summary>
    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(
        RevokeTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        await authService.RevokeAsync(request, clientRequest.ClientIpAddress, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user, resolved from the access
    /// token's "sub" claim.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var result = await authService.GetCurrentUserAsync(cancellationToken);
        return Ok(result);
    }
}
