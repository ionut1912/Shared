using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using System.Diagnostics;

namespace Shared.Api.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring the application's middleware pipeline and endpoints.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds a global exception handler middleware to the application's request pipeline.
        /// </summary>
        /// <typeparam name="T">The type used for logging context.</typeparam>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
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
                    var logger = context.RequestServices.GetRequiredService<ILogger<T>>();
                    logger.LogError(ex, "Unhandled exception caught in global middleware");

                    var exceptionHandler = context.RequestServices.GetRequiredService<IExceptionHandler>();
                    await exceptionHandler.TryHandleAsync(context, ex, CancellationToken.None);
                }
            });

            return app;
        }

        /// <summary>
        /// Adds middleware to log the duration of each HTTP request, including route and status code.
        /// </summary>
        /// <typeparam name="T">The type used for logging context.</typeparam>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
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
                        var routePattern = endpoint.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName ?? endpoint.DisplayName;

                        if (!string.IsNullOrEmpty(routePattern))
                        {
                            Activity.Current?.SetTag("http.route", routePattern);
                        }
                    }

                    var logger = context.RequestServices.GetRequiredService<ILogger<T>>();
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

        /// <summary>
        /// Adds standard middleware components to the application's request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseStandardMiddleware(this IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }

        /// <summary>
        /// Maps API documentation endpoints to the application's endpoint routing pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="configureScalar">An optional action to configure Scalar API reference UI.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder MapApiDocumentation(this IApplicationBuilder app, Action<ScalarOptions>? configureScalar = null)
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
                    options.WithTitle("API Documentation").WithTheme(ScalarTheme.Default);
                });
            }

            return app;
        }

        /// <summary>
        /// Maps standard endpoints such as health checks and metrics to the application's endpoint routing pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder MapStandardEndpoints(this IApplicationBuilder app)
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
}
