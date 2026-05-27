using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using TradingBrowser.Services;
using TradingBrowser.ViewModels;
using System;
using System.Threading.Tasks;

namespace TradingBrowser;

public sealed partial class MainWindow
{
    private async Task InitializeWebViewAsync()
    {
        LoggingService.Info("InitializeWebViewAsync: Starting...");
        try
        {
            await MainWebView.EnsureCoreWebView2Async();
            _isWebViewInitialized = true;
            LoggingService.Info("InitializeWebViewAsync: SUCCESS. _isWebViewInitialized set to TRUE.");
            
            MainWebView.CoreWebView2.DocumentTitleChanged += (s, e) => 
            {
                if (ViewModel.SelectedTab != null)
                    ViewModel.SelectedTab.Title = MainWebView.CoreWebView2.DocumentTitle;
            };
            
            MainWebView.CoreWebView2.NavigationStarting += (s, e) => 
            {
                if (ViewModel.SelectedTab != null)
                    ViewModel.SelectedTab.Url = e.Uri;
            };

            MainWebView.CoreWebView2.Navigate("https://www.google.com");
        }
        catch (Exception ex)
        {
            LoggingService.Error("InitializeWebViewAsync: FATAL FAILURE", ex);
            _isWebViewInitialized = false;
        }
    }
}
