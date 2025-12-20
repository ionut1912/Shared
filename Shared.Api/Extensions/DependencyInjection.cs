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
/// Provides extension methods for configuring dependency injection, database, repositories, application, and presentation layers,
/// as well as OpenTelemetry and health checks for ASP.NET Core applications.
/// </summary>
public static class DependencyInjection
{

    /// <summary>
    /// Adds OpenTelemetry logging to the specified <see cref="WebApplicationBuilder"/> using the provided endpoint, service name, and environment name.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <param name="otelEndpoint">The OpenTelemetry collector endpoint.</param>
    /// <param name="serviceName">The name of the service for telemetry identification.</param>
    /// <param name="environmentName">The environment name (e.g., Development, Production).</param>
    /// <returns>The configured <see cref="WebApplicationBuilder"/> instance.</returns>
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder, string otelEndpoint, string serviceName, string environmentName)
    {
        var resourceBuilder = ServiceCollectionExtensions.CreateServiceResourceBuilder(serviceName, environmentName);
        // Logging
        builder.Logging.AddOpenTelemetryLogging(otelEndpoint, resourceBuilder);
        return builder;
    }
    /// <summary>
    /// Adds and configures the database context of type <typeparamref name="Tdbc"/> to the service collection using the specified configuration.
    /// </summary>
    /// <typeparam name="Tdbc">The type of the <see cref="DbContext"/> to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the database context to.</param>
    /// <param name="configuration">The application configuration used for database setup.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddDatabaseConfig<Tdbc>(this IServiceCollection services, IConfiguration configuration) where Tdbc : DbContext
    {
        services.AddDatabase<Tdbc>(configuration);
        return services;
    }

    /// <summary>
    /// Adds and configures repository services of type <typeparamref name="TRepo"/> with implementation <typeparamref name="TRepoImpl"/> to the service collection.
    /// </summary>
    /// <typeparam name="TRepo">The interface or base type of the repository.</typeparam>
    /// <typeparam name="TRepoImpl">The concrete implementation type of the repository.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the repositories to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddRepositoriesConfig<TRepo, TRepoImpl>(this IServiceCollection services)
        where TRepo : class
        where TRepoImpl : class, TRepo
    {
        services.AddRepos<TRepo, TRepoImpl>();
        return services;
    }

    /// <summary>
    /// Applies any pending migrations for the database context of type <typeparamref name="TDbc"/> at application startup.
    /// </summary>
    /// <typeparam name="TDbc">The type of the <see cref="DbContext"/> to migrate.</typeparam>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to use for migration.</param>
    /// <returns>The configured <see cref="IApplicationBuilder"/> instance.</returns>
    public static IApplicationBuilder MigrateDatabaseConfig<TDbc>(this IApplicationBuilder app) where TDbc : DbContext
    {
        app.MigrateServiceDatabase<TDbc>();
        return app;
    }

    /// <summary>
    /// Adds and configures application services, including MediatR and validation, to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the application services to.</param>
    /// <param name="mediatorAssembly">The assembly containing MediatR handlers.</param>
    /// <param name="validationAssembly">The assembly containing validation logic.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddAplicationConfig(this IServiceCollection services,
        Assembly mediatorAssembly,
        Assembly validationAssembly)
    {
        services.AddApplicationServices(mediatorAssembly, validationAssembly);
        return services;
    }

    /// <summary>
    /// Adds and configures the presentation layer services, including authentication, authorization, observability, API documentation,
    /// exception handling, health checks, and API infrastructure for an ASP.NET Core application.
    /// </summary>
    /// <typeparam name="T">The type implementing <see cref="IExceptionProblemDetailsMapper"/> for mapping exceptions to problem details.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the presentation services to.</param>
    /// <param name="configuration">The application configuration used for authentication and other setup.</param>
    /// <param name="otelEndpoint">The OpenTelemetry collector endpoint for observability.</param>
    /// <param name="serviceName">The name of the service for telemetry and documentation.</param>
    /// <param name="environmentName">The environment name (e.g., Development, Production).</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddPresentation<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string otelEndpoint,
        string serviceName,
        string environmentName) where T : class, IExceptionProblemDetailsMapper
    {
        var resourceBuilder = ServiceCollectionExtensions.CreateServiceResourceBuilder(serviceName, environmentName);
        // Authentication & Authorization
        services
            .AddJwtAuthentication(configuration)
            .AddRoleBasedAuthorization();

        // Observability
        services.AddOpenTelemetryObservability(otelEndpoint, serviceName, resourceBuilder);

        // API Documentation
        services.AddOpenApiWithJwtAuth($"{serviceName}-Api");

        // Exception Handling
        services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
        services.AddSingleton<IExceptionProblemDetailsMapper, T>();

        // Health Checks
        services.AddHealthChecks();

        // API Infrastructure
        services.AddEndpointsApiExplorer();
        services.AddControllers();

        return services;
    }


}
