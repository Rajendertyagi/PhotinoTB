using System;
using Microsoft.Web.WebView2.Core;

namespace TB_Browser.Core.Services;

public class BrowserService
{
    public TabService TabService { get; set; } = null!;
    public event EventHandler<string>? UrlChanged;
    public event EventHandler<bool>? CanGoBackChanged;
    public event EventHandler<bool>? CanGoForwardChanged;

    private CoreWebView2? _webView;

    public void SetWebView(CoreWebView2 webView)
    {
        _webView = webView;
        _webView.SourceChanged += (_, _) =>
        {
            if (TabService.ActiveTab != null)
            {
                TabService.UpdateTab(TabService.ActiveTab.Id, _webView.Source.ToString(), _webView.DocumentTitle);
                UrlChanged?.Invoke(this, _webView.Source.ToString());
            }
        };
        _webView.HistoryChanged += (_, _) =>
        {
            CanGoBackChanged?.Invoke(this, _webView.CanGoBack);
            CanGoForwardChanged?.Invoke(this, _webView.CanGoForward);
        };
    }

    public void Navigate(string url)
    {
        if (!url.StartsWith("http")) url = "https://" + url;
        _webView?.Navigate(url);
    }

    public void GoBack() => _webView?.GoBack();
    public void GoForward() => _webView?.GoForward();
    public void Reload() => _webView?.Reload();
}
