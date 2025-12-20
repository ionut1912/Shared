namespace Shared.Rabbit.Repositories;

/// <summary>
/// Defines a handler for a specific type of event.
/// </summary>
/// <typeparam name="T">The type of event this handler processes.</typeparam>
public interface IEventHandler<in T> where T : IEvent
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="event">The event instance to handle.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous handle operation.</returns>
    Task Handle(T @event);
}
