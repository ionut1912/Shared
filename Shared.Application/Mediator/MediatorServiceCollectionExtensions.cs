using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviours;
using System.Reflection;

namespace Shared.Application.Mediator
{
    /// <summary>
    /// Provides extension methods for registering MediatR-style mediator services and pipeline behaviors.
    /// </summary>
    public static class MediatorServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IMediator"/> implementation, request handlers, and pipeline behaviors
        /// from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assemblies">Assemblies to scan for request handlers and pipeline behaviors.</param>
        /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
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

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
