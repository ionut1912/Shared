using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared.Api.Extensions;

/// <summary>
/// Provides extension methods to configure OpenTelemetry tracing, metrics, and logging for ASP.NET Core applications.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Creates a <see cref="ResourceBuilder"/> pre-populated with service metadata and environment attributes.
    /// </summary>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="environmentName">The environment name (e.g., Development, Production).</param>
    /// <returns>A configured <see cref="ResourceBuilder"/> with service and environment attributes.</returns>
    public static ResourceBuilder CreateServiceResourceBuilder(string serviceName, string environmentName)
    {
        return ResourceBuilder.CreateDefault().AddService(serviceName, null, "1.0.0").AddAttributes(new Dictionary<string, object>
        {
            { "environment", environmentName },
            { "service.namespace", "Freelance" },
            { "telemetry.sdk.language", "dotnet" }
        });
    }

    /// <summary>
    /// Configures OpenTelemetry logging to send logs to a Loki endpoint using OTLP.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to extend.</param>
    /// <param name="lokiEndpoint">The Loki HTTP endpoint (e.g., http://loki:3100).</param>
    /// <param name="resourceBuilder">The <see cref="ResourceBuilder"/> defining service and environment attributes.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> for chaining.</returns>
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder, string lokiEndpoint, ResourceBuilder resourceBuilder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.IncludeFormattedMessage = true;
            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri($"{lokiEndpoint.TrimEnd('/')}/otlp");
                otlpOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry tracing and metrics for ASP.NET Core applications.
    /// Includes HTTP, HttpClient, and Entity Framework Core instrumentation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
    /// <param name="otelEndpoint">The OTLP gRPC endpoint for traces and metrics.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        string otelEndpoint,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation(opts =>
                {
                    opts.RecordException = true;
                    opts.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.client_ip", request.HttpContext?.Connection.RemoteIpAddress?.ToString());
                    };
                    opts.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response_content_length", response.ContentLength);
                    };
                })
                .AddHttpClientInstrumentation(opts => opts.RecordException = true)
                .AddEntityFrameworkCoreInstrumentation(opts =>
                {
                    opts.EnrichWithIDbCommand = (activity, command) =>
                    {
                        activity.SetTag("db.statement", command.CommandText);
                        activity.SetTag("db.command_type", command.CommandType.ToString());
                        activity.SetTag("db.system", "postgresql");
                    };
                })
                .AddSource(serviceName)
                .AddOtlpExporter(otlpOptions =>
                {
                    otlpOptions.Endpoint = new Uri(otelEndpoint);
                    otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                    otlpOptions.TimeoutMilliseconds = 10000;
                });
            })
            .WithMetrics(metrics =>
            {
                metrics.AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddEventCountersInstrumentation(opts =>
                    {
                        opts.AddEventSources(
                            "Microsoft.AspNetCore.Hosting",
                            "System.Net.Http",
                            "System.Net.NameResolution",
                            "System.Net.Security");
                    })
                    .AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otelEndpoint);
                        otlpOptions.Protocol = OtlpExportProtocol.Grpc;
                        otlpOptions.TimeoutMilliseconds = 10000;
                    });
            });

        return services;
    }
}
