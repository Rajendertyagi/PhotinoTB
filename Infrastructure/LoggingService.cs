using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TB_Browser.Infrastructure;

public static class LoggingService
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static string? _currentLogPath;

    public static void Init()
    {
        // Log filename: app-YYYY-MM-DD.log
        _currentLogPath = Path.Combine(
            AppContext.BaseDirectory, "logs", 
            $"app-{DateTime.Now:yyyy-MM-dd}.log");
        
        // Ensure logs dir exists
        Directory.CreateDirectory(Path.GetDirectoryName(_currentLogPath)!);
    }

    public static void Info(string message) => Write("INFO", message);
    public static void Error(string message, Exception? ex = null) => Write("ERROR", message, ex);

    private static async void Write(string level, string message, Exception? ex = null)
    {
        try
        {
            await _semaphore.WaitAsync();
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            if (ex != null) logLine += $"\n{ex}";
            
            await File.AppendAllTextAsync(_currentLogPath!, logLine + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
