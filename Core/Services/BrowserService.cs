using System;
using Microsoft.Web.WebView2.Core;
using TB_Browser.Core.Logging;

namespace TB_Browser.Core.Services
{
    public class BrowserService : IBrowserService
    {
        private readonly ILogger _logger;
        public ITabService TabService { get; set; } = null!;
        public event EventHandler<string>? UrlChanged;
        private CoreWebView2? _webView;

        public BrowserService(ILogger logger) => _logger = logger;

        public void SetWebView(CoreWebView2 webView)
        {
            _webView = webView;
            _webView.SourceChanged += (s, e) =>
            {
                if (TabService.ActiveTab != null)
                {
                    TabService.UpdateTab(TabService.ActiveTab.Id, _webView.Source, _webView.DocumentTitle);
                    UrlChanged?.Invoke(this, _webView.Source);
                    _logger.Debug("BrowserService", $"Source changed: {_webView.Source}");
                }
            };
            _webView.NavigationStarting += (s, e) => _logger.Info("BrowserService", $"Navigating: {e.Uri}");
            
            _webView.NavigationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                    _logger.Info("BrowserService", $"Loaded: {e.Uri}");
                else
                    _logger.Warning("BrowserService", $"Failed: {e.Uri} ({e.WebErrorStatus})");
            };
        }

        public void Navigate(string url)
        {
            try
            {
                if (!url.StartsWith("http")) url = "https://" + url;
                _webView?.Navigate(url);
                _logger.Info("BrowserService", $"Navigate: {url}");
            }
            catch (Exception ex)
            {
                _logger.Error("BrowserService", $"Navigate error: {url}", ex);
            }
        }

        public void GoBack()
        {
            try { _webView?.GoBack(); }
            catch (Exception ex) { _logger.Warning("BrowserService", $"Back failed: {ex.Message}"); }
        }
        public void GoForward()
        {
            try { _webView?.GoForward(); }
            catch (Exception ex) { _logger.Warning("BrowserService", $"Forward failed: {ex.Message}"); }
        }
        public void Reload()
        {
            try { _webView?.Reload(); }
            catch (Exception ex) { _logger.Warning("BrowserService", $"Reload failed: {ex.Message}"); }
        }
    }
}
