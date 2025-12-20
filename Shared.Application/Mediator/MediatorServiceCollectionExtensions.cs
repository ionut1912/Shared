using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Application.Mediator;

/// <summary>
/// Extension methods to register mediator services.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mediator, request handlers, and pipeline behaviors.
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }))
            .ToList();

        foreach (var handler in handlerTypes)
            services.AddScoped(handler.Interface, handler.Implementation);

        var behaviorTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                .Select(i => new { Interface = i, Implementation = t }))
            .ToList();

        foreach (var behavior in behaviorTypes)
            services.AddScoped(behavior.Interface, behavior.Implementation);

        return services;
    }
}
