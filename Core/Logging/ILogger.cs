using System;

namespace TB_Browser.Core.Logging
{
    public interface ILogger
    {
        void Log(LogLevel level, string source, string message, Exception? ex = null);
        void Debug(string source, string message) => Log(LogLevel.Debug, source, message);
        void Info(string source, string message) => Log(LogLevel.Info, source, message);
        void Warning(string source, string message) => Log(LogLevel.Warning, source, message);
        void Error(string source, string message, Exception? ex = null) => Log(LogLevel.Error, source, message, ex);
        void Critical(string source, string message, Exception? ex = null) => Log(LogLevel.Critical, source, message, ex);
    }
}
