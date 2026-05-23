using System;
using TB_Browser.Infrastructure;
using TB_Browser.Models;

namespace TB_Browser.Services;

/// <summary>
/// Manages search provider URLs and privacy headers (Do Not Track).
/// Integrates with SettingsService for user-configurable engines.
/// </summary>
public class SearchService
{
    private readonly SettingsService _settingsService;

    public SearchService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Builds a complete search URL based on the configured provider and query.
    /// Supports OpenSearch-style {searchTerms} placeholder.
    /// </summary>
    public string BuildSearchUrl(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _settingsService.Settings.SearchProvider;

        var provider = _settingsService.Settings.SearchProvider;
        var encodedQuery = Uri.EscapeDataString(query);

        return provider.Contains("{searchTerms}", StringComparison.OrdinalIgnoreCase)
            ? provider.Replace("{searchTerms}", encodedQuery, StringComparison.OrdinalIgnoreCase)
            : $"{provider}?q={encodedQuery}";
    }

    /// <summary>
    /// Indicates whether Do Not Track (DNT) should be sent with requests.
    /// WebView2 header injection will be handled in WebResourceRequested later.
    /// </summary>
    public bool IsDoNotTrackEnabled()
    {
        // Aligns with locked spec: "Respect Do Not Track header on searches? Yes"
        return true;
    }
}
