using CommunityToolkit.Mvvm.ComponentModel;

namespace TradingBrowser.ViewModels;

public partial class TabViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = "New Tab";

    [ObservableProperty]
    public partial string Url { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsPinned { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool CanGoBack { get; set; }

    [ObservableProperty]
    public partial bool CanGoForward { get; set; }
}
