namespace Shared.Rabbit.Repositories;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
