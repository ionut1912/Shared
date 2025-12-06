namespace Shared.Rabbit.Repositories
{
    public interface IEventHandler<in T> where T : IEvent
    {
        Task Handle(T @event);
    }
}
