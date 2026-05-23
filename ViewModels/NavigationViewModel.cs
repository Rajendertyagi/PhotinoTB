using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TB_Browser.Infrastructure;
using TB_Browser.Models;
using TB_Browser.Repositories;
using TB_Browser.Services;

namespace TB_Browser.ViewModels;

public partial class NavigationViewModel : ObservableObject
{
    private readonly HistoryRepository _historyRepo;
    private readonly FuzzyMatcher _fuzzyMatcher = new();

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
        // ✅ FIX CS8620: Filter nulls/empty before passing to FuzzyMatcher
        var items = history
            .Select(h => h.Url)
            .Where(u => !string.IsNullOrEmpty(u))
            .ToList();

        Suggestions = _fuzzyMatcher.Filter(items, query, threshold: 2);
    }
}
