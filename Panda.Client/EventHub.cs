﻿using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Panda.Client
{
    [Export(typeof(IEventHub))]
    public sealed class EventHub : IEventHub
    {
        internal ISubject<IDomainEvent> DomainEventSubject { get; set; } = new Subject<IDomainEvent>();

        public void Broadcast(IDomainEvent @event)
        {
            DomainEventSubject.OnNext(@event);
        }

        public IObservable<TEvent> Get<TEvent>() where TEvent : IDomainEvent
        {
            return DomainEventSubject.OfType<TEvent>();
        }
    }
}