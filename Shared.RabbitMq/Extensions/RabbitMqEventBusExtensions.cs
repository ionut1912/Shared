using Microsoft.Extensions.DependencyInjection;
using Shared.RabbitMq.Repositories;
using Shared.RabbitMq.Services;
using Shared.RabbitMq.Settings;

namespace Shared.RabbitMq.Extensions
{
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
}
