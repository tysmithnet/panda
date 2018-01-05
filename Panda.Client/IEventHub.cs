using System;

namespace Panda.Client
{
    public interface IEventHub
    {
        void Broadcast(IDomainEvent @event);
        IObservable<TEvent> Get<TEvent>() where TEvent : IDomainEvent;
    }
}