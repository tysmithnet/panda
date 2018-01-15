using System;
using Common.Logging;

namespace Panda.LogLauncher
{
    /// <summary>
    ///     Class LogMessage.
    /// </summary>
    /// <seealso cref="Panda.LogLauncher.ILogMessage" />
    internal class LogMessage : ILogMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogMessage" /> class.
        /// </summary>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="logTime">The log time.</param>
        public LogMessage(string loggerName, LogLevel level, string message, Exception exception, DateTime logTime)
        {
            LogName = loggerName;
            Level = level;
            Message = message;
            Exception = exception;
            LogTime = logTime;
        }

        /// <summary>
        ///     Gets the level.
        /// </summary>
        /// <value>The level.</value>
        public LogLevel Level { get; }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }

        /// <summary>
        ///     Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; }

        /// <summary>
        ///     Gets the name of the log.
        /// </summary>
        /// <value>The name of the log.</value>
        public string LogName { get; }

        /// <summary>
        ///     Gets the log time.
        /// </summary>
        /// <value>The log time.</value>
        public DateTime LogTime { get; }
    }
}