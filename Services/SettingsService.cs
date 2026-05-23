using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using TB_Browser.Infrastructure;
using TB_Browser.Models;

namespace TB_Browser.Services;

/// <summary>
/// Manages user settings persistence and live theme application.
/// </summary>
public class SettingsService
{
    private readonly PathResolver _pathResolver;
    private readonly JsonSerializerOptions _jsonOptions;
    private AppSettings _settings = new();

    public AppSettings Settings => _settings;

    public SettingsService(PathResolver pathResolver)
    {
        _pathResolver = pathResolver;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task LoadAsync()
    {
        var path = Path.Combine(_pathResolver.DataDirectory, "settings.json");
        try
        {
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
                LoggingService.Info("Settings loaded.");
            }
            else
            {
                await SaveAsync(); // Create default
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load settings", ex);
            _settings = new AppSettings(); // Fallback
        }

        ApplyTheme();
    }

    public async Task SaveAsync()
    {
        var path = Path.Combine(_pathResolver.DataDirectory, "settings.json");
        try
        {
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            await File.WriteAllTextAsync(path, json);
            ApplyTheme();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to save settings", ex);
        }
    }

    private void ApplyTheme()
    {
        var theme = _settings.Theme.ToLowerInvariant();
        var app = Application.Current;
        
        app.RequestedTheme = theme switch
        {
            "light" => ElementTheme.Light,
            "dark" => ElementTheme.Dark,
            _ => ElementTheme.Default // System
        };
    }
}
