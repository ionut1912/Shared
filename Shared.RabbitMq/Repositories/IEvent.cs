namespace Shared.RabbitMq.Repositories;

public interface IEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
