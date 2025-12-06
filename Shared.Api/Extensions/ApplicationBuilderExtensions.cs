using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;

namespace Shared.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler<T>(
        this IApplicationBuilder app) where T : class
    {
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices
                    .GetRequiredService<ILogger<T>>();
                logger.LogError(ex, "Unhandled exception caught in global middleware");

                var exceptionHandler = context.RequestServices
                    .GetRequiredService<IExceptionHandler>();
                await exceptionHandler.TryHandleAsync(
                    context, ex, CancellationToken.None);
            }
        });

        return app;
    }

    public static IApplicationBuilder UseRequestDurationLogging<T>(
        this IApplicationBuilder app) where T : class
    {
        app.Use(async (context, next) =>
        {
            var sw = Stopwatch.StartNew();
            if (Activity.Current == null)
            {
                var activity = new Activity("http.server");
                activity.Start();
            }

            try
            {
                await next();
            }
            finally
            {
                sw.Stop();
                Activity.Current?.SetTag("http.request_duration_ms", sw.ElapsedMilliseconds);

                if (context.GetEndpoint() is Endpoint endpoint)
                {
                    var routePattern = endpoint.Metadata
                        .GetMetadata<RouteNameMetadata>()?.RouteName
                        ?? endpoint.DisplayName;

                    if (!string.IsNullOrEmpty(routePattern))
                    {
                        Activity.Current?.SetTag("http.route", routePattern);
                    }
                }

                var logger = context.RequestServices
                    .GetRequiredService<ILogger<T>>();
                logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response?.StatusCode,
                    sw.ElapsedMilliseconds);
            }
        });

        return app;
    }

    public static IApplicationBuilder UseStandardMiddleware(
        this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    public static IApplicationBuilder MapApiDocumentation(
        this IApplicationBuilder app,
        Action<ScalarOptions>? configureScalar = null)
    {
        var routeBuilder = (IEndpointRouteBuilder)app;

        routeBuilder.MapOpenApi();

        if (configureScalar != null)
        {
            routeBuilder.MapScalarApiReference(configureScalar);
        }
        else
        {
            routeBuilder.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("API Documentation")
                    .WithTheme(ScalarTheme.Default);
            });
        }

        return app;
    }

    public static IApplicationBuilder MapStandardEndpoints(
        this IApplicationBuilder app)
    {
        var routeBuilder = (IEndpointRouteBuilder)app;

        routeBuilder.MapHealthChecks("/health");
        routeBuilder.MapGet("/metrics", async context =>
        {
            context.Response.StatusCode = 204;
            await context.Response.CompleteAsync();
        });

        return app;
    }
}