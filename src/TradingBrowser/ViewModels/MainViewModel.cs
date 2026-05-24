using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System;

namespace TradingBrowser.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private TabViewModel? _selectedTab;
    [ObservableProperty] private string _omniboxText = string.Empty;

    public ObservableCollection<TabViewModel> Tabs { get; } = [];

    public MainViewModel() => AddTab();

    [RelayCommand]
    private void AddTab()
    {
        var newTab = new TabViewModel();
        Tabs.Add(newTab);
        SelectedTab = newTab;
        OmniboxText = newTab.Url;
    }

    [RelayCommand]
    private void CloseTab(TabViewModel? tab)
    {
        tab ??= SelectedTab;
        if (tab == null) return;

        int index = Tabs.IndexOf(tab);
        Tabs.Remove(tab);

        if (Tabs.Count == 0) 
        {
            AddTab();
        }
        else 
        {
            SelectedTab = Tabs[Math.Min(index, Tabs.Count - 1)];
        }
    }

    [RelayCommand]
    private void NavigateOmnibox()
    {
        if (SelectedTab == null || string.IsNullOrWhiteSpace(OmniboxText)) return;
        
        string input = OmniboxText.Trim();
        
        // Basic URL parsing fallback (Will be moved to UrlParser helper in Phase 4)
        bool isUrl = Uri.TryCreate(input, UriKind.Absolute, out var uriResult) 
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                     
        if (!isUrl && input.Contains('.') && !input.Contains(' '))
        {
            isUrl = true;
            input = $"https://{input}";
        }

        SelectedTab.Url = isUrl ? input : $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
        SelectedTab.Title = "Loading..."; 
    }

    partial void OnSelectedTabChanging(TabViewModel? value)
    {
        if (value != null) OmniboxText = value.Url;
    }
}
