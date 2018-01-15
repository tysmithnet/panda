using System;
using Common.Logging;

namespace Panda.LogLauncher
{
    internal class LogMessage : ILogMessage
    {
        public LogMessage(string loggerName, LogLevel level, string message, Exception exception, DateTime logTime)
        {
            LogName = loggerName;
            Level = level;
            Message = message;
            Exception = exception;
            LogTime = logTime;
        }

        public LogLevel Level { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public string LogName { get; }
        public DateTime LogTime { get; }
    }
}