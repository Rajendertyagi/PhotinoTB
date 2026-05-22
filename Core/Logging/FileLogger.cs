using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TB_Browser.Core.Logging
{
    public class FileLogger : ILogger, IDisposable
    {
        private readonly string _logPath;
        private readonly object _lock = new();
        private readonly int _maxFileSize = 1024 * 1024; // 1MB
        private readonly int _maxFiles = 5;

        public FileLogger(string logDirectory = "logs")
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDirectory, "app.log");
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            RotateIfNeeded();
        }

        public void Log(LogLevel level, string source, string message, Exception? ex = null)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message,
                Exception = ex
            };

            lock (_lock)
            {
                File.AppendAllText(_logPath, entry.ToString() + Environment.NewLine, Encoding.UTF8);
            }

            RotateIfNeeded();
        }

        private void RotateIfNeeded()
        {
            try
            {
                if (!File.Exists(_logPath)) return;
                var info = new FileInfo(_logPath);
                if (info.Length < _maxFileSize) return;

                // Rotate: app.log → app.1.log → app.2.log ...
                for (int i = _maxFiles - 1; i >= 1; i--)
                {
                    var oldPath = _logPath.Replace(".log", $".{i}.log");
                    var newPath = _logPath.Replace(".log", $".{i + 1}.log");
                    if (File.Exists(oldPath))
                    {
                        if (i + 1 > _maxFiles) File.Delete(oldPath);
                        else File.Move(oldPath, newPath, true);
                    }
                }
                File.Move(_logPath, _logPath.Replace(".log", ".1.log"), true);
            }
            catch { /* Silent fail - don't crash app */ }
        }

        public void Dispose() { }
    }
}
