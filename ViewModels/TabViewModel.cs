using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB_Browser.Services;

namespace TB_Browser.ViewModels;

public partial class TabViewModel : ObservableObject
{
    private readonly TabService _tabService;

    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private bool _isBusy;

    // ✅ FIX CS1729: Matches new TabViewModel(url, title, service) calls
    public TabViewModel(string initialUrl, string initialTitle, TabService tabService)
    {
        _url = initialUrl;
        _title = initialTitle;
        _tabService = tabService;
    }

    [RelayCommand]
    private void Close()
    {
        // Delegate actual close logic to MainViewModel via DI or event if needed
        _ = _tabService; // Placeholder to suppress unused warning
    }
}
