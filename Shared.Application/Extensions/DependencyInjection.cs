using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Mediator;
using System.Reflection;

namespace Shared.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,Assembly mediatorAssembly,Assembly validationAssembly)
    {
        services.AddMediator(mediatorAssembly);
        services.AddValidatorsFromAssembly(validationAssembly);
        return services;
    }
}
