using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using TradingBrowser.ViewModels;
using TradingBrowser.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TradingBrowser;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();
    private bool _isWebViewInitialized;

    public MainWindow()
    {
        this.InitializeComponent();
        
        RootGrid.DataContext = this; 
        
        if (this.Content is FrameworkElement content)
        {
            content.RequestedTheme = ElementTheme.Dark;
        }

        SetupTitleBar();
        _ = InitializeWebViewAsync();
    }

    private void SetupTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        var appWindow = this.AppWindow;
        appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        appWindow.TitleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            string userDataFolder = Path.Combine(AppContext.BaseDirectory, "UserData", "Profile");
            Directory.CreateDirectory(userDataFolder);

            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", userDataFolder);

            // FIX 3: Performance Optimizations
            // Force GPU rasterization and disable SmartScreen (reduces network latency per request)
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", 
                "--enable-features=msWebView2CodeCache " +
                "--force-gpu-rasterization " +
                "--disable-features=msSmartScreenProtection,CalculateNativeWinOcclusion");

            await MainWebView.EnsureCoreWebView2Async();
            
            var settings = MainWebView.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = true;
            settings.IsGeneralAutofillEnabled = false;    // Speed up rendering
            settings.IsPasswordAutosaveEnabled = false;   // Speed up rendering
            settings.IsPinchZoomEnabled = false;
            settings.IsSwipeNavigationEnabled = false;    // Prevent accidental back/forward lag
            
            MainWebView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            MainWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            MainWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            
            _isWebViewInitialized = true;
            LoggingService.Log("WebView2 initialized with High-Performance flags.");

            if (ViewModel.SelectedTab != null && ViewModel.SelectedTab.Url != "about:blank")
            {
                MainWebView.CoreWebView2.Navigate(ViewModel.SelectedTab.Url);
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error("WebView2 Init Error", ex);
            string bootstrapper = Path.Combine(AppContext.BaseDirectory, "WebView2Bootstrapper.exe");
            if (File.Exists(bootstrapper)) Process.Start(new ProcessStartInfo(bootstrapper) { UseShellExecute = true });
        }
    }

    // --- Navigation Handlers (Fixes Issue 2) ---

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoBack)
            MainWebView.CoreWebView2.GoBack();
    }

    private void Forward_Click(object sender, RoutedEventArgs e)
    {
        if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoForward)
            MainWebView.CoreWebView2.GoForward();
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        if (_isWebViewInitialized) MainWebView.CoreWebView2.Reload();
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        if (_isWebViewInitialized)
        {
            string home = "https://www.google.com";
            MainWebView.CoreWebView2.Navigate(home);
            if (ViewModel.SelectedTab != null) ViewModel.SelectedTab.Url = home;
            ViewModel.OmniboxText = home;
        }
    }

    // --- Tab Management Handlers (Fixes Issue 1) ---

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        // Reliable extraction of TabViewModel from the button's DataContext
        if (sender is FrameworkElement element && element.DataContext is TabViewModel tab)
        {
            ViewModel.CloseTabCommand.Execute(tab);
        }
    }

    private void NewTab_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.AddTabCommand.Execute(null);
    }

    // --- WebView Events ---

    private void TabListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || ViewModel.SelectedTab == null) return;

        if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TabViewModel oldTab)
        {
            oldTab.Url = MainWebView.CoreWebView2.Source; 
        }

        var newTab = ViewModel.SelectedTab;
        ViewModel.OmniboxText = newTab.Url;
        
        if (MainWebView.CoreWebView2.Source != newTab.Url)
        {
            MainWebView.CoreWebView2.Navigate(newTab.Url);
        }
    }

    private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (ViewModel.SelectedTab != null)
        {
            ViewModel.OmniboxText = args.Uri;
            ViewModel.SelectedTab.Url = args.Uri;
            ViewModel.SelectedTab.IsLoading = true;
        }
    }

    private void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (ViewModel.SelectedTab != null)
        {
            ViewModel.SelectedTab.IsLoading = false;
            if (!args.IsSuccess) LoggingService.Error($"Nav Failed: {args.WebErrorStatus}");
        }

        // Update Nav Button States
        BackButton.IsEnabled = sender.CanGoBack;
        ForwardButton.IsEnabled = sender.CanGoForward;
    }

    private void CoreWebView2_DocumentTitleChanged(CoreWebView2 sender, object args)
    {
        if (ViewModel.SelectedTab != null) ViewModel.SelectedTab.Title = sender.DocumentTitle;
    }

    private void Omnibox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ViewModel.NavigateOmniboxCommand.Execute(null);
            e.Handled = true;
        }
    }
}
