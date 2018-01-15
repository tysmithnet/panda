using System;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Interface ILogService
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        ///     Gets the log messages.
        /// </summary>
        /// <value>The log messages.</value>
        IObservable<ILogMessage> LogMessages { get; }
    }
}