using System;
using System.Collections.Generic;
using System.Linq;

namespace TB_Browser.Infrastructure;

/// <summary>
/// Static utility for fuzzy string matching using Levenshtein distance.
/// Thread-safe, allocation-optimized for autocomplete scenarios.
/// </summary>
public static class FuzzyMatcher
{
    /// <summary>
    /// Filters items by fuzzy match against a query.
    /// Returns items where Levenshtein distance <= threshold, sorted by relevance.
    /// </summary>
    /// <param name="items">Candidate strings to filter</param>
    /// <param name="query">User input to match against</param>
    /// <param name="threshold">Max allowed edit distance (default: 2)</param>
    /// <returns>Filtered list, sorted by ascending distance (best matches first)</returns>
    public static List<string> Filter(IEnumerable<string> items, string query, int threshold = 2)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<string>();

        var results = new List<(string Item, int Distance)>();
        var queryLower = query.ToLowerInvariant();

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item)) continue;

            // Fast path: exact or startswith match (zero cost)
            var itemLower = item.ToLowerInvariant();
            if (itemLower == queryLower || itemLower.StartsWith(queryLower))
            {
                results.Add((item, 0));
                continue;
            }

            var distance = LevenshteinDistance(queryLower, itemLower);
            if (distance <= threshold)
                results.Add((item, distance));
        }

        // Sort by distance (best first), then alphabetically for ties
        return results
            .OrderBy(r => r.Distance)
            .ThenBy(r => r.Item, StringComparer.OrdinalIgnoreCase)
            .Select(r => r.Item)
            .ToList();
    }

    /// <summary>
    /// Computes Levenshtein edit distance between two strings.
    /// Uses O(n) space optimization for performance.
    /// </summary>
    private static int LevenshteinDistance(string source, string target)
    {
        if (source == target) return 0;
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        // Ensure source is the shorter string for space optimization
        if (source.Length > target.Length)
            (source, target) = (target, source);

        var previousRow = new int[target.Length + 1];
        var currentRow = new int[target.Length + 1];

        // Initialize first row
        for (int j = 0; j <= target.Length; j++)
            previousRow[j] = j;

        for (int i = 1; i <= source.Length; i++)
        {
            currentRow[0] = i;

            for (int j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                currentRow[j] = Math.Min(
                    Math.Min(
                        previousRow[j] + 1,      // Deletion
                        currentRow[j - 1] + 1),  // Insertion
                    previousRow[j - 1] + cost    // Substitution
                );
            }

            // Swap rows for next iteration
            (previousRow, currentRow) = (currentRow, previousRow);
        }

        return previousRow[target.Length];
    }
}
