using System;
using System.Windows;
using TB_Browser.Core.Logging;
using TB_Browser.Core.Services;
using TB_Browser.UI.Controls;

namespace TB_Browser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Logger.Info("App", "=== STARTUP BEGIN ===");
            Logger.Info("App", $"Command line: {string.Join(" ", e.Args)}");
            Logger.Info("App", $"Base directory: {AppDomain.CurrentDomain.BaseDirectory}");
            
            try
            {
                Logger.Debug("App", "Creating TabService...");
                var tabSvc = new TabService();
                
                Logger.Debug("App", "Creating BrowserService...");
                var browserSvc = new BrowserService();
                browserSvc.TabService = tabSvc;

                Logger.Debug("App", "Creating UI controls...");
                var tabBar = new TabBar(tabSvc);
                var addressBar = new AddressBar(browserSvc);
                var browserView = new BrowserView(browserSvc);
                
                Logger.Debug("App", "Subscribing to ActiveTabChanged...");
                tabSvc.ActiveTabChanged += (s, tab) => 
                {
                    Logger.Info("App", $"Active tab changed to #{tab?.Id ?? 0}");
                    browserView.SwitchTo(tab);
                };

                Logger.Debug("App", "Creating first tab...");
                tabSvc.CreateTab(); 

                Logger.Debug("App", "Creating MainWindow...");
                var win = new MainWindow(tabBar, addressBar, browserView);
                
                Logger.Debug("App", "Showing window...");
                win.Show();
                
                Logger.Info("App", "=== STARTUP COMPLETE ===");
            }
            catch (Exception ex)
            {
                Logger.Error("App", $"STARTUP FAILED: {ex.Message}");
                Logger.Error("App", $"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Startup failed: {ex.Message}\n\nCheck logs at: .\\logs\\app.log", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger.Info("App", $"=== EXIT (code: {e.ApplicationExitCode}) ===");
            base.OnExit(e);
        }
    }
}
