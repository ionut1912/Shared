namespace Shared.Rabbit.Repositories
{
    /// <summary>
    /// Defines the contract for an event bus that supports publishing and subscribing to events.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes the specified event asynchronously to all subscribers.
        /// </summary>
        /// <typeparam name="T">The type of the event being published.</typeparam>
        /// <param name="event">The event instance to publish.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous publish operation.</returns>
        Task PublishAsync<T>(T @event) where T : IEvent;

        /// <summary>
        /// Subscribes a handler to a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event to subscribe to.</typeparam>
        /// <typeparam name="TH">The type of the event handler that will handle the event.</typeparam>
        void Subscribe<T, TH>() where T : IEvent where TH : class, IEventHandler<T>;

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        /// <typeparam name="T">The type of the event to unsubscribe from.</typeparam>
        /// <typeparam name="TH">The type of the event handler that will be removed.</typeparam>
        void Unsubscribe<T, TH>() where T : IEvent where TH : class, IEventHandler<T>;
    }
}
