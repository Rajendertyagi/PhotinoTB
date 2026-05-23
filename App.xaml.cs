using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TB_Browser.Infrastructure;
using TB_Browser.Repositories;
using TB_Browser.Services;
using TB_Browser.ViewModels;

namespace TB_Browser;

/// <summary>
/// Application entry point. Manages DI container, startup lifecycle, and global state.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Global service provider for dependency injection across the app.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    public App()
    {
        // 1. Initialize WinUI XAML engine
        InitializeComponent();
        
        // 2. Start logging immediately
        LoggingService.Init();
        LoggingService.Info("TB-Browser starting...");
        
        // 3. Configure DI before any windows or services are created
        ConfigureDI();
    }

    private void ConfigureDI()
    {
        var services = new ServiceCollection();

        // ─── Infrastructure ──
        services.AddSingleton<PathResolver>();
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<DbInitializer>();

        // ─── Repositories (Dapper + SQLite) ───
        services.AddSingleton<BookmarkRepository>();
        services.AddSingleton<HistoryRepository>();

        // ─── Core Services ───
        services.AddSingleton<SettingsService>();
        services.AddSingleton<FaviconService>();
        services.AddSingleton<TabService>();
        services.AddSingleton<BookmarkService>();
        services.AddSingleton<HistoryService>();
        
        // Note: NavigationService, UpdateService, SearchService are registered per spec.
        // Provide minimal implementations or stubs if not yet built.
        services.AddSingleton<NavigationService>();
        services.AddSingleton<UpdateService>();
        services.AddSingleton<SearchService>();

        // ─── ViewModels (MVVM) ──
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<NavigationViewModel>();
        services.AddTransient<TabViewModel>(); // New tabs get fresh instances

        // Build container
        Services = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            LoggingService.Info("App launched. Initializing environment...");

            // 1. Resolve paths & ensure data directories exist
            var pathResolver = Services!.GetRequiredService<PathResolver>();
            pathResolver.EnsureDirectories();

            // 2. Initialize SQLite schema (idempotent, safe to call every launch)
            var dbInit = Services.GetRequiredService<DbInitializer>();
            dbInit.Initialize();

            // 3. Create & activate main window
            var mainWindow = new MainWindow();
            mainWindow.Activate();

            LoggingService.Info("MainWindow activated. Startup complete.");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Critical startup failure", ex);
            
            // Framework-dependent apps should fail gracefully rather than crash silently.
            // In production, you might show a native MessageBox or exit cleanly.
            System.Environment.Exit(1);
        }
    }

    protected override void OnClosed(object sender, AppLifecycleEventArgs args)
    {
        // Flush queues & dispose resources before exit
        try
        {
            var bookmarkService = Services?.GetService<BookmarkService>();
            var historyService = Services?.GetService<HistoryService>();
            
            if (bookmarkService != null) bookmarkService.FlushAsync().Wait();
            if (historyService != null) historyService.FlushAsync().Wait();
            
            LoggingService.Info("App closed cleanly.");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Shutdown error", ex);
        }
        
        base.OnClosed(sender, args);
    }
}
