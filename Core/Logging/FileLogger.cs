using System;
using System.IO;
using System.Text;

namespace TB_Browser.Core.Logging
{
    public static class Logger
    {
        private static readonly string _logPath;
        private static readonly object _lock = new();

        static Logger()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        }

        public static void Info(string source, string message) => Write("ℹ️", "Info", source, message);
        public static void Warning(string source, string message) => Write("⚠️", "Warning", source, message);
        public static void Error(string source, string message, Exception? ex = null) => Write("❌", "Error", source, message, ex);
        public static void Debug(string source, string message) => Write("🔍", "Debug", source, message);

        private static void Write(string emoji, string level, string source, string message, Exception? ex = null)
        {
            var line = $"[{DateTime.Now:HH:mm:ss.fff}] {emoji} [{level}] [{source}] {message}";
            if (ex != null) line += $"\n  Exception: {ex.Message}";
            
            lock (_lock)
            {
                File.AppendAllText(_logPath, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
