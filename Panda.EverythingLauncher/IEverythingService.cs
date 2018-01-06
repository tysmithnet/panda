using System;
using System.Threading;

namespace Panda.EverythingLauncher
{
    public interface IEverythingService
    {
        IObservable<EverythingResult> Search(string query, CancellationToken cancellationToken);
    }
}