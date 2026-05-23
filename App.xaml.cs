using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;
using TB.Services;
using TB.ViewModels;

namespace TB;

public partial class App : Application
{
    public static IServiceProvider Services { get; } = new ServiceCollection()
        .AddSingleton<WebViewService>()
        .AddSingleton<TabStateManager>()
        .AddSingleton<NavigationViewModel>()
        .AddSingleton<MainViewModel>()
        .BuildServiceProvider();

    public App()
    {
        try
        {
            // ✅ Console + File logging for missing runtime diagnostics
            Console.WriteLine("[TB] Initializing Windows App SDK Bootstrap...");
            var logPath = Path.Combine(AppContext.BaseDirectory, "TB_Startup.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] Initializing Bootstrap...\n");

            // 0x00010000 = v1.0.0.0+. Fully compatible with SDK 2.x runtime.
            Bootstrap.Initialize(0x00010000);
            
            Console.WriteLine("[TB] ✅ Bootstrap OK");
            File.AppendAllText(logPath, $"[{DateTime.Now:O}] Bootstrap OK\n");
        }
        catch (Exception ex)
        {
            var msg = $"[TB] ❌ FATAL BOOTSTRAP ERROR: {ex.Message}\n" +
                      $"Missing: Windows App SDK Runtime or WebView2 Runtime.\n" +
                      $"Fix (Admin PowerShell):\n" +
                      $"  winget install Microsoft.WindowsAppRuntime.1.5\n" +
                      $"  winget install Microsoft.EdgeWebView2Runtime";
            Console.Error.WriteLine(msg);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "TB_Error.log"), msg);
            throw; // Let app crash visibly with stack trace
        }

        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = new MainWindow();
        window.Activate();
    }
}
