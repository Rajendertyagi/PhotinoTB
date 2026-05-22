using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.Web.WebView2.Core;
using System;

namespace TB_Browser;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize with one tab
        TabView.TabItems.Add(new Microsoft.UI.Xaml.Controls.TabViewItem { Header = "New Tab" });
        TabView.SelectedIndex = 0;
        Navigate("https://www.google.com");
    }

    // --- Tab Logic ---
    private void TabView_AddTabButtonClick(Microsoft.UI.Xaml.Controls.TabView sender, object args)
    {
        var tab = new Microsoft.UI.Xaml.Controls.TabViewItem { Header = "New Tab" };
        sender.TabItems.Add(tab);
        sender.SelectedItem = tab;
        Navigate("https://www.google.com");
    }

    private void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
    {
        sender.TabItems.Remove(args.Tab);
        if (sender.TabItems.Count == 0)
        {
            var tab = new Microsoft.UI.Xaml.Controls.TabViewItem { Header = "New Tab" };
            sender.TabItems.Add(tab);
            sender.SelectedItem = tab;
            Navigate("https://www.google.com");
        }
    }

    // --- Navigation Logic ---
    private void Navigate(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            // Auto-add https:// if missing
            if (!url.StartsWith("http") && !url.StartsWith("file"))
                url = "https://" + url;
            
            try
            {
                WebView.Source = new Uri(url);
            }
            catch (Exception) { /* Invalid URL */ }
        }
    }

    // --- Button Events ---
    private void BackBtn_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CanGoBack) WebView.GoBack();
    }

    private void FwdBtn_Click(object sender, RoutedEventArgs e)
    {
        if (WebView.CanGoForward) WebView.GoForward();
    }

    private void RefBtn_Click(object sender, RoutedEventArgs e)
    {
        WebView.Reload();
    }

    private void HomeBtn_Click(object sender, RoutedEventArgs e)
    {
        Navigate("https://www.google.com");
    }

    private void GoBtn_Click(object sender, RoutedEventArgs e)
    {
        Navigate(UrlBox.Text);
    }

    private void UrlBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            Navigate(UrlBox.Text);
            UrlBox.SelectAll(); 
        }
    }

    // --- WebView Events (Fixed Signature) ---
    private void WebView_NavigationCompleted(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
    {
        if (WebView.Source != null)
        {
            UrlBox.Text = WebView.Source.AbsoluteUri;
            // Optional: Select all text on navigation finish
            // UrlBox.SelectAll(); 
        }
    }
}
