using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TB.Core.DI;
using TB.Infrastructure;

namespace TB.Core;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // ✅ Check .NET Desktop Runtime
        if (!System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.Contains(".NET 10"))
        {
            MessageBox.Show("TB requires .NET 10 Desktop Runtime.\n\nDownload: https://dotnet.microsoft.com/download/dotnet/10.0", 
                "Missing Runtime", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        // ✅ Check WebView2
        try
        {
            Microsoft.Web.WebView2.Core.CoreWebView2Environment.GetAvailableBrowserVersionString();
        }
        catch
        {
            MessageBox.Show("TB requires Microsoft Edge WebView2 Runtime.\n\nDownload: https://developer.microsoft.com/en-us/microsoft-edge/webview2/", 
                "Missing WebView2", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        var pathResolver = new PathResolver();
        pathResolver.EnsureDirectories();
        Services = Container.Register(pathResolver);
    }
}
