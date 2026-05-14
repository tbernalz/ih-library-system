using IH.LibrarySystem.Api.Extensions;
using IH.LibrarySystem.IntegrationTests.Stubs;
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
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    private LibraryWebApplicationFactory? _factory;

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

        builder.UseSetting("AiSettings:Provider", "openrouter");
        builder.UseSetting("AiSettings:Model", "integration-test");
        builder.UseSetting("AiSettings:ApiKey", "test-key");
        builder.UseSetting("AiSettings:BaseUrl", "https://example.invalid");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IChatClient>();
            services.AddSingleton<IChatClient, StubChatClient>();
        });
    }
}
