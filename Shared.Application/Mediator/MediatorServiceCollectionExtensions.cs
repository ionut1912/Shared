using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Behaviours;
using System.Reflection;

namespace Shared.Application.Mediator;

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

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        foreach (var item in (from t in assemblies.SelectMany((Assembly a) => a.GetTypes())
                              where t.IsClass && !t.IsAbstract
                              select t).SelectMany((Type t) => from i in t.GetInterfaces()
                                                               where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                                                               select new
                                                               {
                                                                   Interface = i,
                                                                   Implementation = t
                                                               }).ToList())
        {
            services.AddScoped(item.Interface, item.Implementation);
        }

        foreach (var item2 in (from t in assemblies.SelectMany((Assembly a) => a.GetTypes())
                               where t.IsClass && !t.IsAbstract && t != typeof(ValidationBehavior<,>)
                               select t).SelectMany((Type t) => from i in t.GetInterfaces()
                                                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)
                                                                select new
                                                                {
                                                                    Interface = i,
                                                                    Implementation = t
                                                                }).ToList())
        {
            services.AddScoped(item2.Interface, item2.Implementation);
        }

        return services;
    }
}
