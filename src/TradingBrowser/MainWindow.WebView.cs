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
            // FIX 1: Bypassed CreateAsync overload issues by using the default environment
            await MainWebView.EnsureCoreWebView2Async();
            _isWebViewInitialized = true;
            LoggingService.Info($"InitializeWebViewAsync: SUCCESS. CoreWebView2 ready. Runtime: {MainWebView.CoreWebView2.Environment.BrowserVersionString}");

            // --- WEBVIEW2 TELEMETRY HOOKS ---
            MainWebView.CoreWebView2.DocumentTitleChanged += (s, e) =>
            {
                string title = MainWebView.CoreWebView2.DocumentTitle;
                LoggingService.Info($"[WebView] DocumentTitle changed: {title}");
                if (ViewModel.SelectedTab != null) ViewModel.SelectedTab.Title = title;
            };

            MainWebView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                LoggingService.Info($"[WebView] NavigationStarting -> {e.Uri} (Initiator: {e.NavigationKind})");
                if (ViewModel.SelectedTab != null) ViewModel.SelectedTab.Url = e.Uri;
            };

            MainWebView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                // FIX 2: Read the URL from the WebView Source, since 'e' does not contain 'Uri'
                string currentUri = MainWebView.CoreWebView2.Source;
                if (e.IsSuccess)
                {
                    LoggingService.Info($"[WebView] NavigationCompleted SUCCESS -> {currentUri}");
                }
                else
                {
                    LoggingService.Error($"[WebView] NavigationCompleted FAILED -> {currentUri} | Error: {e.WebErrorStatus}");
                }
            };

            MainWebView.CoreWebView2.ProcessFailed += (s, e) =>
            {
                LoggingService.Error($"[WebView] PROCESS FAILED | Kind: {e.ProcessFailedKind} | Reason: {e.ExitCode}");
            };

            MainWebView.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                LoggingService.Info($"[WebView] JS Bridge Message: {e.TryGetWebMessageAsString()}");
            };

            // --- JS ERROR CATCHER ---
            string jsErrorCatcher = @"
                window.addEventListener('error', function(e) {
                    window.chrome.webview.postMessage('JS_ERROR: ' + e.message + ' @ ' + e.filename + ':' + e.lineno);
                });
                window.addEventListener('unhandledrejection', function(e) {
                    window.chrome.webview.postMessage('PROMISE_ERROR: ' + e.reason);
                });
            ";
            await MainWebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(jsErrorCatcher);
            LoggingService.Info("[WebView] JS error catcher injected.");

            MainWebView.CoreWebView2.Navigate("https://www.google.com");
            LoggingService.Info("[WebView] Initial navigation to google.com triggered.");
        }
        catch (Exception ex)
        {
            LoggingService.Error("InitializeWebViewAsync: FATAL FAILURE", ex);
            _isWebViewInitialized = false;
        }
    }
}
