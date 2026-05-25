namespace TradingBrowser.ViewModels;

/// <summary>
/// Data class representing a single bookmark for UI binding.
/// </summary>
public class BookmarkItem
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
