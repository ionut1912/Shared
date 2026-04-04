using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Application.Extensions;

/// <summary>
///     Provides methods to register application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Registers Mediator, pipeline behaviors, and FluentValidation validators.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        Assembly validatorAssembly)
    {
        services.AddValidatorsFromAssembly(validatorAssembly);

        return services;
    }
}