using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Application.Mediator;

public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mediator and all handlers from the specified assemblies
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddScoped<IMediator, Mediator>();

        // Register handlers
        var handlerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }))
            .ToList();

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        // Register pipeline behaviors
        var behaviorTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                .Select(i => new { Interface = i, Implementation = t }))
            .ToList();

        foreach (var behavior in behaviorTypes)
        {
            services.AddScoped(behavior.Interface, behavior.Implementation);
        }

        return services;
    }
}