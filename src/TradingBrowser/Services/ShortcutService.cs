using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using TradingBrowser.ViewModels;
using Windows.System;
using Microsoft.UI.Input;
using Windows.UI.Core;

namespace TradingBrowser.Services;

/// <summary>
/// Encapsulates logic for handling keyboard shortcuts, mouse navigation buttons,
/// and JavaScript-injected shortcuts from the WebView2 control.
/// </summary>
public class ShortcutService
{
    private readonly MainViewModel _viewModel;
    private readonly Func<CoreWebView2?> _getCoreWebView;

    public ShortcutService(MainViewModel viewModel, Func<CoreWebView2?> getCoreWebView)
    {
        _viewModel = viewModel;
        _getCoreWebView = getCoreWebView;
    }

    /// <summary>
    /// Event triggered when the user presses Ctrl+D to bookmark the current page.
    /// </summary>
    public event Action? BookmarkRequested;

    /// <summary>
    /// Handles keyboard shortcuts when the WinUI UI (Omnibox, Tabs) has focus.
    /// </summary>
    /// <param name="e">The key event arguments from the UI.</param>
    public void HandleUiKeyDown(KeyRoutedEventArgs e)
    {
        bool ctrl = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
        bool shift = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
        
        // If the shortcut is handled, mark the event as Handled to prevent default behavior
        if (ProcessKey(ctrl, shift, e.Key.ToString()))
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Handles mouse back/forward buttons (Mouse Button 4 and 5).
    /// </summary>
    public void HandlePointerPressed(PointerRoutedEventArgs e)
    {
        var props = e.GetCurrentPoint(null).Properties;
        var coreWebView = _getCoreWebView();
        if (coreWebView == null) return;

        if (props.IsXButton1Pressed && coreWebView.CanGoBack) 
            coreWebView.GoBack(); // Mouse Button 4: Back
        else if (props.IsXButton2Pressed && coreWebView.CanGoForward) 
            coreWebView.GoForward(); // Mouse Button 5: Forward
    }

    /// <summary>
    /// Handles shortcuts forwarded from the WebView2 JS injector (e.g., when focus is inside a web page).
    /// </summary>
    /// <param name="message">The message string received from JS (e.g., "SHORTCUT:CTRL_W").</param>
    public void HandleWebViewMessage(string message)
    {
        if (!message.StartsWith("SHORTCUT:")) return;

        string key = message.Replace("SHORTCUT:", "");
        bool ctrl = key.StartsWith("CTRL_");
        bool shift = key.Contains("SHIFT_");
        
        // Normalize JS key strings to match the expected logic
        string normalizedKey = key.Replace("CTRL_", "").Replace("SHIFT_", "");
        if (key == "CTRL_TAB") normalizedKey = "Tab";
        if (key == "CTRL_SHIFT_TAB") { normalizedKey = "Tab"; shift = true; ctrl = true; }
        if (key.StartsWith("CTRL_NUM_")) normalizedKey = key[^1..]; 

        ProcessKey(ctrl, shift, normalizedKey);
    }

    /// <summary>
    /// Centralized routing logic that maps key combinations to actions.
    /// </summary>
    /// <returns>True if a shortcut was handled, false otherwise.</returns>
    private bool ProcessKey(bool ctrl, bool shift, string key)
    {
        var coreWebView = _getCoreWebView();

        // Bookmark Current Page (Ctrl+D)
        if (ctrl && key == "D") 
        { 
            BookmarkRequested?.Invoke(); 
            return true; 
        }

        // Reopen Closed Tab (Ctrl+Shift+T)
        if (ctrl && shift && key == "T") { _viewModel.ReopenClosedTabCommand.Execute(null); return true; }
        // New Tab (Ctrl+T)
        if (ctrl && key == "T") { _viewModel.AddTabCommand.Execute(null); return true; }
        // Close Tab (Ctrl+W)
        if (ctrl && key == "W") { _viewModel.CloseTabCommand.Execute(null); return true; }
        // Focus Omnibox (Ctrl+L)
        if (ctrl && key == "L") { _viewModel.TriggerFocusOmnibox(); return true; }
        // Cycle Tabs (Ctrl+Tab / Ctrl+Shift+Tab)
        if (ctrl && key == "Tab") { if (shift) _viewModel.PreviousTab(); else _viewModel.NextTab(); return true; }
        // Fullscreen (F11)
        if (key == "F11") { _viewModel.TriggerToggleFullscreen(); return true; }
        // Dev Tools (F12)
        if (key == "F12") { _viewModel.TriggerOpenDevTools(); return true; }
        // Reload (F5)
        if (key == "F5") { coreWebView?.Reload(); return true; }
        
        // Switch to specific tab (Ctrl+1 to Ctrl+8)
        if (ctrl && int.TryParse(key, out int num)) 
        { 
            _viewModel.SwitchToTab(num == 9 ? _viewModel.Tabs.Count - 1 : num - 1); 
            return true; 
        }

        return false;
    }
}
