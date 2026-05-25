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
using Microsoft.UI.Windowing;
using Windows.System;

namespace TradingBrowser;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();
    private bool _isWebViewInitialized;
    private readonly SessionService _sessionService;
    private readonly string _shortcutsJs;

    public MainWindow()
    {
        this.InitializeComponent();
        RootGrid.DataContext = this; 
        
        if (this.Content is FrameworkElement content) content.RequestedTheme = ElementTheme.Dark;

        _sessionService = new SessionService(App.Db!);
        
        string jsPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "shortcuts.js");
        _shortcutsJs = File.Exists(jsPath) ? File.ReadAllText(jsPath) : "";

        SetupTitleBar();
        SetupEventHooks();
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

    private void SetupEventHooks()
    {
        RootGrid.PointerPressed += RootGrid_PointerPressed;
        RootGrid.KeyDown += RootGrid_KeyDown;
        
        ViewModel.NavigationRequested += url => { if (_isWebViewInitialized) MainWebView.CoreWebView2.Navigate(url); };
        ViewModel.FocusOmniboxRequested += () => { Omnibox.Focus(FocusState.Programmatic); Omnibox.SelectAll(); };
        ViewModel.ToggleFullscreenRequested += ToggleFullscreen;
        ViewModel.OpenDevToolsRequested += () => { if (_isWebViewInitialized) MainWebView.CoreWebView2.OpenDevToolsWindow(); };

        this.AppWindow.Closing += (s, e) => {
            if (ViewModel.SelectedTab != null)
                _sessionService.SaveSession(ViewModel.Tabs, ViewModel.SelectedTab.Id.ToString());
        };
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            string userDataFolder = Path.Combine(AppContext.BaseDirectory, "UserData", "Profile");
            Directory.CreateDirectory(userDataFolder);

            // FIX: Use the official CoreWebView2EnvironmentOptions API.
            // This safely applies performance flags without crashing the engine, 
            // and explicitly passing the object bypasses C# 14 optional parameter bugs.
            var options = new CoreWebView2EnvironmentOptions
            {
                AdditionalBrowserArguments = "--enable-features=msWebView2CodeCache --force-gpu-rasterization",
                Language = "en-US"
            };

            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
            await MainWebView.EnsureCoreWebView2Async(env);
            
            var settings = MainWebView.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = true;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsPinchZoomEnabled = false;
            settings.IsSwipeNavigationEnabled = false;
            
            MainWebView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            MainWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            MainWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            MainWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            
            if (!string.IsNullOrEmpty(_shortcutsJs))
            {
                await MainWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_shortcutsJs);
            }

            _isWebViewInitialized = true;
            LoggingService.Log("WebView2 initialized successfully using system runtime.");

            var restoredTabs = _sessionService.LoadSession(out string? activeId);
            ViewModel.InitializeSession(restoredTabs, activeId);
        }
        catch (Exception ex)
        {
            LoggingService.Error("WebView2 Init Error", ex);
            // Removed blind bootstrapper execution. 
            // If it fails now, it will log the exact reason to ./logs/ instead of throwing a fake install error.
        }
    }

    private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string msg = args.TryGetWebMessageAsString();
        if (msg?.StartsWith("SHORTCUT:") == true)
        {
            string key = msg.Replace("SHORTCUT:", "");
            ProcessShortcut(key);
        }
    }

    private void RootGrid_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        bool ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        bool shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        string key = e.Key.ToString();
        if (ctrl && shift && key == "T") { ViewModel.ReopenClosedTabCommand.Execute(null); e.Handled = true; }
        else if (ctrl && key == "T") { ViewModel.AddTabCommand.Execute(null); e.Handled = true; }
        else if (ctrl && key == "W") { ViewModel.CloseTabCommand.Execute(null); e.Handled = true; }
        else if (ctrl && key == "L") { ViewModel.TriggerFocusOmnibox(); e.Handled = true; }
        else if (ctrl && key == "Tab") { if (shift) ViewModel.PreviousTab(); else ViewModel.NextTab(); e.Handled = true; }
        else if (key == "F11") { ViewModel.TriggerToggleFullscreen(); e.Handled = true; }
        else if (key == "F12") { ViewModel.TriggerOpenDevTools(); e.Handled = true; }
        else if (key == "F5") { if (_isWebViewInitialized) MainWebView.CoreWebView2.Reload(); e.Handled = true; }
    }

    private void ProcessShortcut(string key)
    {
        if (key == "CTRL_T") ViewModel.AddTabCommand.Execute(null);
        else if (key == "CTRL_W") ViewModel.CloseTabCommand.Execute(null);
        else if (key == "CTRL_L") ViewModel.TriggerFocusOmnibox();
        else if (key == "CTRL_SHIFT_T") ViewModel.ReopenClosedTabCommand.Execute(null);
        else if (key == "CTRL_TAB") ViewModel.NextTab();
        else if (key == "CTRL_SHIFT_TAB") ViewModel.PreviousTab();
        else if (key == "F11") ViewModel.TriggerToggleFullscreen();
        else if (key == "F12") ViewModel.TriggerOpenDevTools();
        else if (key == "F5") { if (_isWebViewInitialized) MainWebView.CoreWebView2.Reload(); }
        else if (key.StartsWith("CTRL_NUM_")) { if (int.TryParse(key[^1..], out int num)) ViewModel.SwitchToTab(num == 9 ? ViewModel.Tabs.Count - 1 : num - 1); }
    }

    private void RootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;
        if (props.IsXButton1Pressed) { if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoBack) MainWebView.CoreWebView2.GoBack(); }
        else if (props.IsXButton2Pressed) { if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoForward) MainWebView.CoreWebView2.GoForward(); }
    }

    private void ToggleFullscreen()
    {
        var presenter = this.AppWindow.Presenter as OverlappedPresenter;
        if (presenter != null)
        {
            if (presenter.State == OverlappedPresenterState.Maximized && !ExtendsContentIntoTitleBar)
            {
                presenter.Restore();
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
            }
            else
            {
                ExtendsContentIntoTitleBar = false;
                SetTitleBar(null);
                presenter.SetBorderAndTitleBar(false, false);
                presenter.Maximize();
            }
        }
    }

    private void Back_Click(object sender, RoutedEventArgs e) { if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoBack) MainWebView.CoreWebView2.GoBack(); }
    private void Forward_Click(object sender, RoutedEventArgs e) { if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoForward) MainWebView.CoreWebView2.GoForward(); }
    private void Reload_Click(object sender, RoutedEventArgs e) { if (_isWebViewInitialized) MainWebView.CoreWebView2.Reload(); }
    private void Home_Click(object sender, RoutedEventArgs e) { ViewModel.GoHomeCommand.Execute(null); }
    private void CloseTab_Click(object sender, RoutedEventArgs e) { if (sender is FrameworkElement el && el.DataContext is TabViewModel tab) ViewModel.CloseTabCommand.Execute(tab); }
    private void NewTab_Click(object sender, RoutedEventArgs e) { ViewModel.AddTabCommand.Execute(null); }

    private void TabListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || ViewModel.SelectedTab == null) return;
        if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TabViewModel oldTab) oldTab.Url = MainWebView.CoreWebView2.Source;
        
        var newTab = ViewModel.SelectedTab;
        ViewModel.OmniboxText = newTab.Url;
        if (MainWebView.CoreWebView2.Source != newTab.Url) MainWebView.CoreWebView2.Navigate(newTab.Url);
    }

    private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (ViewModel.SelectedTab != null) { ViewModel.OmniboxText = args.Uri; ViewModel.SelectedTab.Url = args.Uri; ViewModel.SelectedTab.IsLoading = true; }
    }

    private void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (ViewModel.SelectedTab != null) { ViewModel.SelectedTab.IsLoading = false; if (!args.IsSuccess) LoggingService.Error($"Nav Failed: {args.WebErrorStatus}"); }
        BackButton.IsEnabled = sender.CanGoBack;
        ForwardButton.IsEnabled = sender.CanGoForward;
    }

    private void CoreWebView2_DocumentTitleChanged(CoreWebView2 sender, object args)
    {
        if (ViewModel.SelectedTab != null) ViewModel.SelectedTab.Title = sender.DocumentTitle;
    }

    private void Omnibox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.Enter) { ViewModel.NavigateOmniboxCommand.Execute(null); e.Handled = true; }
    }
}
