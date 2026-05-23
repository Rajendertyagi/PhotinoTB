using System;
using System.Collections.Generic;
using System.Linq;

namespace TB_Browser.Infrastructure;

/// <summary>
/// Fuzzy string matching using Levenshtein distance.
/// Filters results where distance is within the threshold (default 2).
/// </summary>
public static class FuzzyMatcher
{
    public static List<string> Filter(IEnumerable<string> items, string query, int threshold = 2)
    {
        if (string.IsNullOrWhiteSpace(query)) return items.ToList();
        
        var results = new List<string>();
        foreach (var item in items)
        {
            if (CalculateDistance(item.ToLowerInvariant(), query.ToLowerInvariant()) <= threshold)
                results.Add(item);
        }
        return results;
    }

    private static int CalculateDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }
}
