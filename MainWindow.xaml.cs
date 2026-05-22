using Microsoft.UI.Xaml;
using System;

namespace TB_Browser;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Add initial tab
        TabView.TabItems.Add(new Microsoft.UI.Xaml.Controls.TabViewItem { Header = "New Tab" });
        TabView.SelectedIndex = 0;
        Navigate("https://www.google.com");
    }

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

    private void Navigate(string url)
    {
        WebView.Source = new Uri(url);
    }
}
