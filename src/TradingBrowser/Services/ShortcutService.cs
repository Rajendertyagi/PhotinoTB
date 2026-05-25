using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using TradingBrowser.ViewModels;
using Windows.System;
using Microsoft.UI.Input;
using Windows.UI.Core;

namespace TradingBrowser.Services;

public class ShortcutService
{
    private readonly MainViewModel _viewModel;
    private readonly Func<CoreWebView2?> _getCoreWebView;

    public ShortcutService(MainViewModel viewModel, Func<CoreWebView2?> getCoreWebView)
    {
        _viewModel = viewModel;
        _getCoreWebView = getCoreWebView;
    }

    // Handles keyboard shortcuts when the WinUI UI (Omnibox, Tabs) has focus
    public void HandleUiKeyDown(KeyRoutedEventArgs e)
    {
        bool ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
        bool shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
        
        if (ProcessKey(ctrl, shift, e.Key.ToString()))
        {
            e.Handled = true;
        }
    }

    // Handles mouse back/forward buttons
    public void HandlePointerPressed(PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;
        var coreWebView = _getCoreWebView();
        if (coreWebView == null) return;

        if (props.IsXButton1Pressed && coreWebView.CanGoBack) coreWebView.GoBack();
        else if (props.IsXButton2Pressed && coreWebView.CanGoForward) coreWebView.GoForward();
    }

    // Handles shortcuts forwarded from the WebView2 JS injector
    public void HandleWebViewMessage(string message)
    {
        if (!message.StartsWith("SHORTCUT:")) return;

        string key = message.Replace("SHORTCUT:", "");
        bool ctrl = key.StartsWith("CTRL_");
        bool shift = key.Contains("SHIFT_");
        
        // Normalize JS key strings to match ProcessKey expectations
        string normalizedKey = key.Replace("CTRL_", "").Replace("SHIFT_", "");
        if (key == "CTRL_TAB") normalizedKey = "Tab";
        if (key == "CTRL_SHIFT_TAB") { normalizedKey = "Tab"; shift = true; ctrl = true; }
        if (key.StartsWith("CTRL_NUM_")) normalizedKey = key[^1..]; 

        ProcessKey(ctrl, shift, normalizedKey);
    }

    // Centralized routing logic
    private bool ProcessKey(bool ctrl, bool shift, string key)
    {
        var coreWebView = _getCoreWebView();

        if (ctrl && shift && key == "T") { _viewModel.ReopenClosedTabCommand.Execute(null); return true; }
        if (ctrl && key == "T") { _viewModel.AddTabCommand.Execute(null); return true; }
        if (ctrl && key == "W") { _viewModel.CloseTabCommand.Execute(null); return true; }
        if (ctrl && key == "L") { _viewModel.TriggerFocusOmnibox(); return true; }
        if (ctrl && key == "Tab") { if (shift) _viewModel.PreviousTab(); else _viewModel.NextTab(); return true; }
        if (key == "F11") { _viewModel.TriggerToggleFullscreen(); return true; }
        if (key == "F12") { _viewModel.TriggerOpenDevTools(); return true; }
        if (key == "F5") { coreWebView?.Reload(); return true; }
        
        if (ctrl && int.TryParse(key, out int num)) 
        { 
            _viewModel.SwitchToTab(num == 9 ? _viewModel.Tabs.Count - 1 : num - 1); 
            return true; 
        }

        return false;
    }
}
