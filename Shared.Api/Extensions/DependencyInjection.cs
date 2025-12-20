using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Abstractions;
using Shared.Api.Handlers;
using Shared.Application.Extensions;
using Shared.Infra.Extensions;
using System.Reflection;

namespace Shared.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring dependency injection, database, repositories, application, and presentation layers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds OpenTelemetry logging to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder, string otelEndpoint, string serviceName, string environmentName)
    {
        var resourceBuilder = ServiceCollectionExtensions.CreateServiceResourceBuilder(serviceName, environmentName);
        builder.Logging.AddOpenTelemetryLogging(otelEndpoint, resourceBuilder);
        return builder;
    }

    /// <summary>
    /// Adds and configures the database context of type <typeparamref name="Tdbc"/>.
    /// </summary>
    public static IServiceCollection AddDatabaseConfig<Tdbc>(this IServiceCollection services, IConfiguration configuration) where Tdbc : DbContext
    {
        services.AddDatabase<Tdbc>(configuration);
        return services;
    }

    /// <summary>
    /// Adds and configures repository services of type <typeparamref name="TRepo"/> with implementation <typeparamref name="TRepoImpl"/>.
    /// </summary>
    public static IServiceCollection AddRepositoriesConfig<TRepo, TRepoImpl>(this IServiceCollection services)
        where TRepo : class
        where TRepoImpl : class, TRepo
    {
        services.AddRepos<TRepo, TRepoImpl>();
        return services;
    }

    /// <summary>
    /// Registers all generic repository services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> with generic repository services registered.</returns>
    public static IServiceCollection AddGenericRepositoriesConfig(this IServiceCollection services)
    {
        services.AddGenericRepos();
        return services;
    }


    /// <summary>
    /// Registers a repository for a specific entity type using a DbSet from the provided DbContext,
    /// and maps it to the corresponding repository interface.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TRepository">The concrete repository type.</typeparam>
    /// <typeparam name="TInterface">The repository interface type.</typeparam>
    /// <typeparam name="TDbContext">The DbContext type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddRepository<TEntity, TRepository, TInterface, TDbContext>(this IServiceCollection services)
        where TEntity : class
        where TRepository : class, TInterface
        where TInterface : class
        where TDbContext : DbContext
    {
        services.AddScoped<TInterface>(sp =>
        {
            var context = sp.GetRequiredService<TDbContext>();
            var dbSet = context.Set<TEntity>();
            return (TInterface)Activator.CreateInstance(typeof(TRepository), dbSet)!;
        });

        return services;
    }

    /// <summary>
    /// Applies any pending migrations for the database context of type <typeparamref name="TDbc"/>.
    /// </summary>
    public static IApplicationBuilder MigrateDatabaseConfig<TDbc>(this IApplicationBuilder app) where TDbc : DbContext
    {
        app.MigrateServiceDatabase<TDbc>();
        return app;
    }

    /// <summary>
    /// Adds and configures application services, including MediatR and validation.
    /// </summary>
    public static IServiceCollection AddAplicationConfig(this IServiceCollection services, Assembly mediatorAssembly, Assembly validationAssembly)
    {
        services.AddApplicationServices(mediatorAssembly, validationAssembly);
        return services;
    }

    /// <summary>
    /// Adds and configures the presentation layer services.
    /// </summary>
    public static IServiceCollection AddPresentation<T>(this IServiceCollection services, IConfiguration configuration, string otelEndpoint, string serviceName, string environmentName) where T : class, IExceptionProblemDetailsMapper
    {
        var resourceBuilder = ServiceCollectionExtensions.CreateServiceResourceBuilder(serviceName, environmentName);

        services
            .AddJwtAuthentication(configuration)
            .AddRoleBasedAuthorization();

        services.AddOpenTelemetryObservability(otelEndpoint, serviceName, resourceBuilder);

        services.AddOpenApiWithJwtAuth($"{serviceName}-Api");

        services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
        services.AddSingleton<IExceptionProblemDetailsMapper, T>();

        services.AddHealthChecks();

        services.AddEndpointsApiExplorer();
        services.AddControllers();

        return services;
    }
}
