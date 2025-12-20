using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infra.Extensions;

/// <summary>
/// Provides extension methods for registering and managing database contexts.
/// </summary>
public static class DatabaseExtension
{
    /// <summary>
    /// Applies any pending migrations for the specified <see cref="DbContext"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="DbContext"/>.</typeparam>
    /// <param name="services">The service provider used to create a scope and resolve the context.</param>
    /// <remarks>
    /// This method creates a scope to resolve the database context, checks for pending migrations,
    /// and applies them if any exist. Errors during migration are logged to the console.
    /// </remarks>
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

    /// <summary>
    /// Registers the specified <see cref="DbContext"/> with the dependency injection container
    /// using a PostgreSQL connection string from configuration.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/> to register.</typeparam>
    /// <param name="services">The service collection to register the context with.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDatabaseContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }
}
