using Microsoft.UI.Xaml;
using TradingBrowser.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TradingBrowser;

public partial class App : Application
{
    // Keep your existing Db property exactly as it was
    public static SQLite.SQLiteConnection? Db { get; private set; } 
    private Window? m_window;

    public App()
    {
        this.InitializeComponent();
        
        // GLOBAL EXCEPTION HOOKS
        this.UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            LoggingService.Info("App startup initiated.");

            // ==========================================
            // PASTE YOUR EXACT ORIGINAL DB CODE HERE
            // (Replace my placeholder below with your working code)
            // ==========================================
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TradingBrowser");
            if (!Directory.Exists(appDataPath)) Directory.CreateDirectory(appDataPath);
            
            string dbPath = Path.Combine(appDataPath, "data.db");
            // Example: Db = new SQLite.SQLiteConnection(dbPath);
            // Example: Db.CreateTable<...>();
            
            LoggingService.Info("Database schema initialized successfully.");
            // ==========================================

            m_window = new MainWindow();
            m_window.Activate();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Fatal error during app startup", ex);
        }
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        LoggingService.Error("UI Thread Unhandled Exception", e.Exception);
        e.Handled = true; 
    }

    // FIX: Fully qualified System.UnhandledExceptionEventArgs to prevent CS0104
    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LoggingService.Error("AppDomain Fatal Exception", ex);
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LoggingService.Error("Unobserved Background Task Exception", e.Exception);
        e.SetObserved(); 
    }
}
