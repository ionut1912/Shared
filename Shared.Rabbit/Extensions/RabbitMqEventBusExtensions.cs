using Microsoft.Extensions.DependencyInjection;
using Shared.Rabbit.Repositories;
using Shared.Rabbit.Services;
using Shared.Rabbit.Settings;

namespace Shared.Rabbit.Extensions
{
    /// <summary>
    /// Provides extension methods to register RabbitMQ event bus services in the dependency injection container.
    /// </summary>
    public static class RabbitMqEventBusExtensions
    {
        /// <summary>
        /// Registers the RabbitMQ event bus and its options with the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to register the event bus with.</param>
        /// <param name="configureOptions">An action to configure the <see cref="RabbitMqEventBusOptions"/>.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
        /// <remarks>
        /// This method registers:
        /// <list type="bullet">
        /// <item><description>A singleton instance of <see cref="RabbitMqEventBusOptions"/> configured by <paramref name="configureOptions"/>.</description></item>
        /// <item><description>A singleton implementation of <see cref="IEventBus"/> using <see cref="RabbitMqEventBus"/>.</description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, Action<RabbitMqEventBusOptions> configureOptions)
        {
            var options = new RabbitMqEventBusOptions();
            configureOptions(options);

            services.AddSingleton(options);
            services.AddSingleton<IEventBus, RabbitMqEventBus>();

            return services;
        }
    }
}
