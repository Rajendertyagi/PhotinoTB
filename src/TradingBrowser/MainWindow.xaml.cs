using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using TradingBrowser.ViewModels;
using TradingBrowser.Services;
using TradingBrowser.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using System.Collections.Generic;

namespace TradingBrowser;

/// <summary>
/// Main application window.
/// Follows clean architecture by delegating business logic to services and ViewModels.
/// This class focuses solely on UI initialization and event wiring.
/// </summary>
public sealed partial class MainWindow : Window
{
    // Core ViewModel for UI state management
    public MainWindowViewModel ViewModel { get; } = new();
    
    // Flag indicating if WebView2 has been initialized
    private bool _isWebViewInitialized;
    
    // Core services for different functionalities
    private readonly SessionService _sessionService;
    private readonly ShortcutService _shortcutService;
    private readonly HistoryBookmarkService _hbService;
    private readonly DownloadService _downloadService;
    private WebViewNavigationService? _navService; // Initialized after WebView
    
    // JavaScript injectors loaded from files
    private readonly string _shortcutsJs;
    private readonly string _tradingViewJs;

    /// <summary>
    /// Constructor: Initializes components, services, and event handlers.
    /// </summary>
    public MainWindow()
    {
        this.InitializeComponent();
        RootGrid.DataContext = this; 
        
        // Enforce Dark Theme on the root content
        if (this.Content is FrameworkElement content) 
            content.RequestedTheme = ElementTheme.Dark;

        // Initialize all backend services
        _sessionService = new SessionService(App.Db!);
        _hbService = new HistoryBookmarkService(App.Db!);
        _downloadService = new DownloadService(App.Db!);
        
        // Initialize Shortcut Service with WebView access delegate
        _shortcutService = new ShortcutService(
            ViewModel, 
            () => _isWebViewInitialized ? MainWebView.CoreWebView2 : null
        );

        // Hook up the Bookmark shortcut event (Ctrl+D)
        _shortcutService.BookmarkRequested += () => {
            if (ViewModel.SelectedTab != null)
            {
                ToggleBookmark(ViewModel.SelectedTab.Url, ViewModel.SelectedTab.Title);
            }
        };

        // Load JavaScript Injectors from disk
        string shortcutsPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "shortcuts.js");
        _shortcutsJs = File.Exists(shortcutsPath) ? File.ReadAllText(shortcutsPath) : "";

        string tvJsPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "tradingview-tweaks.js");
        _tradingViewJs = File.Exists(tvJsPath) ? File.ReadAllText(tvJsPath) : "";

        SetupTitleBar();
        SetupEventHooks();
        _ = InitializeWebViewAsync();
    }

    /// <summary>
    /// Configures the custom title bar with transparent buttons.
    /// </summary>
    private void SetupTitleBar()
    {
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        var appWindow = this.AppWindow;
        appWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        appWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        appWindow.TitleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
    }

    /// <summary>
    /// Wires up all event handlers for UI interactions.
    /// </summary>
    private void SetupEventHooks()
    {
        // Route raw UI events to the ShortcutService
        RootGrid.PointerPressed += (s, e) => _shortcutService.HandlePointerPressed(e);
        RootGrid.KeyDown += (s, e) => _shortcutService.HandleUiKeyDown(e);
        
        // Delegate navigation requests to WebView
        ViewModel.NavigationRequested += url => { 
            if (_isWebViewInitialized) 
                MainWebView.CoreWebView2.Navigate(url); 
        };
        
        // Handle omnibox focus requests
        ViewModel.FocusOmniboxRequested += () => { 
            Omnibox.Focus(FocusState.Programmatic); 
            Omnibox.SelectAll(); 
        };
        
        // Handle fullscreen toggle requests
        ViewModel.ToggleFullscreenRequested += ToggleFullscreen;
        
        // Handle dev tools requests
        ViewModel.OpenDevToolsRequested += () => { 
            if (_isWebViewInitialized) 
                MainWebView.CoreWebView2.OpenDevToolsWindow(); 
        };

        // Save session to SQLite when the window is closing
        this.AppWindow.Closing += (s, e) => {
            if (ViewModel.SelectedTab != null)
                _sessionService.SaveSession(ViewModel.Tabs, ViewModel.SelectedTab.Id.ToString());
        };
    }

    /// <summary>
    /// Initializes WebView2 with environment variables and injects scripts.
    /// </summary>
    private async Task InitializeWebViewAsync()
    {
        try
        {
            string userDataFolder = Path.Combine(AppContext.BaseDirectory, "UserData", "Profile");
            Directory.CreateDirectory(userDataFolder);

            // Bypass C# compiler overload bugs using official WebView2 environment variables
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", userDataFolder);
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--enable-features=msWebView2CodeCache --force-gpu-rasterization");
            Environment.SetEnvironmentVariable("WEBVIEW2_LANGUAGE", "en-US");

            await MainWebView.EnsureCoreWebView2Async();
            
            // Configure WebView2 settings for performance and UX
            var settings = MainWebView.CoreWebView2.Settings;
            settings.IsStatusBarEnabled = false;
            settings.AreDefaultContextMenusEnabled = true;
            settings.IsGeneralAutofillEnabled = false;
            settings.IsPasswordAutosaveEnabled = false;
            settings.IsPinchZoomEnabled = false;
            settings.IsSwipeNavigationEnabled = false;
            
            // Attach CoreWebView2 event handlers
            MainWebView.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
            MainWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            MainWebView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            MainWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            
            // Initialize Download Manager to intercept file downloads
            _downloadService.Initialize(MainWebView.CoreWebView2);
            
            // Initialize Navigation Service for special URI handling
            _navService = new WebViewNavigationService(_downloadService, MainWebView.CoreWebView2);
            
            // Inject JavaScript files for shortcuts and TradingView tweaks
            if (!string.IsNullOrEmpty(_shortcutsJs))
                await MainWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_shortcutsJs);
            if (!string.IsNullOrEmpty(_tradingViewJs))
                await MainWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_tradingViewJs);

            _isWebViewInitialized = true;
            LoggingService.Log("WebView2 initialized successfully via Environment Variables.");

            // Session Restore Logic based on User Settings
            bool shouldRestore = SettingsService.Get("RestoreSession", "true") == "true";
            if (shouldRestore)
            {
                var restoredTabs = _sessionService.LoadSession(out string? activeId);
                ViewModel.InitializeSession(restoredTabs, activeId);
            }
            else
            {
                ViewModel.InitializeSession(new List<TabViewModel>(), null);
            }
            
            // Refresh Sidebar with data from DB on startup
            RefreshSidebar();
        }
        catch (Exception ex)
        {
            LoggingService.Error("WebView2 Init Error", ex);
        }
    }

    /// <summary>
    /// Refreshes the Sidebar ListViews with the latest data from the SQLite database.
    /// </summary>
    private void RefreshSidebar()
    {
        var b = _hbService.GetBookmarks();
        var h = _hbService.GetHistory();
        
        var bookmarkList = new List<ViewModels.BookmarkItem>();
        foreach(var item in b) 
            bookmarkList.Add(new ViewModels.BookmarkItem { Url = item.Url, Title = item.Title });
        BookmarkListView.ItemsSource = bookmarkList;

        var historyList = new List<ViewModels.HistoryItem>();
        foreach(var item in h) 
            historyList.Add(new ViewModels.HistoryItem { Url = item.Url, Title = item.Title, VisitTime = item.Time });
        HistoryListView.ItemsSource = historyList;

        // Update Star icon state for current tab
        if (ViewModel.SelectedTab != null)
        {
            bool isBookmarked = _hbService.IsBookmarked(ViewModel.SelectedTab.Url);
            BookmarkButton.Content = isBookmarked ? "★" : "☆";
        }
    }

    /// <summary>
    /// Toggles the bookmark status of the current URL.
    /// </summary>
    private void ToggleBookmark(string url, string title)
    {
        if (string.IsNullOrEmpty(url)) return;
        
        bool isBookmarked = _hbService.IsBookmarked(url);
        
        if (isBookmarked)
        {
            _hbService.RemoveBookmark(url);
            BookmarkButton.Content = "☆";
            LoggingService.Log($"Removed bookmark: {title}");
        }
        else
        {
            _hbService.AddBookmark(url, title);
            BookmarkButton.Content = "★";
            LoggingService.Log($"Added bookmark: {title}");
        }
        
        RefreshSidebar();
    }

    // --- Event Handlers for ListViews ---
    private void BookmarkListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BookmarkListView.SelectedItem is ViewModels.BookmarkItem item)
        {
            ViewModel.NavigateToUrlCommand.Execute(item.Url);
            MainSplitView.IsPaneOpen = false; 
            BookmarkListView.SelectedItem = null;
        }
    }

    private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (HistoryListView.SelectedItem is ViewModels.HistoryItem item)
        {
            ViewModel.NavigateToUrlCommand.Execute(item.Url);
            MainSplitView.IsPaneOpen = false;
            HistoryListView.SelectedItem = null;
        }
    }

    // --- UI Click Handlers ---
    private void Bookmark_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedTab != null)
        {
            ToggleBookmark(ViewModel.SelectedTab.Url, ViewModel.SelectedTab.Title);
        }
    }
    
    /// <summary>
    /// Opens the Downloads History page.
    /// Delegates to navigation service.
    /// </summary>
    private void Downloads_Click(object sender, RoutedEventArgs e)
    {
        if (_isWebViewInitialized)
        {
            MainWebView.CoreWebView2.Navigate("about:downloads");
        }
    }
    
    private void Back_Click(object sender, RoutedEventArgs e) { 
        if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoBack) 
            MainWebView.CoreWebView2.GoBack(); 
    }
    
    private void Forward_Click(object sender, RoutedEventArgs e) { 
        if (_isWebViewInitialized && MainWebView.CoreWebView2.CanGoForward) 
            MainWebView.CoreWebView2.GoForward(); 
    }
    
    private void Reload_Click(object sender, RoutedEventArgs e) { 
        if (_isWebViewInitialized) 
            MainWebView.CoreWebView2.Reload(); 
    }
    
    private void Home_Click(object sender, RoutedEventArgs e) { 
        ViewModel.GoHomeCommand.Execute(null); 
    }
    
    private void CloseTab_Click(object sender, RoutedEventArgs e) { 
        if (sender is FrameworkElement el && el.DataContext is TabViewModel tab) 
            ViewModel.CloseTabCommand.Execute(tab); 
    }
    
    private void NewTab_Click(object sender, RoutedEventArgs e) { 
        ViewModel.AddTabCommand.Execute(null); 
    }

    /// <summary>
    /// Routes messages from WebView2 JavaScript to appropriate handlers.
    /// </summary>
    private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string? msg = args.TryGetWebMessageAsString();
        if (msg == null) return;

        // Route Shortcut messages
        if (msg.StartsWith("SHORTCUT:"))
        {
            _shortcutService.HandleWebViewMessage(msg);
        }
        // Route JS Error logs
        else if (msg.StartsWith("LOG:"))
        {
            LoggingService.Log(msg, "WEBVIEW_JS");
        }
        // Route Download Page interactions to Navigation Service
        else if (msg.StartsWith("REMOVE_DOWNLOAD:") || 
                 msg == "CLEAR_ALL_DOWNLOADS" || 
                 msg.StartsWith("COPY_LINK:"))
        {
            _navService?.HandleDownloadPageMessage(msg);
        }
    }

    /// <summary>
    /// Toggles between fullscreen and windowed mode.
    /// </summary>
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

    // --- WebView State Sync ---
    private void TabListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isWebViewInitialized || ViewModel.SelectedTab == null) return;
        if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is TabViewModel oldTab) 
            oldTab.Url = MainWebView.CoreWebView2.Source;
        
        var newTab = ViewModel.SelectedTab;
        ViewModel.OmniboxText = newTab.Url;
        if (MainWebView.CoreWebView2.Source != newTab.Url) 
            MainWebView.CoreWebView2.Navigate(newTab.Url);
        
        bool isBookmarked = _hbService.IsBookmarked(newTab.Url);
        BookmarkButton.Content = isBookmarked ? "★" : "☆";
    }

    /// <summary>
    /// Handles navigation starting events.
    /// Intercepts special URIs like 'about:downloads'.
    /// </summary>
    private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        // Delegate special URI handling to Navigation Service
        if (_navService != null && _navService.HandleSpecialUri(args.Uri))
        {
            args.Cancel = true;
            return;
        }

        if (ViewModel.SelectedTab != null) 
        { 
            ViewModel.OmniboxText = args.Uri; 
            ViewModel.SelectedTab.Url = args.Uri; 
            ViewModel.SelectedTab.IsLoading = true; 
        }
    }

    /// <summary>
    /// Handles navigation completion events.
    /// Updates UI state and saves to history.
    /// </summary>
    private void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        if (ViewModel.SelectedTab != null) 
        { 
            ViewModel.SelectedTab.IsLoading = false; 
            if (!args.IsSuccess) 
                LoggingService.Error($"Nav Failed: {args.WebErrorStatus}");
        }
        
        // Update navigation button states via ViewModel
        ViewModel.UpdateNavigationState(sender.CanGoBack, sender.CanGoForward);

        // Add to History if navigation was successful
        if (args.IsSuccess && ViewModel.SelectedTab != null)
        {
            _hbService.AddHistory(ViewModel.SelectedTab.Url, ViewModel.SelectedTab.Title);
            RefreshSidebar();
        }
    }

    private void CoreWebView2_DocumentTitleChanged(CoreWebView2 sender, object args)
    {
        if (ViewModel.SelectedTab != null) 
        {
            ViewModel.SelectedTab.Title = sender.DocumentTitle;
        }
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
