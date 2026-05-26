using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using TradingBrowser.Services;
using TradingBrowser.ViewModels;

namespace TradingBrowser;

public sealed partial class MainWindow
{
    private TabViewModel? _secondaryTab;

    private void TileTabs(TabViewModel primary, TabViewModel secondary)
    {
        ViewModel.SelectedTab = primary; // Loads into MainWebView
        _secondaryTab = secondary;
        SplitPane(secondary.Url);
    }

    private async void SplitPane(string? url = null)
    {
        if (_isSplitPaneActive) return;
        _isSplitPaneActive = true;

        LeftPaneColumn.Width = new GridLength(1, GridUnitType.Star);
        RightPaneColumn.Width = new GridLength(1, GridUnitType.Star);
        PaneDivider.Visibility = Visibility.Visible;
        RightPaneHost.Visibility = Visibility.Visible;

        try
        {
            await SecondaryWebView.EnsureCoreWebView2Async();
            
            // Sync secondary WebView back to the TabViewModel
            SecondaryWebView.CoreWebView2.DocumentTitleChanged += (s, e) => {
                if (_secondaryTab != null) _secondaryTab.Title = SecondaryWebView.CoreWebView2.DocumentTitle;
            };
            SecondaryWebView.CoreWebView2.NavigationStarting += (s, e) => {
                if (_secondaryTab != null) _secondaryTab.Url = e.Uri;
            };

            SecondaryWebView.CoreWebView2.Navigate(url ?? "https://www.tradingview.com");
            LoggingService.Log("Secondary WebView initialized (Tiled).");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Secondary WebView Init Error", ex);
        }
    }

    private void CollapsePane()
    {
        if (!_isSplitPaneActive) return;
        _isSplitPaneActive = false;
        _secondaryTab = null;

        RightPaneColumn.Width = new GridLength(0);
        PaneDivider.Visibility = Visibility.Collapsed;
        RightPaneHost.Visibility = Visibility.Collapsed;
        
        ViewModel.UntileTabsCommand.Execute(null);
    }

    private void SplitPane_Click(object sender, RoutedEventArgs e) => SplitPane();
    private void CollapsePane_Click(object sender, RoutedEventArgs e) => CollapsePane();

    private void PaneDivider_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double newLeftWidth = LeftPaneColumn.Width.Value + e.HorizontalChange;
        double totalWidth = WebViewHost.ActualWidth - 4; 
        
        if (newLeftWidth > 200 && (totalWidth - newLeftWidth) > 200)
        {
            LeftPaneColumn.Width = new GridLength(newLeftWidth, GridUnitType.Pixel);
            RightPaneColumn.Width = new GridLength(totalWidth - newLeftWidth, GridUnitType.Pixel);
        }
    }
}
