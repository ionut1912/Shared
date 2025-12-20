namespace Shared.Rabbit.Settings
{
    /// <summary>
    /// Represents configuration options for the RabbitMQ event bus.
    /// </summary>
    public class RabbitMqEventBusOptions
    {
        /// <summary>
        /// Gets or sets the list of exchanges to configure for the event bus.
        /// </summary>
        public List<ExchangeConfiguration> ExchangeConfigurations { get; set; } = new();

        /// <summary>
        /// Gets or sets a function to resolve the routing key for a given event name.
        /// If not set, the default routing key is derived from the event name.
        /// </summary>
        public Func<string, string>? RoutingKeyResolver { get; set; }

        /// <summary>
        /// Gets or sets a function to resolve the exchange name for a given event name.
        /// If not set, the first exchange in <see cref="ExchangeConfigurations"/> is used.
        /// </summary>
        public Func<string, string>? ExchangeResolver { get; set; }
    }
}
