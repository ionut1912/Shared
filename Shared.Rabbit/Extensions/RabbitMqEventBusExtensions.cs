using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Shared.Rabbit.Repositories;
using Shared.Rabbit.Services;
using Shared.Rabbit.Settings;

namespace Shared.Rabbit.Extensions;

/// <summary>
/// Provides extension methods to register RabbitMQ event bus services in the dependency injection container.
/// </summary>
public static class RabbitMqEventBusExtensions
{
    /// <summary>
    /// Registers the RabbitMQ event bus, its configuration options, and a RabbitMQ connection
    /// with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register the event bus with.</param>
    /// <param name="configureOptions">
    /// An action used to configure the <see cref="RabbitMqEventBusOptions"/> instance.
    /// </param>
    /// <param name="hostname">The RabbitMQ host name used to establish the connection.</param>
    /// <param name="username">The username used to authenticate with RabbitMQ.</param>
    /// <param name="password">The password used to authenticate with RabbitMQ.</param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> for chaining.
    /// </returns>
    /// <remarks>
    /// This method performs the following registrations:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// A singleton instance of <see cref="RabbitMqEventBusOptions"/> configured by
    /// <paramref name="configureOptions"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A singleton <see cref="IConnection"/> created using the provided RabbitMQ
    /// <paramref name="hostname"/>, <paramref name="username"/>, and <paramref name="password"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A singleton implementation of <see cref="IEventBus"/> using <see cref="RabbitMqEventBus"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddRabbitMqEventBus(
        this IServiceCollection services,
        Action<RabbitMqEventBusOptions> configureOptions,
        string hostname,
        string username,
        string password)
    {
        var options = new RabbitMqEventBusOptions();
        configureOptions(options);

        services.AddSingleton(options);

        services.AddSingleton<IConnection>(_ =>
        {
            var factory = new ConnectionFactory
            {
                HostName = hostname,
                UserName = username,
                Password = password
            };

            return factory.CreateConnection();
        });

        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        return services;
    }
}
