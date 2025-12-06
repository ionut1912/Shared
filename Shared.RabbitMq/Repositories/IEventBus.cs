using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RabbitMq.Repositories
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T @event) where T : IEvent;
        void Subscribe<T, TH>() where T : IEvent where TH : class, IEventHandler<T>;
        void Unsubscribe<T, TH>() where T : IEvent where TH : class, IEventHandler<T>;
    }
}
