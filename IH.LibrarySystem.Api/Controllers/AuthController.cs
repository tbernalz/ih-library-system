using IH.LibrarySystem.Application.Common.Abstractions;
using IH.LibrarySystem.Application.Configuration;
using IH.LibrarySystem.Application.Identity;
using IH.LibrarySystem.Application.Identity.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAuthService authService,
    IClientRequestContext clientRequest,
    IOptions<GoogleAuthSettings> googleAuthSettings
) : ControllerBase
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

    /// <summary>
    /// Redirects the browser straight to the Google Account Chooser screen,
    /// telling Google to send the token back to our local API callback.
    /// </summary>
    [HttpGet("connect/google")]
    [AllowAnonymous]
    public IActionResult ConnectGoogle()
    {
        var clientId = googleAuthSettings.Value.ClientId;

        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var localCallback = $"{scheme}://{host}/api/auth/callback";

        var googleAuthUrl =
            "https://accounts.google.com/o/oauth2/v2/auth?"
            + $"client_id={Uri.EscapeDataString(clientId)}"
            + $"&redirect_uri={Uri.EscapeDataString(localCallback)}"
            + "&response_type=id_token"
            + "&scope=openid%20email%20profile"
            + "&prompt=select_account"
            + "&response_mode=form_post"
            + "&nonce="
            + Guid.NewGuid().ToString("N");

        return Redirect(googleAuthUrl);
    }

    /// <summary>
    /// Catches the token from Google, processes it, and displays all generated tokens on a clean webpage.
    /// </summary>
    [HttpPost("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback([FromForm] string id_token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id_token))
            {
                return Content(BuildErrorHtml("No ID token received from Google."), "text/html");
            }

            var result = await authService.LoginWithGoogleAsync(
                new GoogleLoginRequest(id_token),
                clientRequest.ClientIpAddress,
                CancellationToken.None
            );

            return Content(BuildSuccessHtml(result, id_token), "text/html");
        }
        catch (Exception ex)
        {
            return Content(BuildErrorHtml($"Authentication failed: {ex.Message}"), "text/html");
        }
    }

    private static string BuildSuccessHtml(AuthTokenResponse result, string googleToken)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Authentication Successful</title>
            <style>
                body {{ font-family: sans-serif; background: #121212; color: #e0e0e0; padding: 40px; max-width: 800px; margin: 0 auto; }}
                h2 {{ color: #4CAF50; border-bottom: 1px solid #333; padding-bottom: 10px; }}
                .token-section {{ margin-bottom: 25px; }}
                label {{ display: block; font-weight: bold; margin-bottom: 5px; color: #aaa; }}
                textarea {{ width: 100%; height: 80px; background: #1e1e1e; color: #fff; border: 1px solid #333; padding: 10px; font-family: monospace; font-size: 14px; box-sizing: border-box; resize: none; }}
                .btn {{ background: #4CAF50; color: white; padding: 8px 12px; border: none; cursor: pointer; margin-top: 5px; font-weight: bold; border-radius: 4px; }}
                .btn:hover {{ background: #45a049; }}
                .info {{ color: #00bcd4; font-family: monospace; font-size: 14px; margin-top: 5px; }}
            </style>
            <script>
                function copyToClipboard(id) {{
                    var copyText = document.getElementById(id);
                    navigator.clipboard.writeText(copyText.value);
                }}
            </script>
        </head>
        <body>
            <h2>Authentication Successful!</h2>
            <p>Your application tokens have been generated. Use these in Postman:</p>

            <div class='token-section'>
                <label for='accessToken'>App Access Token:</label>
                <textarea id='accessToken' readonly>{result.AccessToken}</textarea>
                <button class='btn' onclick=""copyToClipboard('accessToken')"">Copy Access Token</button>
                <div class='info'>Expires: {result.AccessTokenExpiresAt:yyyy-MM-dd HH:mm:ss} UTC</div>
            </div>

            <div class='token-section'>
                <label for='refreshToken'>App Refresh Token:</label>
                <textarea id='refreshToken' readonly>{result.RefreshToken}</textarea>
                <button class='btn' onclick=""copyToClipboard('refreshToken')"">Copy Refresh Token</button>
                <div class='info'>Expires: {result.RefreshTokenExpiresAt:yyyy-MM-dd HH:mm:ss} UTC</div>
            </div>

            <div class='token-section'>
                <label for='googleToken'>Original Google ID Token:</label>
                <textarea id='googleToken' readonly>{googleToken}</textarea>
                <button class='btn' onclick=""copyToClipboard('googleToken')"">Copy Google Token</button>
            </div>
        </body>
        </html>";
    }

    private static string BuildErrorHtml(string errorMessage)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Authentication Failed</title>
            <style>
                body {{ font-family: sans-serif; background: #121212; color: #e0e0e0; padding: 40px; max-width: 800px; margin: 0 auto; }}
                h2 {{ color: #f44336; border-bottom: 1px solid #333; padding-bottom: 10px; }}
                .error {{ color: #ff6b6b; background: #2d1f1f; padding: 15px; border-radius: 4px; border-left: 4px solid #f44336; }}
            </style>
        </head>
        <body>
            <h2>Authentication Failed</h2>
            <div class='error'>{errorMessage}</div>
        </body>
        </html>";
    }
}
