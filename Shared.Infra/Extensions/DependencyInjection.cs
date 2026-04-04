using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Infra.Extensions;

/// <summary>
///     Provides extension methods for registering database services and repositories
///     and performing database migrations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers a database context of type <typeparamref name="TDbC" /> using the provided configuration.
    /// </summary>
    /// <typeparam name="TDbC">The type of the <see cref="DbContext" /> to register.</typeparam>
    /// <param name="services">The service collection to add the database context to.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddDatabase<TDbC>(this IServiceCollection services, IConfiguration configuration)
        where TDbC : DbContext
    {
        services.AddDatabaseContext<TDbC>(configuration);
        return services;
    }

    /// <summary>
    ///     Registers a repository interface and its implementation with the dependency injection container.
    /// </summary>
    /// <typeparam name="TRepo">The repository interface type.</typeparam>
    /// <typeparam name="TRepoImpl">The concrete implementation type of the repository.</typeparam>
    /// <param name="services">The service collection to register the repository with.</param>
    /// <returns>The updated <see cref="IServiceCollection" /> for chaining.</returns>
    public static IServiceCollection AddRepos<TRepo, TRepoImpl>(this IServiceCollection services)
        where TRepo : class
        where TRepoImpl : class, TRepo
    {
        services.AddScoped<TRepo, TRepoImpl>();
        return services;
    }

    /// <summary>
    ///     Applies pending database migrations for the specified <see cref="DbContext" /> type.
    /// </summary>
    /// <typeparam name="TDbc">The type of the <see cref="DbContext" /> to migrate.</typeparam>
    /// <param name="app">The application builder used to access services.</param>
    /// <returns>The updated <see cref="IApplicationBuilder" /> for chaining.</returns>
    /// <remarks>
    ///     This method calls <see cref="DatabaseExtension.MigrateDatabase{T}" /> internally
    ///     to ensure the database is up-to-date with all migrations.
    /// </remarks>
    public static IApplicationBuilder MigrateServiceDatabase<TDbc>(this IApplicationBuilder app) where TDbc : DbContext
    {
        app.ApplicationServices.MigrateDatabase<TDbc>();
        return app;
    }

    /// <summary>
    ///     Registers a repository for a specific entity type using a DbSet from the provided DbContext,
    ///     and maps it to the corresponding repository interface.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TRepository">The concrete repository type.</typeparam>
    /// <typeparam name="TInterface">The repository interface type.</typeparam>
    /// <typeparam name="TDbContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRepository<TEntity, TRepository, TInterface, TDbContext>(
        this IServiceCollection services)
        where TEntity : class
        where TRepository : class, TInterface
        where TInterface : class
        where TDbContext : DbContext
    {
        services.AddScoped(sp =>
        {
            var dbContext = sp.GetRequiredService<TDbContext>();
            var dbSet = dbContext.Set<TEntity>();
            return (TInterface)Activator.CreateInstance(typeof(TRepository), dbSet)!;
        });

        return services;
    }
}