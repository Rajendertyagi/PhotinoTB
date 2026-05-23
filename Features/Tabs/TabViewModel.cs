using CommunityToolkit.Mvvm.ComponentModel;
namespace TB.Features.Tabs;
public partial class TabViewModel : ObservableObject
{
    private readonly TabService _service;
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _title = "New Tab";
    public TabViewModel(string url, string title, TabService service) => (_url, _title, _service) = (url, title, service);
    public void Close() => _service.Remove(this);
}
