using IH.LibrarySystem.Api.Extensions;
using IH.LibrarySystem.Application.Notifications;
using IH.LibrarySystem.IntegrationTests.Auth;
using IH.LibrarySystem.IntegrationTests.Stubs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace IH.LibrarySystem.IntegrationTests.Fixtures;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .Build();

    private LibraryWebApplicationFactory? _factory;

    /// <summary>
    /// Default client. Thanks to <see cref="TestAuthenticationHandler"/>, requests with no
    /// extra headers are authenticated as a standard Member — call the
    /// <c>AsRole</c>/<c>AsAdmin</c>/<c>AsStaff</c>/<c>AsAnonymous</c> extension methods
    /// (see <see cref="TestAuthHttpClientExtensions"/>) to change identity per-test.
    /// </summary>
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync().ConfigureAwait(false);

        _factory = new LibraryWebApplicationFactory(_postgres.GetConnectionString());
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync().ConfigureAwait(false);
        }

        await _postgres.DisposeAsync().ConfigureAwait(false);
    }

    public IServiceScope CreateScope() =>
        (
            _factory ?? throw new InvalidOperationException("Fixture not initialized.")
        ).Services.CreateScope();

    /// <summary>
    /// Creates a fresh, independently-configurable HttpClient against the same test server.
    /// Useful when a test needs two clients with different identities at once (e.g. asserting
    /// a Member can't see what an Admin can, side by side).
    /// </summary>
    public HttpClient CreateClient() =>
        (
            _factory ?? throw new InvalidOperationException("Fixture not initialized.")
        ).CreateClient();

    private Respawner _respawner = default!;

    public async Task ResetDatabaseAsync()
    {
        using var connection = new NpgsqlConnection(_postgres.GetConnectionString());
        await connection.OpenAsync();

        if (_respawner == null)
        {
            _respawner = await Respawner.CreateAsync(
                connection,
                new RespawnerOptions
                {
                    DbAdapter = DbAdapter.Postgres,
                    TablesToIgnore = ["__EFMigrationsHistory"],
                }
            );
        }

        await _respawner.ResetAsync(connection);
    }
}

internal sealed class LibraryWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public LibraryWebApplicationFactory(string connectionString) =>
        _connectionString =
            connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(MigrationExtensions.IntegrationTestingEnvironment);

        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);

        builder.UseSetting("Discovery:EnableBackgroundIngestion", "false");

        builder.UseSetting("AiSettings:Provider", "openrouter");
        builder.UseSetting("AiSettings:Model", "integration-test");
        builder.UseSetting("AiSettings:ApiKey", "test-key");
        builder.UseSetting("AiSettings:BaseUrl", "https://example.invalid");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatClient>();
            services.AddSingleton<IChatClient, StubChatClient>();

            services.RemoveAll<IEmbeddingGenerator<string, Embedding<float>>>();
            services.AddSingleton<
                IEmbeddingGenerator<string, Embedding<float>>,
                StubEmbeddingGenerator
            >();

            services.RemoveAll<IEmailNotificationService>();
            services.AddSingleton<IEmailNotificationService, StubEmailNotificationService>();

            services
                .AddAuthentication(TestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { }
                );
        });
    }
}
