using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;

namespace Shared.Api.Endpoints;

/// <summary>
///     Provides extension methods for registering health check endpoints.
/// </summary>
public static class HealthCheckEndpoints
{
    /// <summary>
    ///     Maps standard health, readiness, and liveness endpoints to the routing pipeline.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder" /> to add the routes to.</param>
    /// <returns>The modified <see cref="IEndpointRouteBuilder" />.</returns>
    /// <remarks>
    ///     This method registers three endpoints:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><c>/healthz</c>: Detailed health report using <see cref="UIResponseWriter" />.</description>
    ///         </item>
    ///         <item>
    ///             <description><c>/ready</c>: Reports status only for checks tagged with "ready".</description>
    ///         </item>
    ///         <item>
    ///             <description><c>/live</c>: A basic liveness probe that returns 200 OK without running sub-checks.</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Detailed health report (often used by UIs or developers)
        endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Readiness probe: Checks if the application is ready to serve traffic (e.g., DB connected)
        endpoints.MapHealthChecks("/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        });

        // Liveness probe: Checks if the process is alive (returns 200 immediately)
        endpoints.MapHealthChecks("/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        return endpoints;
    }
}