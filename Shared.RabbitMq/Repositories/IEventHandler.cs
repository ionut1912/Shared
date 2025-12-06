
namespace Shared.RabbitMq.Repositories
{
    public interface IEventHandler<in T> where T : IEvent
    {
        Task Handle(T @event);
    }
}
