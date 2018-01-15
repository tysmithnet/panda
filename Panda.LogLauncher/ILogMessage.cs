using System;
using Common.Logging;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Interface ILogMessage
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        ///     Gets the level.
        /// </summary>
        /// <value>The level.</value>
        LogLevel Level { get; }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>The message.</value>
        string Message { get; }

        /// <summary>
        ///     Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        Exception Exception { get; }

        /// <summary>
        ///     Gets the name of the log.
        /// </summary>
        /// <value>The name of the log.</value>
        string LogName { get; }

        /// <summary>
        ///     Gets the log time.
        /// </summary>
        /// <value>The log time.</value>
        DateTime LogTime { get; }
    }
}