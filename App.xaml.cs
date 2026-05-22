using System.Windows;
using TB_Browser.Core.Logging;
using TB_Browser.Core.Services;
using TB_Browser.UI.Controls;

namespace TB_Browser
{
    public partial class App : Application
    {
        public static ILogger Logger { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Initialize Logger
            Logger = new FileLogger();
            Logger.Info("App", "Application started");

            try
            {
                // 2. Services
                var tabService = new TabService(Logger);
                var browserService = new BrowserService(Logger);
                browserService.TabService = tabService;

                // 3. UI Controls
                var tabBar = new TabBar(tabService);
                var addressBar = new AddressBar(browserService);
                var browserView = new BrowserView(browserService);

                // 4. Bind
                tabService.ActiveTabChanged += (s, tab) => browserView.SwitchTo(tab);

                // 5. Show window
                var mainWindow = new MainWindow(tabBar, addressBar, browserView);
                mainWindow.Show();

                Logger.Info("App", "MainWindow shown");
            }
            catch (Exception ex)
            {
                Logger.Critical("App", "Startup failed", ex);
                MessageBox.Show($"Startup error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Logger?.Info("App", "Application exiting");
            (Logger as IDisposable)?.Dispose();
            base.OnExit(e);
        }
    }
}
