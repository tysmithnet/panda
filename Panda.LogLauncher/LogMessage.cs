using System;
using Common.Logging;

namespace Panda.LogLauncher
{
    internal class LogMessage : ILogMessage
    {
        public LogLevel Level { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public string LogName { get; private set; }

        public LogMessage(string loggerName, LogLevel level, string message, Exception exception)
        {
            LogName = loggerName;
            Level = level;
            Message = message;
            Exception = exception;
        }
    }
}