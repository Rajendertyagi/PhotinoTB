using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;
using TB.Features;
using TB.Features.Navigation;
using TB.Features.Tabs;

namespace TB.Core;

public partial class MainWindow
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        try
        {
            InitializeComponent();
            ViewModel = new MainViewModel(new TabService(), new NavigationViewModel());
            DataContext = ViewModel;
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }
        catch (Exception ex)
        {
            LogError("MainWindow constructor", ex);
            throw;
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await BrowserView.EnsureCoreWebView2Async();
            if (BrowserView.CoreWebView2 != null && ViewModel.SelectedTab != null)
            {
                BrowserView.CoreWebView2.Navigate(ViewModel.SelectedTab.Url);
                BrowserView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            }
        }
        catch (Exception ex)
        {
            LogError("WebView2 initialization", ex);
            Omnibox.Text = $"Error: {ex.Message}";
        }
    }

    private void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        try
        {
            if (ViewModel.SelectedTab != null && BrowserView.CoreWebView2 != null)
            {
                var title = string.IsNullOrEmpty(BrowserView.CoreWebView2.DocumentTitle)
                    ? ViewModel.SelectedTab.Url
                    : BrowserView.CoreWebView2.DocumentTitle;
                ViewModel.SelectedTab.Title = title;
            }
        }
        catch (Exception ex)
        {
            LogError("NavigationCompleted handler", ex);
        }
    }

    private async void TabStrip_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BrowserView.CoreWebView2 == null || e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is TabViewModel tab)
        {
            try
            {
                await BrowserView.EnsureCoreWebView2Async();
                BrowserView.CoreWebView2.Navigate(tab.Url);
            }
            catch (Exception ex)
            {
                LogError("Tab switch navigation", ex);
            }
        }
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void BtnMax_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private void GoBack_Click(object sender, RoutedEventArgs e)
    {
        try { BrowserView.CoreWebView2?.GoBack(); }
        catch (Exception ex) { LogError("GoBack", ex); }
    }

    private void GoForward_Click(object sender, RoutedEventArgs e)
    {
        try { BrowserView.CoreWebView2?.GoForward(); }
        catch (Exception ex) { LogError("GoForward", ex); }
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        try { BrowserView.CoreWebView2?.Reload(); }
        catch (Exception ex) { LogError("Reload", ex); }
    }

    private void Omnibox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox tb) return;
        var query = tb.Text?.Trim();
        if (string.IsNullOrEmpty(query)) return;

        try
        {
            var url = query.Contains(".") && !query.StartsWith("http")
                ? $"https://{query}"
                : $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";
            BrowserView.CoreWebView2?.Navigate(url);
            tb.Text = url;
        }
        catch (Exception ex)
        {
            LogError("Omnibox navigation", ex);
            tb.Text = $"Error: {ex.Message}";
        }
    }

    private void TabClose_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button btn && btn.DataContext is TabViewModel tab)
                tab.Close();
        }
        catch (Exception ex) { LogError("TabClose", ex); }
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e) { /* Phase 3 */ }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        try { BrowserView?.Dispose(); }
        catch { /* Ignore cleanup errors */ }
    }

    private static void LogError(string context, Exception ex)
    {
        var message = $"[{DateTime.Now:HH:mm:ss}] {context}: {ex.Message}\n{ex}";
        System.Diagnostics.Debug.WriteLine(message);
        try
        {
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TB", "runtime.log");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath)!);
            System.IO.File.AppendAllText(logPath, message + "\n\n");
        }
        catch { /* Ignore logging failures */ }
    }
}
