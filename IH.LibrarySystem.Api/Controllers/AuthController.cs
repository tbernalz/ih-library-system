using System.Security.Claims;
using IH.LibrarySystem.Application.Identity;
using IH.LibrarySystem.Application.Identity.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
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
            GetClientIpAddress(),
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
            GetClientIpAddress(),
            cancellationToken
        );
        return Ok(result);
    }

    /// <summary>
    /// Logs out a single session by revoking its refresh token. The short-lived access token
    /// already in the client's hands will simply expire on its own (by design — see README).
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke(
        RevokeTokenRequest request,
        CancellationToken cancellationToken
    )
    {
        await authService.RevokeAsync(request, GetClientIpAddress(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user, resolved from the access
    /// token's "sub" claim.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await authService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        if (sub is null || !Guid.TryParse(sub, out var userId))
        {
            throw new UnauthorizedAccessException("Access token is missing a valid subject claim.");
        }

        return userId;
    }

    private string? GetClientIpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
