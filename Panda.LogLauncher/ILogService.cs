using System;

namespace Panda.LogLauncher
{
    public interface ILogService
    {
        IObservable<ILogMessage> LogMessages { get; }
    }
}