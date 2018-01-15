using System;
using Common.Logging;

namespace Panda.LogLauncher
{
    public interface ILogMessage
    {
        LogLevel Level { get; }
        string Message { get; }
        Exception Exception { get; }
        string LogName { get; }
        DateTime LogTime { get; }
    }
}