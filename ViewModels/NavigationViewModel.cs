using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB.Services;

namespace TB.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private readonly WebViewService _webView;
    [ObservableProperty] private string _addressBarText = string.Empty;

    public NavigationViewModel(WebViewService webView) => _webView = webView;

    [RelayCommand] private void Back() => _webView.CoreWebView2?.GoBack();
    [RelayCommand] private void Forward() => _webView.CoreWebView2?.GoForward();
    [RelayCommand] private void Reload() => _webView.CoreWebView2?.Reload();
}
