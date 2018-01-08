using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Panda.Client
{
    /// <inheritdoc />
    /// <summary>
    ///     Default implementation of IEventHub
    /// </summary>
    /// <seealso cref="T:Panda.Client.IEventHub" />
    [Export(typeof(IEventHub))]
    internal sealed class EventHub : IEventHub
    {
        /// <summary>
        ///     Gets or sets the domain event subject.
        /// </summary>
        /// <value>
        ///     The domain event subject.
        /// </value>
        internal ISubject<IDomainEvent> DomainEventSubject { get; set; } = new Subject<IDomainEvent>();

        /// <inheritdoc />
        /// <summary>
        ///     Broadcasts the specified event to all interested listeners
        /// </summary>
        /// <param name="event">The event.</param>
        public void Broadcast(IDomainEvent @event)
        {
            DomainEventSubject.OnNext(@event);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets all domain events of a certain type
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to listen for.</typeparam>
        /// <returns></returns>
        public IObservable<TEvent> Get<TEvent>() where TEvent : IDomainEvent
        {
            return DomainEventSubject.OfType<TEvent>();
        }
    }
}