using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TB.Core.DI;
using TB.Infrastructure;

namespace TB.Core;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        try
        {
            // ✅ 1. Initialize paths & DI
            var pathResolver = new PathResolver();
            pathResolver.EnsureDirectories();
            Services = Container.Register(pathResolver);

            // ✅ 2. Check WebView2 runtime (fail early with message)
            try
            {
                Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"WebView2 Runtime not found.\n\nError: {ex.Message}\n\nDownload: https://developer.microsoft.com/en-us/microsoft-edge/webview2/",
                    "TB Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            // ✅ 3. Create & show MainWindow via DI (or direct new)
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            // ✅ 4. Log + show any unhandled startup errors
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TB", "startup-error.log");
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now}] {ex}\n\n");
            
            MessageBox.Show(
                $"TB failed to start.\n\nError: {ex.Message}\n\nCheck log: {logPath}",
                "TB Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}
