using System;

namespace Panda.Client
{
    /// <summary>
    /// Represents something that is capable of routing domain events between interested parties
    /// </summary>
    public interface IEventHub
    {
        /// <summary>
        /// Broadcasts the specified event to all interested listeners
        /// </summary>
        /// <param name="event">The event.</param>
        void Broadcast(IDomainEvent @event);

        /// <summary>
        /// Gets all domain events of a certain type
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to listen for.</typeparam>
        /// <returns></returns>
        IObservable<TEvent> Get<TEvent>() where TEvent : IDomainEvent;
    }
}