using System;
using Microsoft.Web.WebView2.Core;

namespace TB_Browser.Core.Services
{
    public interface IBrowserService
    {
        ITabService TabService { get; set; }
        void SetWebView(CoreWebView2 webView); // ✅ ADD THIS
        void Navigate(string url);
        void GoBack();
        void GoForward();
        void Reload();
        event EventHandler<string> UrlChanged;
    }
}
