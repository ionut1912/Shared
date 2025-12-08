using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using Shared.Api.Abstractions;
using Shared.Api.Handlers;
using Shared.Application.Extensions;
using Shared.Infra.Extensions;
using System.Reflection;

namespace Shared.Api.Extensions;

public static class DependencyInjection
{

    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder,string otelEndpoint,string serviceName,string environmentName)
    {
        var resourceBuilder = ServiceCollectionExtensions.CreateServiceResourceBuilder(serviceName,environmentName);
        // Logging
        builder.Logging.AddOpenTelemetryLogging(otelEndpoint, resourceBuilder);
        return builder;
    }
    public static IServiceCollection AddDatabaseConfig<Tdbc>(this IServiceCollection services, IConfiguration configuration)where Tdbc : DbContext
    {
        services.AddDatabase<Tdbc>(configuration);
        return services;    
    }

    public static IServiceCollection AddRepositoriesConfig<TRepo, TRepoImpl>(this IServiceCollection services)
        where TRepo : class
        where TRepoImpl : class, TRepo
    {
        services.AddRepos<TRepo, TRepoImpl>();
        return services;
    }

    public static IApplicationBuilder MigrateDatabaseConfig<TDbc>(this IApplicationBuilder app) where TDbc : DbContext
    {
        app.MigrateServiceDatabase<TDbc>();
        return app;
    }

    public static IServiceCollection AddAplicationConfig(this IServiceCollection services,
        Assembly mediatorAssembly,
        Assembly validationAssembly)
    {
        services.AddApplicationServices(mediatorAssembly, validationAssembly);
        return services;
    }

    public static IServiceCollection AddPresentation<T>(
    this IServiceCollection services,
    IConfiguration configuration,
    string otelEndpoint,
    string serviceName,
    string environmentName,
    string openApiTelemetryTitle,
    string openApiDescription) where T : class, IExceptionProblemDetailsMapper
    {
        var resourceBuilder = ServiceCollectionExtensions.CreateServiceResourceBuilder(serviceName, environmentName);
        // Authentication & Authorization
        services
            .AddJwtAuthentication(configuration)
            .AddRoleBasedAuthorization();

        // Observability
        services.AddOpenTelemetryObservability(otelEndpoint,openApiTelemetryTitle, resourceBuilder);

        // API Documentation
        services.AddOpenApiWithJwtAuth(openApiDescription);

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
