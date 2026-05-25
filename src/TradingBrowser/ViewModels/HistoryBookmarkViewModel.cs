using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TradingBrowser.ViewModels;

/// <summary>
/// ViewModel for the History/Bookmark Sidebar UI.
/// Exposes collections to bind to ListView controls in the SplitView pane.
/// </summary>
public partial class HistoryBookmarkViewModel : ObservableObject
{
    /// <summary>
    /// Collection of items to display in the Bookmarks tab of the sidebar.
    /// Bound directly to BookmarkListView.ItemsSource in MainWindow.xaml.
    /// </summary>
    public ObservableCollection<(string Url, string Title)> Bookmarks { get; } = [];
    
    /// <summary>
    /// Collection of items to display in the History tab of the sidebar.
    /// Bound directly to HistoryListView.ItemsSource in MainWindow.xaml.
    /// </summary>
    public ObservableCollection<(string Url, string Title)> History { get; } = [];

    /// <summary>
    /// Updates the sidebar lists with fresh data from the SQLite database.
    /// Clears existing items and repopulates to ensure UI reflects current state.
    /// </summary>
    /// <param name="bookmarks">Latest bookmarks from HistoryBookmarkService.</param>
    /// <param name="history">Latest history entries from HistoryBookmarkService.</param>
    public void LoadData(System.Collections.Generic.List<(string Url, string Title)> bookmarks, System.Collections.Generic.List<(string Url, string Title)> history)
    {
        // Clear current UI lists before repopulating to prevent duplicates
        Bookmarks.Clear();
        History.Clear();
        
        foreach (var item in bookmarks) Bookmarks.Add(item);
        foreach (var item in history) History.Add(item);
    }
}
