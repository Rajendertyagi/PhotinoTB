using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TB.Services;

namespace TB.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<TabViewModel> Tabs { get; } = new();
    [ObservableProperty] private TabViewModel? _selectedTab;

    private readonly TabStateManager _manager;
    public MainViewModel(TabStateManager manager) => _manager = manager;
}
