using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IH.LibrarySystem.IntegrationTests.Auth;

/// <summary>
/// Mocks authentication for integration tests so we don't have to deal with real Google tokens.
/// It uses test headers to fake who is logged in, while still letting our actual [Authorize] policies run.
///
/// How to use it via headers:
///  - (None) -> Logs you in as a standard Member (safe default).
///  - X-Test-Role: Staff|Admin -> Logs you in with that specific role.
///  - X-Test-Anonymous: true -> Forces an unauthenticated (401) request.
/// </summary>
public sealed class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public const string RoleHeader = "X-Test-Role";
    public const string AnonymousHeader = "X-Test-Anonymous";

    public static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string DefaultEmail = "test-member@ihlibrary.local";
    public const string DefaultDisplayName = "Test Member";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder
    )
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (
            Request.Headers.TryGetValue(AnonymousHeader, out var anonymousValue)
            && string.Equals(anonymousValue, "true", StringComparison.OrdinalIgnoreCase)
        )
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = "Member";
        if (
            Request.Headers.TryGetValue(RoleHeader, out var roleValue)
            && !string.IsNullOrWhiteSpace(roleValue)
        )
        {
            role = roleValue!;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, DefaultUserId.ToString()),
            new("sub", DefaultUserId.ToString()),
            new(ClaimTypes.Email, DefaultEmail),
            new(ClaimTypes.Name, DefaultDisplayName),
            new(ClaimTypes.Role, role),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
