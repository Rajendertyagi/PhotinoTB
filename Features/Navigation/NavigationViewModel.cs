using CommunityToolkit.Mvvm.ComponentModel;
namespace TB.Features.Navigation;
public partial class NavigationViewModel : ObservableObject
{
    [ObservableProperty] private string _addressBarText = string.Empty;
}
