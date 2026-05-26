using Microsoft.Web.WebView2.Core;
using TradingBrowser.Helpers;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;

namespace TradingBrowser.Services;

public class WebViewNavigationService
{
    private readonly DownloadService _downloadService;
    private readonly CoreWebView2 _webView;
    private readonly Window _window;

    public WebViewNavigationService(DownloadService downloadService, CoreWebView2 webView, Window window)
    {
        _downloadService = downloadService;
        _webView = webView;
        _window = window;
    }

    public bool HandleSpecialUri(string uri)
    {
        if (uri == "about:downloads")
        {
            _webView.NavigateToString(DownloadPageGenerator.GenerateHtml(_downloadService.GetHistory()));
            return true;
        }

        if (uri == "about:settings")
        {
            string engine = SettingsService.Get("SearchEngine", "Google");
            bool suggestions = SettingsService.Get("ShowSuggestions", "true") == "true";
            bool fullUrls = SettingsService.Get("ShowFullUrls", "false") == "true";
            bool httpsWarn = SettingsService.Get("HttpsWarning", "false") == "true";
            string dlPath = SettingsService.Get("DownloadPath", System.IO.Path.Combine(AppContext.BaseDirectory, "Downloads"));
            bool askSave = SettingsService.Get("AskBeforeSave", "false") == "true";
            bool restore = SettingsService.Get("RestoreSession", "true") == "true";

            // Format options for HTML select tags
            string googleOpt = engine == "Google" ? "selected" : "";
            string bingOpt = engine == "Bing" ? "selected" : "";
            string ddgOpt = engine == "DuckDuckGo" ? "selected" : "";
            string suggestionsChecked = suggestions ? "checked" : "";
            string fullUrlsChecked = fullUrls ? "checked" : "";
            string httpsChecked = httpsWarn ? "checked" : "";
            string askChecked = askSave ? "checked" : "";
            string restoreChecked = restore ? "checked" : "";

            string html = SettingsPageGenerator.GenerateHtml(
                engine, suggestions, fullUrls, httpsWarn, false,
                dlPath, askSave, restore.ToString().ToLower(), "");
            
            // Inject state into HTML
            html = string.Format(html, googleOpt, bingOpt, ddgOpt, suggestionsChecked, fullUrlsChecked, httpsChecked, dlPath, askChecked, restoreChecked);
            
            _webView.NavigateToString(html);
            return true;
        }

        return false;
    }

    public async Task HandleWebMessageAsync(string message)
    {
        if (message.StartsWith("REMOVE_DOWNLOAD:"))
        {
            int id = int.Parse(message.Replace("REMOVE_DOWNLOAD:", ""));
            _downloadService.DeleteDownload(id);
            HandleSpecialUri("about:downloads");
        }
        else if (message == "CLEAR_ALL_DOWNLOADS")
        {
            _downloadService.ClearAllDownloads();
            HandleSpecialUri("about:downloads");
        }
        else if (message == "CLEAR_ALL_DATA")
        {
            await ClearBrowsingDataAsync();
        }
        else if (message == "CHANGE_DOWNLOAD_PATH")
        {
            await ChangeDownloadPathAsync();
        }
        else if (message == "RESTART_APP")
        {
            // Graceful restart: close window, OS will relaunch if configured, or user can manually restart
            _window.Close();
        }
        else if (message.StartsWith("SETTING_UPDATE:"))
        {
            string[] parts = message.Split(':', 3);
            if (parts.Length == 3)
            {
                string key = parts[1];
                string value = parts[2];
                SettingsService.Set(key, value);

                // Update download service immediately if path changed
                if (key == "DownloadPath")
                {
                    _downloadService.UpdateDownloadPath(value);
                }
            }
        }
    }

    private async Task ClearBrowsingDataAsync()
    {
        try
        {
            string profile = System.IO.Path.Combine(AppContext.BaseDirectory, "UserData", "Profile");
            if (System.IO.Directory.Exists(profile))
            {
                // Delete files recursively, ignoring locked files
                foreach (var dir in System.IO.Directory.GetDirectories(profile, "*", System.IO.SearchOption.AllDirectories))
                {
                    try { System.IO.Directory.Delete(dir, true); } catch { }
                }
                foreach (var file in System.IO.Directory.GetFiles(profile, "*", System.IO.SearchOption.AllDirectories))
                {
                    try { System.IO.File.Delete(file); } catch { }
                }
            }
            LoggingService.Log("Browsing data cleared.");
        }
        catch (Exception ex) { LoggingService.Error("Failed to clear data", ex); }
    }

    private async Task ChangeDownloadPathAsync()
    {
        try
        {
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeFilter.Add("*");
            
            // WinUI 3 FolderPicker requires initialization on the current window
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hWnd);
            
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                string newPath = folder.Path;
                SettingsService.Set("DownloadPath", newPath);
                _downloadService.UpdateDownloadPath(newPath);
                
                // Refresh settings page to show new path
                HandleSpecialUri("about:settings");
            }
        }
        catch (Exception ex) { LoggingService.Error("Failed to pick download folder", ex); }
    }
}
