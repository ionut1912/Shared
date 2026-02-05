using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Mediator;
using System.Reflection;

namespace Shared.Application.Extensions;

/// <summary>
/// Provides methods to register application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Mediator, pipeline behaviors, and FluentValidation validators.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        Assembly mediatorAssembly,
        Assembly validatorAssembly,
        Assembly behaviorAssembly)
    {
        services.AddMediator(mediatorAssembly, behaviorAssembly);
        services.AddValidatorsFromAssembly(validatorAssembly);
        return services;
    }
}
