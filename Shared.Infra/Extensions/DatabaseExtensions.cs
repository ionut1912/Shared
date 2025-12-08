using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infra.Extensions;

public static class DatabaseExtension
{
    public static void MigrateDatabase<T>(this IServiceProvider services) where T : DbContext
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<T>();
        try
        {
            var pendingMigrations = dbContext.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                Console.WriteLine("Applying pending migrations...");
                dbContext.Database.Migrate();
            }
            else
            {
                Console.WriteLine("Database is up-to-date.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while checking/applying migrations: {ex.Message}");
        }
    }

    public static IServiceCollection AddDatabaseContext<TContext>(
    this IServiceCollection services,
    IConfiguration configuration) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}