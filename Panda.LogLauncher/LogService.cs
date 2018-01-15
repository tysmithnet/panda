using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Panda.Client;

namespace Panda.LogLauncher
{
    [Export(typeof(ILogService))]
    internal sealed class LogService : ILogService
    {           
        public IObservable<ILogMessage> LogMessages => LogLauncherTarget.LogMessages;
    }
}