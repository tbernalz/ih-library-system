using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IH.LibrarySystem.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<LibraryDbContext>();
        var logger = services.GetRequiredService<ILogger<LibraryDbContext>>();
        var seeder = services.GetRequiredService<LibraryDataSeeder>();

        try
        {
            logger.LogInformation("Starting database initialization...");

            var retryCount = 0;
            const int maxRetries = 3;

            while (retryCount < maxRetries)
            {
                try
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation(
                            "Applying {Count} pending migrations...",
                            pendingMigrations.Count()
                        );
                        await context.Database.MigrateAsync();
                    }
                    else
                    {
                        logger.LogInformation("No pending migrations found.");
                    }

                    break;
                }
                catch (Exception ex) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    logger.LogWarning(
                        ex,
                        "Migration attempt {Count} failed (Database might be starting up). Retrying in 2s...",
                        retryCount
                    );
                    await Task.Delay(2000);
                }
            }

            logger.LogInformation("Applying data seeding...");
            await seeder.SeedAsync();

            logger.LogInformation("Database migration and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "A fatal error occurred during the database initialization phase."
            );

            throw;
        }
    }
}
