using System;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;
using TB_Browser.Core.Logging;
using TB_Browser.Core.Models;
using TB_Browser.Core.Services;

namespace TB_Browser.UI.Controls
{
    public partial class BrowserView : UserControl
    {
        private readonly IBrowserService _svc;
        private WebView2? _currentWebView;
        private Action<bool>? _progressHandler;
        private Action<string>? _statusHandler;
        private double _zoom = 1.0;

        public BrowserView(IBrowserService svc)
        {
            InitializeComponent();
            _svc = svc;
            Logger.Debug("BrowserView", "Control initialized");
            
            MouseWheel += (_, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control && _currentWebView != null)
                {
                    _zoom += e.Delta > 0 ? 0.1 : -0.1;
                    _currentWebView.ZoomFactor = Math.Max(0.25, Math.Min(5.0, _zoom));
                    Logger.Info("BrowserView", $"Zoom changed to {_zoom:F1}x");
                }
            };
        }

        public void SetProgressHandler(Action<bool> handler) => _progressHandler = handler;
        public void SetStatusHandler(Action<string> handler) => _statusHandler = handler;

        public async void SwitchTo(TabModel tab)
        {
            try
            {
                Logger.Info("BrowserView", $"Switching to tab #{tab.Id}: {tab.Url}");
                
                Logger.Debug("BrowserView", "Creating WebView2 instance...");
                var wv = new WebView2();
                _currentWebView = wv;
                WebViewHost.Content = wv;
                
                Logger.Debug("BrowserView", "Initializing CoreWebView2...");
                await wv.EnsureCoreWebView2Async();
                Logger.Info("BrowserView", "CoreWebView2 initialized successfully");
                
                Logger.Debug("BrowserView", "Setting up WebView2 event handlers...");
                _svc.SetWebView(wv.CoreWebView2!);
                
                _svc.IsLoadingChanged += (_, l) => 
                {
                    Logger.Debug("BrowserView", $"Loading state: {l}");
                    _progressHandler?.Invoke(l);
                };
                
                wv.CoreWebView2.StatusBarTextChanged += (_, e) => 
                {
                    var status = wv.CoreWebView2.StatusBarText;
                    Logger.Debug("BrowserView", $"Status: {status}");
                    _statusHandler?.Invoke(status);
                };
                
                Logger.Info("BrowserView", $"Navigating to: {tab.Url}");
                wv.Source = new Uri(tab.Url);
                
                Logger.Info("BrowserView", $"Tab #{tab.Id} switch complete");
            }
            catch (Exception ex)
            {
                Logger.Error("BrowserView", $"Failed to switch to tab #{tab?.Id ?? 0}: {ex.Message}");
                Logger.Error("BrowserView", $"Stack: {ex.StackTrace}");
            }
        }
        
        public void Reload()
        {
            Logger.Info("BrowserView", "Reload requested");
            _currentWebView?.Reload();
        }
        
        public void GoBack()
        {
            Logger.Info("BrowserView", "Back requested");
            _currentWebView?.GoBack();
        }
        
        public void GoForward()
        {
            Logger.Info("BrowserView", "Forward requested");
            _currentWebView?.GoForward();
        }
    }
}
