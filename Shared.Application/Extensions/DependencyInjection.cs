using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Mediator;
using System.Reflection;

namespace Shared.Application.Extensions
{
    /// <summary>
    /// Provides extension methods for registering application services with the dependency injection container.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Registers MediatR handlers and FluentValidation validators from the specified assemblies.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="mediatorAssembly">The assembly containing MediatR request handlers.</param>
        /// <param name="validationAssembly">The assembly containing FluentValidation validators.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            Assembly mediatorAssembly,
            Assembly validationAssembly)
        {
            // Register MediatR request handlers from the specified assembly
            services.AddMediator(mediatorAssembly);

            // Register all FluentValidation validators from the specified assembly
            services.AddValidatorsFromAssembly(validationAssembly);

            return services;
        }
    }
}
