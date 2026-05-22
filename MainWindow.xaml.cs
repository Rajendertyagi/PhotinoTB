using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TB_Browser.UI.Controls;

namespace TB_Browser;

public partial class MainWindow : Window
{
    public MainWindow(TabBar tabBar, AddressBar addressBar, BrowserView browserView)
    {
        InitializeComponent();
        TabBarHost.Content = tabBar;
        AddressBarHost.Content = addressBar;
        BrowserHost.Content = browserView;

        CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (_, _) => tabBar.CreateNewTab()));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (_, _) => tabBar.CloseActive()));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Refresh, (_, _) => browserView.Reload()));
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
