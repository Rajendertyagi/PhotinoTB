using System.Windows;
using System.Windows.Input;
using TB_Browser.UI.Controls;

namespace TB_Browser
{
    public partial class MainWindow : Window
    {
        private TabBar? _tabBar;
        private AddressBar? _addressBar;
        private BrowserView? _browserView;

        public MainWindow(TabBar tabBar, AddressBar addressBar, BrowserView browserView)
        {
            InitializeComponent();
            _tabBar = tabBar; _addressBar = addressBar; _browserView = browserView;
            TabBarHost.Content = tabBar;
            AddressBarHost.Content = addressBar;
            BrowserHost.Content = browserView;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (_, _) => _tabBar?.CreateNewTab()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (_, _) => _tabBar?.CloseActive()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties, (_, _) => _addressBar?.FocusUrl()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Refresh, (_, _) => _browserView?.Reload()));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, (_, _) => _browserView?.GoBack()));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, (_, _) => _browserView?.GoForward()));

            _browserView?.SetProgressHandler(ShowProgress);
            _browserView?.SetStatusHandler(s => StatusText.Text = s);
        }

        private void ShowProgress(bool isVisible)
        {
            LoadProgress.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            if (isVisible)
            {
                LoadProgress.Value = 0;
                LoadProgress.BeginAnimation(ProgressBar.ValueProperty, new System.Windows.Media.Animation.DoubleAnimation(100, TimeSpan.FromSeconds(1)));
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}
