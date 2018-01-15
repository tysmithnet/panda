using System;
using System.ComponentModel.Composition;

namespace Panda.LogLauncher
{
    [Export(typeof(ILogService))]
    internal sealed class LogService : ILogService
    {
        public IObservable<ILogMessage> LogMessages => LogLauncherTarget.LogMessages;
    }
}