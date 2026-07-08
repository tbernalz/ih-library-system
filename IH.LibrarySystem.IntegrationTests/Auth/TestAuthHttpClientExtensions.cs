namespace IH.LibrarySystem.IntegrationTests.Auth;

/// <summary>
/// Fluent helpers for controlling the fake authenticated identity used by
/// <see cref="TestAuthenticationHandler"/> in integration tests.
/// </summary>
public static class TestAuthHttpClientExtensions
{
    /// <summary>
    /// Sends subsequent requests as the given role (e.g. "Staff", "Admin").
    /// Clears any previously-set anonymous header.
    /// </summary>
    public static HttpClient AsRole(this HttpClient client, string role)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.AnonymousHeader);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, role);
        return client;
    }

    /// <summary>
    /// Sends subsequent requests as the default authenticated Member (the baseline identity).
    /// </summary>
    public static HttpClient AsMember(this HttpClient client) => client.AsRole("Member");

    /// <summary>
    /// Sends subsequent requests as the given member ID.
    /// </summary>
    public static HttpClient AsMember(this HttpClient client, Guid memberId)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.AnonymousHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.UserIdHeader);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RoleHeader, "Member");
        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.UserIdHeader,
            memberId.ToString()
        );
        return client;
    }

    public static HttpClient AsStaff(this HttpClient client) => client.AsRole("Staff");

    public static HttpClient AsAdmin(this HttpClient client) => client.AsRole("Admin");

    /// <summary>
    /// Sends subsequent requests with no identity at all, so [Authorize] genuinely rejects
    /// them with 401 — useful for testing the unauthenticated path explicitly.
    /// </summary>
    public static HttpClient AsAnonymous(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.RoleHeader);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.AnonymousHeader);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.AnonymousHeader, "true");
        return client;
    }
}
