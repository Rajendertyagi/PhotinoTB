using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;

namespace MinimalBrowser;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, (string Url, string Title)> _tabData = new();
    private string? _currentTabId;
    private bool _isInitialized;

    public MainWindow()
    {
        InitializeComponent();
        TabView.SelectionChanged += TabView_SelectionChanged;
        WebView.CoreWebView2InitializationCompleted += WebView_InitializationCompleted;
        TabView_AddTabButtonClick(null, null);
    }

    private void WebView_InitializationCompleted(WebView2 sender, CoreWebView2InitializationCompletedEventArgs args)
    {
        _isInitialized = true;
        if (_currentTabId != null) NavigateToCurrentTab();
    }

    private void TabView_AddTabButtonClick(TabView sender, object? args)
    {
        var id = Guid.NewGuid().ToString();
        var tab = new TabViewItem { Header = "New Tab", Tag = id };
        _tabData[id] = ("https://www.google.com", "New Tab");
        sender.TabItems.Add(tab);
        sender.SelectedItem = tab;
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        var id = (string)args.Tab.Tag;
        _tabData.Remove(id);
        sender.TabItems.Remove(args.Tab);
        if (sender.TabItems.Count == 0) TabView_AddTabButtonClick(sender, null);
    }

    private void TabView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (TabView.SelectedItem is TabViewItem tab)
        {
            _currentTabId = (string)tab.Tag;
            if (_isInitialized) NavigateToCurrentTab();
        }
    }

    private void NavigateToCurrentTab()
    {
        if (_currentTabId == null) return;
        var data = _tabData[_currentTabId];
        UrlBox.Text = data.Url;
        ((TabViewItem)TabView.SelectedItem!).Header = data.Title;
        WebView.CoreWebView2?.Navigate(data.Url);
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e) => WebView.CoreWebView2?.GoBack();
    private void FwdBtn_Click(object sender, RoutedEventArgs e) => WebView.CoreWebView2?.GoForward();
    private void GoBtn_Click(object sender, RoutedEventArgs e) => Navigate();
    private void UrlBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter) Navigate();
    }

    private void Navigate()
    {
        var url = UrlBox.Text.Trim();
        if (!url.StartsWith("http")) url = "https://" + url;
        WebView.CoreWebView2?.Navigate(url);
        if (_currentTabId != null) _tabData[_currentTabId] = (url, "Loading...");
    }
}
