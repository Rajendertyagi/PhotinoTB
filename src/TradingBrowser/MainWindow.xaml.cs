using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TradingBrowser.Helpers;

namespace TradingBrowser.ViewModels;

public enum TilingLayout { None, Horizontal, Vertical, Grid }

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private TabViewModel? _selectedTab;
    [ObservableProperty] private string _omniboxText = string.Empty;
    [ObservableProperty] private bool _canGoBack;
    [ObservableProperty] private bool _canGoForward;

    // ==========================================
    // TILING STATE
    // ==========================================
    [ObservableProperty] private TilingLayout _currentTilingLayout = TilingLayout.None;
    public ObservableCollection<TabViewModel> TiledTabs { get; } = [];

    public event Action<TilingLayout>? TilingLayoutChanged;
    public event Action<ICollection<TabViewModel>>? TilingTabsChanged;
    // ==========================================

    public ObservableCollection<TabViewModel> Tabs { get; } = [];
    private readonly Stack<string> _closedTabs = new();
    private string _searchEngine = "Google";

    public event Action<string>? NavigationRequested;
    public event Action? FocusOmniboxRequested;
    public event Action? ToggleFullscreenRequested;
    public event Action? OpenDevToolsRequested;

    public MainViewModel() { }

    public void InitializeSession(List<TabViewModel> restoredTabs, string? activeTabId)
    {
        Tabs.Clear();
        if (restoredTabs.Any())
        {
            foreach (var tab in restoredTabs) Tabs.Add(tab);
            SelectedTab = Tabs.FirstOrDefault(t => t.Id.ToString() == activeTabId) ?? Tabs.First();
        }
        else
        {
            AddTab();
        }
    }

    [RelayCommand] private void AddTab() { /* ... existing logic ... */ }
    [RelayCommand] private void CloseTab(TabViewModel? tab) { /* ... existing logic ... */ }
    [RelayCommand] private void ReopenClosedTab() { /* ... existing logic ... */ }
    [RelayCommand] private void DuplicateTab(TabViewModel? tab) { /* ... existing logic ... */ }
    [RelayCommand] private void PinTab(TabViewModel? tab) { /* ... existing logic ... */ }
    [RelayCommand] private void CloseOtherTabs(TabViewModel? tab) { /* ... existing logic ... */ }
    [RelayCommand] private void CloseTabsToRight(TabViewModel? tab) { /* ... existing logic ... */ }
    [RelayCommand] private void NavigateOmnibox() { /* ... existing logic ... */ }
    [RelayCommand] private void GoHome() { /* ... existing logic ... */ }
    [RelayCommand] private void NavigateToUrl(string url) { /* ... existing logic ... */ }

    public void UpdateNavigationState(bool canGoBack, bool canGoForward)
    {
        CanGoBack = canGoBack;
        CanGoForward = canGoForward;
    }

    public void NextTab() { /* ... existing logic ... */ }
    public void PreviousTab() { /* ... existing logic ... */ }
    public void SwitchToTab(int index) { /* ... existing logic ... */ }
    public void TriggerFocusOmnibox() => FocusOmniboxRequested?.Invoke();
    public void TriggerToggleFullscreen() => ToggleFullscreenRequested?.Invoke();
    public void TriggerOpenDevTools() => OpenDevToolsRequested?.Invoke();

    partial void OnSelectedTabChanging(TabViewModel? value) { if (value != null) OmniboxText = value.Url; }

    // ==========================================
    // TILING ENGINE
    // ==========================================
    public void TileSelection(IEnumerable<TabViewModel> selection, TilingLayout layout)
    {
        var tabs = selection.Take(2).ToList(); // Vivaldi supports 2+; we cap at 2 for stable WebView2 perf
        if (tabs.Count < 2) return;

        TiledTabs.Clear();
        foreach (var t in tabs) TiledTabs.Add(t);
        
        CurrentTilingLayout = layout;
        TilingTabsChanged?.Invoke(TiledTabs);
        TilingLayoutChanged?.Invoke(layout);
    }

    [RelayCommand]
    private void UntileTabs()
    {
        TiledTabs.Clear();
        CurrentTilingLayout = TilingLayout.None;
        TilingTabsChanged?.Invoke(TiledTabs);
        TilingLayoutChanged?.Invoke(TilingLayout.None);
    }

    [RelayCommand]
    private void SwitchTilingLayout(TilingLayout layout)
    {
        if (TiledTabs.Count >= 2 && layout != CurrentTilingLayout)
        {
            CurrentTilingLayout = layout;
            TilingLayoutChanged?.Invoke(layout);
        }
    }
}
