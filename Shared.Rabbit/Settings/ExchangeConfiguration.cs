using RabbitMQ.Client;

namespace Shared.Rabbit.Settings;

public class ExchangeConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = ExchangeType.Topic;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
}
