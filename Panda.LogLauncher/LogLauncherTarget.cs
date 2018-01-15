using System;
using System.Reactive.Subjects;
using NLog;
using NLog.Targets;
using LogLevel = Common.Logging.LogLevel;

namespace Panda.LogLauncher
{
    [Target("LogLauncher")]
    public sealed class LogLauncherTarget : TargetWithLayout
    {
        private static readonly Subject<ILogMessage> LogMessageSubject = new Subject<ILogMessage>();

        internal static IObservable<ILogMessage> LogMessages => LogMessageSubject;

        protected override void Write(LogEventInfo logEvent)
        {
            LogLevel level;
            if (logEvent.Level == NLog.LogLevel.Debug)
                level = LogLevel.Debug;
            else if (logEvent.Level == NLog.LogLevel.Error)
                level = LogLevel.Error;
            else if (logEvent.Level == NLog.LogLevel.Trace)
                level = LogLevel.Trace;
            else if (logEvent.Level == NLog.LogLevel.Info)
                level = LogLevel.Info;
            else if (logEvent.Level == NLog.LogLevel.Warn)
                level = LogLevel.Warn;
            else if (logEvent.Level == NLog.LogLevel.Fatal)
                level = LogLevel.Fatal;
            else if (logEvent.Level == NLog.LogLevel.Off)
                level = LogLevel.Off;
            else
                level = LogLevel.All;

            var logMessage = new LogMessage(logEvent.LoggerName, level, logEvent.FormattedMessage, logEvent.Exception,
                DateTime.Now);
            lock (LogMessageSubject)
            {
                LogMessageSubject.OnNext(logMessage);
            }
        }
    }
}