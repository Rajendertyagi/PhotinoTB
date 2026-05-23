using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using TB_Browser.Infrastructure;
using TB_Browser.Services;
using TB_Browser.ViewModels;
using TB_Browser.Repositories;

namespace TB_Browser;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    public App()
    {
        InitializeComponent();
        
        // Initialize Logging immediately
        LoggingService.Init();
        LoggingService.Info("App started.");
        
        ConfigureDI();
    }

    private void ConfigureDI()
    {
        var services = new ServiceCollection();
        
        // --- Infrastructure ---
        services.AddSingleton<PathResolver>();
        services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
        services.AddSingleton<DbInitializer>();
        
        // --- Services ---
        services.AddSingleton<SettingsService>();
        services.AddSingleton<FaviconService>();
        services.AddSingleton<TabService>();
        services.AddSingleton<NavigationService>();
        services.AddSingleton<BookmarkService>();
        services.AddSingleton<HistoryService>();
        services.AddSingleton<SearchService>();
        services.AddSingleton<UpdateService>();
        
        // --- Repositories ---
        services.AddSingleton<BookmarkRepository>();
        services.AddSingleton<HistoryRepository>();
        
        // --- ViewModels ---
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<NavigationViewModel>();
        services.AddTransient<TabViewModel>(); // NewTab creates transient instances

        Services = services.BuildServiceProvider();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            // Ensure Data Directory exists
            var pathResolver = Services!.GetRequiredService<PathResolver>();
            pathResolver.EnsureDirectories();
            
            // Initialize Database (Async but we block here for startup simplicity, 
            // or fire-and-forget if we add a splash screen later. For now, sync init is safer.)
            var dbInit = Services!.GetRequiredService<DbInitializer>();
            dbInit.Initialize();
            
            // Create Main Window
            var mainWindow = new MainWindow();
            mainWindow.Activate();
            
            LoggingService.Info("MainWindow activated.");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Startup failed", ex);
            // Fallback to simple dialog if UI fails
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog();
            dialog.Title = "Startup Error";
            dialog.Content = ex.Message;
            dialog.PrimaryButtonText = "Close";
            // Note: Dialog requires active window, but if app crashes here, we just exit.
        }
    }
}
