using System;
using System.Threading;
using System.Threading.Tasks;

namespace Panda.AppLauncher
{
    public interface IRegisteredApplicationService
    {
        IObservable<RegisteredApplication> ApplicationRegisteredObservable { get; }
        IObservable<RegisteredApplication> ApplicationUnregisteredObservable { get; }

        void Add(RegisteredApplication registeredApplication);
        IObservable<RegisteredApplication> Get();
        void Remove(RegisteredApplication registeredApplication);
        void Save();
        Task Setup(CancellationToken cancellationToken);
    }
}