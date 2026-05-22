using System.Windows;
using TB_Browser.Core.Services;
using TB_Browser.UI.Controls;

namespace TB_Browser
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Instantiate Services
            var tabService = new TabService();
            var browserService = new BrowserService();
            browserService.TabService = tabService;

            // 2. Create UI Controls & Inject Services
            var tabBar = new TabBar(tabService);
            var addressBar = new AddressBar(browserService);
            var browserView = new BrowserView(browserService);

            // 3. Bind Tab Changes to Browser View
            tabService.ActiveTabChanged += (s, tab) => browserView.SwitchTo(tab);

            // 4. Create MainWindow and Show
            var mainWindow = new MainWindow(tabBar, addressBar, browserView);
            mainWindow.Show();
        }
    }
}
