using System.Windows;
using TB_Browser.UI.Controls; // ✅ Add this line

namespace TB_Browser
{
    public partial class MainWindow : Window
    {
        public MainWindow(TabBar tabBar, AddressBar addressBar, BrowserView browserView)
        {
            InitializeComponent();
            TabBarHost.Content = tabBar;
            AddressBarHost.Content = addressBar;
            BrowserHost.Content = browserView;
        }
    }
}
