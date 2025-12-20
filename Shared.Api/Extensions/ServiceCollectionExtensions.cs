using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Infra.Settings;
using System.Text;

namespace Shared.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring dependency injection and OpenTelemetry services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT authentication to the service collection.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var key = jwtSettings["Key"];
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("JWT Key is missing in configuration.");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
            };
        });

        return services;
    }

    /// <summary>
    /// Adds role-based authorization policies to the service collection.
    /// </summary>
    public static IServiceCollection AddRoleBasedAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("FreelancerOnly", policy => policy.RequireRole("Freelancer"));
            options.AddPolicy("ClientOnly", policy => policy.RequireRole("Client"));
        });

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry tracing and metrics to the service collection.
    /// </summary>
    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, string otelEndpoint, string serviceName, ResourceBuilder resourceBuilder)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(rb => rb.AddService(serviceName: serviceName, serviceVersion: "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(opts => opts.EnrichWithIDbCommand = (activity, command) => activity.SetTag("db.statement", command.CommandText))
                    .AddSource(serviceName)
                    .AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddEventCountersInstrumentation(c => c.AddEventSources("Microsoft.AspNetCore.Hosting"))
                    .AddOtlpExporter(o => o.Endpoint = new Uri(otelEndpoint));
            });

        return services;
    }

    /// <summary>
    /// Adds OpenTelemetry logging to the logging builder.
    /// </summary>
    public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder logging, string otelEndpoint, ResourceBuilder resourceBuilder)
    {
        logging.ClearProviders();
        logging.AddConsole();

        logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options.AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(otelEndpoint));
        });

        return logging;
    }

    /// <summary>
    /// Adds OpenAPI/Swagger documentation with JWT authentication to the service collection.
    /// </summary>
    public static IServiceCollection AddOpenApiWithJwtAuth(this IServiceCollection services, string title, string version = "v1")
    {
        services.AddOpenApi(version, options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = title;
                document.Info.Version = version;

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                };

                document.SecurityRequirements = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                            Array.Empty<string>()
                        }
                    }
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Creates a <see cref="ResourceBuilder"/> for OpenTelemetry.
    /// </summary>
    public static ResourceBuilder CreateServiceResourceBuilder(string serviceName, string environmentName, string serviceVersion = "1.0.0")
    {
        return ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new[]
            {
                new KeyValuePair<string, object>("deployment.environment", environmentName),
                new KeyValuePair<string, object>("service.namespace", "freelance")
            });
    }
}
