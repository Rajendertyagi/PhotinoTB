using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TradingBrowser.Helpers;
using System.IO;

namespace TradingBrowser.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty] private int _selectedSearchEngineIndex;
    [ObservableProperty] private bool _restoreSessionOnStartup;

    public string[] SearchEngines { get; } = { "Google", "Bing", "DuckDuckGo" };

    public SettingsViewModel()
    {
        // Load saved settings
        string engine = SettingsService.Get("SearchEngine", "Google");
        SelectedSearchEngineIndex = engine switch
        {
            "Bing" => 1,
            "DuckDuckGo" => 2,
            _ => 0
        };

        RestoreSessionOnStartup = SettingsService.Get("RestoreSession", "true") == "true";
    }

    partial void OnSelectedSearchEngineIndexChanged(int value)
    {
        SettingsService.Set("SearchEngine", SearchEngines[value]);
    }

    partial void OnRestoreSessionOnStartupChanged(bool value)
    {
        SettingsService.Set("RestoreSession", value.ToString().ToLower());
    }

    [RelayCommand]
    private void ClearBrowsingData()
    {
        string profilePath = Path.Combine(AppContext.BaseDirectory, "UserData", "Profile");
        if (Directory.Exists(profilePath))
        {
            try
            {
                Directory.Delete(profilePath, true);
                LoggingService.Log("Browsing data cleared successfully.");
            }
            catch (IOException)
            {
                // Files might be locked by WebView2. Advise user to restart.
                LoggingService.Log("Could not clear all data. WebView2 may be locking files. Restart required.");
            }
        }
    }
}
