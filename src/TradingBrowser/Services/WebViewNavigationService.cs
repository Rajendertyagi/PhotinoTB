using Microsoft.Web.WebView2.Core;
using TradingBrowser.Helpers;

namespace TradingBrowser.Services;

/// <summary>
/// Handles all WebView2 navigation logic including special URI routing.
/// </summary>
public class WebViewNavigationService
{
    private readonly DownloadService _downloadService;
    private readonly CoreWebView2 _webView;

    public WebViewNavigationService(DownloadService downloadService, CoreWebView2 webView)
    {
        _downloadService = downloadService;
        _webView = webView;
    }

    /// <summary>
    /// Handles navigation to special internal URIs.
    /// </summary>
    public bool HandleSpecialUri(string uri)
    {
        // Check for Downloads page
        if (uri == "about:downloads")
        {
            var records = _downloadService.GetHistory();
            string html = DownloadPageGenerator.GenerateHtml(records);
            _webView.NavigateToString(html);
            return true;
        }

        // Check for Settings page
        if (uri == "about:settings")
        {
            string html = SettingsPageGenerator.GenerateHtml();
            _webView.NavigateToString(html);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles messages received from web pages (like the downloads page).
    /// </summary>
    public void HandleWebMessage(string message)
    {
        if (message.StartsWith("REMOVE_DOWNLOAD:"))
        {
            int id = int.Parse(message.Replace("REMOVE_DOWNLOAD:", ""));
            _downloadService.DeleteDownload(id);
            // Reload downloads page
            HandleSpecialUri("about:downloads");
        }
        else if (message == "CLEAR_ALL_DOWNLOADS")
        {
            _downloadService.ClearAllDownloads();
            HandleSpecialUri("about:downloads");
        }
        else if (message.StartsWith("SETTING_UPDATE:"))
        {
            // Parse SETTING_UPDATE:Key:Value
            string[] parts = message.Split(':');
            if (parts.Length >= 3)
            {
                string key = parts[1];
                string value = parts[2];
                SettingsService.Set(key, value);
            }
        }
    }
}
