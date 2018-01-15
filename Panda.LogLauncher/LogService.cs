using System;
using System.ComponentModel.Composition;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Class LogService. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="Panda.LogLauncher.ILogService" />
    [Export(typeof(ILogService))]
    internal sealed class LogService : ILogService
    {
        /// <summary>
        ///     Gets the log messages.
        /// </summary>
        /// <value>The log messages.</value>
        public IObservable<ILogMessage> LogMessages => LogLauncherTarget.LogMessages;
    }
}