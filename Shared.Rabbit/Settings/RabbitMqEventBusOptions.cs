namespace Shared.Rabbit.Settings;

public class RabbitMqEventBusOptions
{
    public List<ExchangeConfiguration> ExchangeConfigurations { get; set; } = new();
    public Func<string,string> ? RoutingKeyResolver { get; set; }
    public Func<string,string>? ExchangeResolver { get; set; }
}
