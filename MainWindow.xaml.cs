using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TB_Browser.Core.Services;
using TB_Browser.UI.Controls;

namespace TB_Browser
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow(TabBar tabBar, AddressBar addressBar, BrowserView browserView, TabService tabService)
        {
            InitializeComponent();

            TabBarHost.Content = tabBar;
            AddressBarHost.Content = addressBar;
            BrowserHost.Content = browserView;

            // Bind tab changes to browser view
            tabService.ActiveTabChanged += (s, tab) => browserView.SwitchTo(tab);
        }
    }
}
