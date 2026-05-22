using System.Windows;
using TB_Browser.UI.Controls; // ✅ Added missing namespace

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

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
