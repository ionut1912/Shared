using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Api.Abstractions;
using Shared.Api.Handlers;

namespace Shared.Api.Extensions;

/// <summary>
///     Provides extension methods for configuring dependency injection, database, repositories, application, and
///     presentation layers.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Adds and configures the presentation layer services.
    /// </summary>
    public static IServiceCollection AddPresentation<T>(this IServiceCollection services, IConfiguration configuration,
        string serviceName, string? otelEndpoint = null, bool needsRoles = false, List<string>? requiredPolicies = null,
        List<string>? requiredRoles = null)
        where T : class, IExceptionProblemDetailsMapper
    {
        services.AddJwtAuthentication(configuration);

        if (needsRoles)
        {
            if (requiredPolicies is null || requiredRoles is null)
                throw new ArgumentNullException(
                    "If 'needsRoles' is true, you must provide both 'requiredPolicies' and 'requiredRoles'."
                );

            if (requiredPolicies.Count != requiredPolicies.Count)
                throw new ArgumentException("The number of policies must match the number of roles.");

            services.AddRoleBasedAuthorization(requiredPolicies, requiredRoles);
        }

        if (!string.IsNullOrWhiteSpace(otelEndpoint)) services.AddOpenTelemetryObservability(otelEndpoint, serviceName);

        services.AddOpenApiWithJwtAuth(serviceName + "-Api");
        services.AddSingleton<IExceptionHandler, GlobalExceptionHandler>();
        services.AddSingleton<IExceptionProblemDetailsMapper, T>();
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("DefaultConnection")!,
                name: "postgres",
                tags: ["ready"]);
        ;
        services.AddEndpointsApiExplorer();
        services.AddControllers();
        return services;
    }
}