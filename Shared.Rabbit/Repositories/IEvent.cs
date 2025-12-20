namespace Shared.Rabbit.Repositories
{
    /// <summary>
    /// Represents a domain or integration event in the system.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Gets the unique identifier of the event.
        /// </summary>
        Guid EventId { get; }

        /// <summary>
        /// Gets the UTC timestamp indicating when the event occurred.
        /// </summary>
        DateTime OccurredAt { get; }
    }
}
