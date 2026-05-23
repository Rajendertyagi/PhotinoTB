using Avalonia;
using System;

namespace TB;

class Program
{
    // Entry point for Avalonia application
    [STAThread]
    public static void Main(string[] args) => 
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
