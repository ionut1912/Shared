using RabbitMQ.Client;

namespace Shared.Rabbit.Settings
{
    /// <summary>
    /// Represents the configuration of a RabbitMQ exchange.
    /// </summary>
    public class ExchangeConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the exchange.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the exchange (e.g., <see cref="ExchangeType.Topic"/>).
        /// </summary>
        public string Type { get; set; } = ExchangeType.Topic;

        /// <summary>
        /// Gets or sets a value indicating whether the exchange should survive broker restarts.
        /// </summary>
        public bool Durable { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the exchange should be automatically deleted when no queues are bound to it.
        /// </summary>
        public bool AutoDelete { get; set; } = false;
    }
}
