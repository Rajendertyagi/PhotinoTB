using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using TradingBrowser.Helpers;
using TradingBrowser.Services;

namespace TradingBrowser.ViewModels;

public enum TilingLayout { None, Horizontal, Vertical, Grid }

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private TabViewModel? _selectedTab;
    
    // ✅ FIX: Manual implementation to avoid CS0111 generator conflict
    private string _omniboxText = string.Empty;
    public string OmniboxText
    {
        get => _omniboxText;
        set => SetProperty(ref _omniboxText, value);
    }
    
    // ✅ FIX: Manual implementation to avoid CS0111 generator conflict
    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set => SetProperty(ref _canGoBack, value);
    }

    private bool _canGoForward;
    public bool CanGoForward
    {
        get => _canGoForward;
        set => SetProperty(ref _canGoForward, value);
    }

    [ObservableProperty] private TilingLayout _currentTilingLayout = TilingLayout.None;
    public ObservableCollection<TabViewModel> TiledTabs { get; } = [];

    public event Action<TilingLayout>? TilingLayoutChanged;
    public event Action<ICollection<TabViewModel>>? TilingTabsChanged;

    public ObservableCollection<TabViewModel> Tabs { get; } = [];
    private readonly Stack<string> _closedTabs = new();
    private string _searchEngine = "Google";

    public event Action<string>? NavigationRequested;
    public event Action? FocusOmniboxRequested;
    public event Action? ToggleFullscreenRequested;
    public event Action? OpenDevToolsRequested;

    public MainViewModel()
    {
        Tabs.CollectionChanged += (s, e) =>
        {
            LoggingService.Info($"[VM] Tabs collection: {e.Action}. Total: {Tabs.Count}");
        };
    }

    public void InitializeSession(List<TabViewModel> restoredTabs, string? activeTabId)
    {
        LoggingService.Info($"[VM] InitializeSession called. Restored tabs: {restoredTabs.Count}. ActiveId: {activeTabId ?? "null"}");
        Tabs.Clear();
        if (restoredTabs.Any())
        {
            foreach (var tab in restoredTabs) Tabs.Add(tab);
            SelectedTab = Tabs.FirstOrDefault(t => t.Id.ToString() == activeTabId) ?? Tabs.First();
            LoggingService.Info($"[VM] Session restored. Active tab: {SelectedTab?.Title}");
        }
        else
        {
            LoggingService.Info("[VM] No session. Creating fresh tab.");
            AddTab();
        }
    }

    [RelayCommand]
    private void AddTab()
    {
        var newTab = new TabViewModel
        {
            Id = Guid.NewGuid(),
            Title = "New Tab",
            Url = "https://www.google.com"
        };
        Tabs.Add(newTab);
        SelectedTab = newTab;
        LoggingService.Info($"[VM] AddTab executed. New ID: {newTab.Id}. Total tabs: {Tabs.Count}");
    }

    [RelayCommand]
    private void CloseTab(TabViewModel? tab)
    {
        if (tab == null) { LoggingService.Warning("[VM] CloseTab called with null tab."); return; }
        int index = Tabs.IndexOf(tab);
        _closedTabs.Push(tab.Url);
        Tabs.Remove(tab);
        LoggingService.Info($"[VM] CloseTab: {tab.Title}. Removed at index {index}. Remaining: {Tabs.Count}");

        if (Tabs.Count == 0) { AddTab(); return; }
        if (index >= Tabs.Count) index = Tabs.Count - 1;
        SelectedTab = Tabs[index];
    }

    [RelayCommand]
    private void ReopenClosedTab()
    {
        if (_closedTabs.Any())
        {
            string url = _closedTabs.Pop();
            LoggingService.Info($"[VM] ReopenClosedTab: {url}");
            AddTab();
        }
    }

    [RelayCommand]
    private void DuplicateTab(TabViewModel? tab)
    {
        if (tab != null)
        {
            var t = new TabViewModel { Id = Guid.NewGuid(), Title = tab.Title + " (Copy)", Url = tab.Url };
            Tabs.Add(t);
            SelectedTab = t;
            LoggingService.Info($"[VM] DuplicateTab: {tab.Title}");
        }
    }

    [RelayCommand] private void PinTab(TabViewModel? tab) { LoggingService.Info($"[VM] PinTab stub: {tab?.Title}"); }

    [RelayCommand]
    private void CloseOtherTabs(TabViewModel? tab)
    {
        if (tab == null) return;
        LoggingService.Info($"[VM] CloseOtherTabs. Keeping: {tab.Title}");
        Tabs.Clear();
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private void CloseTabsToRight(TabViewModel? tab)
    {
        if (tab == null) return;
        int idx = Tabs.IndexOf(tab);
        int removed = 0;
        for (int i = Tabs.Count - 1; i > idx; i--) { Tabs.RemoveAt(i); removed++; }
        LoggingService.Info($"[VM] CloseTabsToRight. Removed: {removed}");
    }

    [RelayCommand]
    private void NavigateOmnibox()
    {
        string text = OmniboxText.Trim();
        LoggingService.Info($"[VM] NavigateOmnibox. Raw input: '{text}'");
        if (string.IsNullOrEmpty(text)) return;

        string url = text;
        if (!text.StartsWith("http://") && !text.StartsWith("https://") && !text.Contains("."))
            url = $"https://www.google.com/search?q={Uri.EscapeDataString(text)}";
        else if (!text.StartsWith("http"))
            url = "https://" + text;

        LoggingService.Info($"[VM] NavigateOmnibox. Resolved URL: {url}");
        NavigationRequested?.Invoke(url);
    }

    [RelayCommand]
    private void GoHome()
    {
        LoggingService.Info("[VM] GoHome triggered.");
        OmniboxText = "https://www.google.com";
        NavigationRequested?.Invoke("https://www.google.com");
    }

    [RelayCommand]
    private void NavigateToUrl(string url)
    {
        LoggingService.Info($"[VM] NavigateToUrl: {url}");
        OmniboxText = url;
        NavigationRequested?.Invoke(url);
    }

    public void UpdateNavigationState(bool canGoBack, bool canGoForward)
    {
        CanGoBack = canGoBack;
        CanGoForward = canGoForward;
    }

    public void NextTab()
    {
        if (SelectedTab != null)
        {
            int i = Tabs.IndexOf(SelectedTab);
            SelectedTab = Tabs[(i + 1) % Tabs.Count];
            LoggingService.Info($"[VM] NextTab -> {SelectedTab.Title}");
        }
    }
    public void PreviousTab()
    {
        if (SelectedTab != null)
        {
            int i = Tabs.IndexOf(SelectedTab);
            SelectedTab = Tabs[(i - 1 + Tabs.Count) % Tabs.Count];
            LoggingService.Info($"[VM] PreviousTab -> {SelectedTab.Title}");
        }
    }
    public void SwitchToTab(int index)
    {
        if (index >= 0 && index < Tabs.Count)
        {
            SelectedTab = Tabs[index];
            LoggingService.Info($"[VM] SwitchToTab({index}) -> {SelectedTab.Title}");
        }
    }

    public void TriggerFocusOmnibox()
    {
        LoggingService.Info("[VM] TriggerFocusOmnibox");
        FocusOmniboxRequested?.Invoke();
    }
    public void TriggerToggleFullscreen()
    {
        LoggingService.Info("[VM] TriggerToggleFullscreen");
        ToggleFullscreenRequested?.Invoke();
    }
    public void TriggerOpenDevTools()
    {
        LoggingService.Info("[VM] TriggerOpenDevTools");
        OpenDevToolsRequested?.Invoke();
    }

    partial void OnSelectedTabChanging(TabViewModel? value)
    {
        if (value != null) OmniboxText = value.Url;
    }

    public void TileSelection(IEnumerable<TabViewModel> selection, TilingLayout layout)
    {
        var tabs = selection.Take(2).ToList();
        if (tabs.Count < 2) { LoggingService.Warning("[VM] TileSelection: less than 2 tabs."); return; }

        TiledTabs.Clear();
        foreach (var t in tabs) TiledTabs.Add(t);

        CurrentTilingLayout = layout;
        LoggingService.Info($"[VM] TileSelection. Layout: {layout}. Tabs: {string.Join(", ", tabs.Select(t => t.Title))}");
        TilingTabsChanged?.Invoke(TiledTabs);
        TilingLayoutChanged?.Invoke(layout);
    }

    [RelayCommand]
    private void UntileTabs()
    {
        LoggingService.Info("[VM] UntileTabs");
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
            LoggingService.Info($"[VM] SwitchTilingLayout -> {layout}");
            CurrentTilingLayout = layout;
            TilingLayoutChanged?.Invoke(layout);
        }
    }
}
