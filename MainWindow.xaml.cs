using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TB_Browser.UI.Controls;

namespace TB_Browser
{
    public partial class MainWindow : Window
    {
        public MainWindow(TabBar tabBar, AddressBar addressBar, BrowserView browserView)
        {
            InitializeComponent();
            TabBarHost.Content = tabBar; AddressBarHost.Content = addressBar; BrowserHost.Content = browserView;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (_, _) => tabBar.CreateNewTab()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (_, _) => tabBar.CloseActive()));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Properties, (_, _) => addressBar.FocusUrl()));
            CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, (_, _) => browserView.Reload()));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, (_, _) => browserView.GoBack()));
            CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, (_, _) => browserView.GoForward()));

            browserView.SetProgressHandler(ShowProgress);
            browserView.SetStatusHandler(s => StatusText.Text = s);
        }

        private void ShowProgress(bool isVisible)
        {
            LoadProgress.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            if (isVisible)
            {
                LoadProgress.Value = 0;
                LoadProgress.BeginAnimation(ProgressBar.ValueProperty, new DoubleAnimation(100, TimeSpan.FromSeconds(1)));
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }
    }
}
