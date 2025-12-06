using Microsoft.Extensions.DependencyInjection;
using Shared.Rabbit.Repositories;
using Shared.Rabbit.Services;
using Shared.Rabbit.Settings;

namespace Shared.Rabbit.Extensions;

public static class RabbitMqEventBusExtensions
{
    public static IServiceCollection AddRabbitMqEventBus(this IServiceCollection services, Action<RabbitMqEventBusOptions> configureOptions)
    {
        var options = new RabbitMqEventBusOptions();
        configureOptions(options);
        services.AddSingleton(options);
        services.AddSingleton<IEventBus, RabbitMqEventBus>();
        return services;
    }
}
