using System;
using System.Timers;
using TB_Browser.Infrastructure;
using TB_Browser.ViewModels;

namespace TB_Browser.Services;

/// <summary>
/// Monitors tab activity and requests suspension for inactive tabs.
/// </summary>
public class TabService
{
    private readonly Timer _timer;
    public event Action<TabViewModel>? SuspendRequested;

    public TabService()
    {
        // Check every 60 seconds
        _timer = new Timer(60_000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        // This event would typically be raised by MainViewModel tracking last activity.
        // For now, it's a placeholder for the timer infrastructure.
    }

    public void RequestSuspend(TabViewModel tab) => SuspendRequested?.Invoke(tab);
}
