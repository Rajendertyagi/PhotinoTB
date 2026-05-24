// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
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

        if (Tabs.Count == 0) AddTab();
        else SelectedTab = Tabs[Math.Min(index, Tabs.Count - 1)];
    }

    [RelayCommand]
    private void NavigateOmnibox()
    {
        if (SelectedTab == null || string.IsNullOrWhiteSpace(OmniboxText)) return;
        
        // Basic URL parsing fallback (Detailed logic goes in UrlParser helper later)
        string input = OmniboxText.Trim();
        SelectedTab.Url = Uri.TryCreate(input, UriKind.Absolute, out _) || input.Contains('.') 
            ? (input.StartsWith("http") ? input : $"https://{input}") 
            : $"https://www.google.com/search?q={Uri.EscapeDataString(input)}";
            
        SelectedTab.Title = SelectedTab.Url; // Placeholder until WebView updates title
    }

    partial void OnSelectedTabChanged(TabViewModel? value)
    {
        if (value != null) OmniboxText = value.Url;
    }
}
