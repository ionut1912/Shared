using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Shared.Api.Infrastructure;

/// <summary>
/// Provides extension methods for configuring endpoint routes with conventional patterns and automatic discovery.
/// </summary>
public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps an endpoint group to a route with automatic naming conventions.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="group">The endpoint group to map.</param>
    /// <returns>A <see cref="RouteGroupBuilder"/> that can be used to further configure the group.</returns>
    /// <remarks>
    /// The route pattern is derived from the group type name by converting PascalCase to kebab-case
    /// and removing the "EndpointGroup" suffix. Tags are automatically generated from the type name.
    /// </remarks>
    public static RouteGroupBuilder MapGroup(this IEndpointRouteBuilder endpoints, EndpointGroup group) => endpoints
        .MapGroup($"/{group.GetType().Name.Trim().PascalToKebabCase().Replace("-endpoint-group", "")}")
        .WithTags(group.GetType().Name.Replace("EndpointGroup", "").PascalToWords());

    /// <summary>
    /// Maps a GET endpoint with automatic naming based on the handler method name.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="handler">The delegate that handles the GET request.</param>
    /// <param name="pattern">The route pattern. Defaults to an empty string.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        builder.MapGet(pattern, handler)
            .WithName(handler.Method.Name);
        return builder;
    }

    /// <summary>
    /// Maps a POST endpoint with automatic naming based on the handler method name.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="handler">The delegate that handles the POST request.</param>
    /// <param name="pattern">The route pattern. Defaults to an empty string.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        builder.MapPost(pattern, handler)
            .WithName(handler.Method.Name);
        return builder;
    }

    /// <summary>
    /// Maps a PUT endpoint with automatic naming based on the handler method name.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="handler">The delegate that handles the PUT request.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
    {
        builder.MapPut(pattern, handler)
            .WithName(handler.Method.Name);
        return builder;
    }

    /// <summary>
    /// Maps a DELETE endpoint with automatic naming based on the handler method name.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <param name="handler">The delegate that handles the DELETE request.</param>
    /// <param name="pattern">The route pattern.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    public static IEndpointRouteBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
    {
        builder.MapDelete(pattern, handler)
            .WithName(handler.Method.Name);
        return builder;
    }

    /// <summary>
    /// Automatically discovers and maps all endpoint groups in the specified assembly.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="assembly">
    /// The assembly to scan for endpoint groups. If null, scans the calling assembly.
    /// </param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> for chaining.</returns>
    /// <remarks>
    /// This method scans the specified assembly (or the calling assembly if not provided) for all types 
    /// that inherit from <see cref="EndpointGroup"/>, creates instances of them, and invokes their Map 
    /// method to register endpoints.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Scan the calling assembly
    /// app.MapEndpoints();
    /// 
    /// // Scan a specific assembly
    /// app.MapEndpoints(typeof(UserEndpointGroup).Assembly);
    /// </code>
    /// </example>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints, Assembly? assembly = null)
    {
        var endpointGroupType = typeof(EndpointGroup);
        var targetAssembly = assembly ?? Assembly.GetCallingAssembly();

        var endpointGroupTypes = targetAssembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroup instance)
            {
                instance.Map(endpoints);
            }
        }

        return endpoints;
    }

    internal static string PascalToKebabCase(this string value) => !string.IsNullOrEmpty(value)
        ? PascalToKebabRegex().Replace(value, "-$1").ToLowerInvariant()
        : value;

    internal static string PascalToWords(this string value) => !string.IsNullOrEmpty(value)
        ? PascalToWordsRegex().Replace(value, " $1").Trim()
        : value;

    [GeneratedRegex(@"(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])")]
    private static partial Regex PascalToKebabRegex();

    [GeneratedRegex(@"(?<!^)([A-Z])")]
    private static partial Regex PascalToWordsRegex();
}