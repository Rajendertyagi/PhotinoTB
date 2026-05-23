using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB_Browser.Infrastructure;
using TB_Browser.Models;
using TB_Browser.Repositories;

namespace TB_Browser.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private readonly HistoryRepository _historyRepo;

    [ObservableProperty] private string _addressBarText = string.Empty;
    [ObservableProperty] private List<string> _suggestions = new();

    public NavigationViewModel(HistoryRepository historyRepo) => _historyRepo = historyRepo;

    [RelayCommand]
    private async Task LoadSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Suggestions.Clear();
            return;
        }

        var history = await _historyRepo.GetRecentAsync(50);
        var items = history
            .Select(h => h.Url)
            .Where(u => !string.IsNullOrEmpty(u))
            .ToList();

        // ✅ FIX CS0723: Call static method directly, no 'new FuzzyMatcher()'
        Suggestions = FuzzyMatcher.Filter(items, query, threshold: 2);
    }
}
