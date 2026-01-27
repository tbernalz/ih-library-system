using IH.LibrarySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using LibraryDbContext context =
            scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

        await context.Database.MigrateAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<LibraryDataSeeder>();
        await seeder.SeedAsync();
    }
}
