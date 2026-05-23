using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using TB.Data.Models;
using TB.Infrastructure;

namespace TB.Data.Services;

public class SettingsService
{
    private readonly PathResolver _path;
    public AppSettings Current { get; set; } = new();
    public SettingsService(PathResolver path) => _path = path;
    public async Task LoadAsync()
    {
        var file = Path.Combine(_path.DataDir, "settings.json");
        if (File.Exists(file)) Current = JsonSerializer.Deserialize<AppSettings>(await File.ReadAllTextAsync(file)) ?? new();
    }
    public async Task SaveAsync() => await File.WriteAllTextAsync(Path.Combine(_path.DataDir, "settings.json"), JsonSerializer.Serialize(Current));
    public void ApplyTheme(ElementTheme theme) { if (Window.Current?.Content is FrameworkElement root) root.RequestedTheme = theme; }
}
