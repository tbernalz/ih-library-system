using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<LibraryDbContext>>();

        try
        {
            using LibraryDbContext context = services.GetRequiredService<LibraryDbContext>();

            logger.LogInformation("Applying migrations...");
            await context.Database.MigrateAsync();

            var seeder = services.GetRequiredService<LibraryDataSeeder>();
            await seeder.SeedAsync();
            logger.LogInformation("Database migration and seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating the database.");
            throw;
        }
    }
}
