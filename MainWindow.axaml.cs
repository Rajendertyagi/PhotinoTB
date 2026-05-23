using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.WebView2;
using TB.Features;
using TB.Features.Navigation;
using TB.Features.Tabs;

namespace TB;

public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel(new TabService(), new NavigationViewModel());
        DataContext = ViewModel;
        Opened += MainWindow_Opened;
    }

    private async void MainWindow_Opened(object? sender, EventArgs e)
    {
        if (BrowserView != null && ViewModel.SelectedTab != null)
        {
            await BrowserView.EnsureCoreWebView2Async();
            BrowserView.Source = new Uri(ViewModel.SelectedTab.Url);
            BrowserView.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            BrowserView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
        }
    }

    private void CoreWebView2_SourceChanged(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
    {
        if (BrowserView?.CoreWebView2?.Source != null)
            Omnibox.Text = BrowserView.CoreWebView2.Source;
    }

    private void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        if (ViewModel.SelectedTab != null && BrowserView?.CoreWebView2 != null)
        {
            var title = BrowserView.CoreWebView2.DocumentTitle;
            ViewModel.SelectedTab.Title = string.IsNullOrEmpty(title) ? "Google" : title;
        }
    }

    private async void TabStrip_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (BrowserView == null || e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is TabViewModel tab)
        {
            await BrowserView.EnsureCoreWebView2Async();
            BrowserView.Source = new Uri(tab.Url);
        }
    }

    private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void BtnMin_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void BtnMax_Click(object? sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void BtnClose_Click(object? sender, RoutedEventArgs e) => Close();

    private void AddTab_Click(object? sender, RoutedEventArgs e) => ViewModel.AddTab();
    private void TabClose_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control btn && btn.DataContext is TabViewModel tab)
            tab.Close();
    }

    private void GoBack_Click(object? sender, RoutedEventArgs e) => BrowserView?.CoreWebView2?.GoBack();
    private void GoForward_Click(object? sender, RoutedEventArgs e) => BrowserView?.CoreWebView2?.GoForward();
    private void Reload_Click(object? sender, RoutedEventArgs e) => BrowserView?.CoreWebView2?.Reload();

    private void Omnibox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox tb) return;
        var query = tb.Text?.Trim();
        if (string.IsNullOrEmpty(query)) return;

        var url = query.Contains(".") && !query.StartsWith("http")
            ? $"https://{query}"
            : $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
        BrowserView?.CoreWebView2?.Navigate(url);
        tb.Text = url;
    }

    private void OpenSettings_Click(object? sender, RoutedEventArgs e) { /* Phase 3 */ }
}
