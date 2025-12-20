using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Shared.Application.Mediator
{
    /// <summary>
    /// Provides extension methods to register MediatR-style mediator services and their handlers in the DI container.
    /// </summary>
    public static class MediatorServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IMediator"/> implementation, request handlers, and pipeline behaviors
        /// from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assemblies">Assemblies to scan for request handlers and pipeline behaviors.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// This method automatically registers:
        /// <list type="bullet">
        /// <item><description>The <see cref="IMediator"/> implementation.</description></item>
        /// <item><description>All classes implementing <see cref="IRequestHandler{TRequest, TResponse}"/> from the provided assemblies.</description></item>
        /// <item><description>All classes implementing <see cref="IPipelineBehavior{TRequest, TResponse}"/> from the provided assemblies.</description></item>
        /// </list>
        /// All services are registered with scoped lifetime.
        /// </remarks>
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Register the mediator implementation
            services.AddScoped<IMediator, Mediator>();

            // Register all IRequestHandler<TRequest, TResponse> implementations
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

            // Register all IPipelineBehavior<TRequest, TResponse> implementations
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
}
