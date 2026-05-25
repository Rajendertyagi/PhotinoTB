using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using TradingBrowser.Helpers;

namespace TradingBrowser.Services;

/// <summary>
/// Handles database operations for browsing history, bookmarks, and omnibox auto-completion.
/// </summary>
public class HistoryBookmarkService
{
    private readonly DatabaseService _db;

    public HistoryBookmarkService(DatabaseService db) => _db = db;

    /// <summary>
    /// Saves a visited URL to the browsing history table with a timestamp.
    /// </summary>
    public void AddHistory(string url, string title)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            // Uses INSERT OR IGNORE to prevent duplicate exact history entries
            cmd.CommandText = "INSERT OR IGNORE INTO History (Url, Title, VisitTime) VALUES (@url, @title, datetime('now'));";
            cmd.Parameters.AddWithValue("@url", url);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to add history entry", ex);
        }
    }

    /// <summary>
    /// Fetches history and bookmark suggestions for the Omnibox auto-complete dropdown.
    /// </summary>
    /// <param name="query">The partial text entered by the user.</param>
    /// <returns>A list of matching URLs.</returns>
    public List<string> GetSuggestions(string query)
    {
        var results = new List<string>();
        if (string.IsNullOrWhiteSpace(query)) return results;

        string likeQuery = $"%{query}%";
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            // Combines history and bookmarks, removes duplicates, and limits to top 5 matches
            cmd.CommandText = @"
                SELECT DISTINCT Url FROM History WHERE Url LIKE @query 
                UNION SELECT Url FROM Bookmarks WHERE Url LIKE @query 
                LIMIT 5;";
            cmd.Parameters.AddWithValue("@query", likeQuery);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(reader.GetString(0));
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load suggestions", ex);
        }
        return results;
    }

    /// <summary>
    /// Retrieves the complete list of bookmarks ordered by position.
    /// </summary>
    public List<(string Url, string Title)> GetBookmarks()
    {
        var bookmarks = new List<(string Url, string Title)>();
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Url, Title FROM Bookmarks ORDER BY Position ASC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                bookmarks.Add((reader.GetString(0), reader.GetString(1)));
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load bookmarks", ex);
        }
        return bookmarks;
    }

    /// <summary>
    /// Retrieves the complete browsing history ordered by most recent visits.
    /// </summary>
    public List<(string Url, string Title)> GetHistory()
    {
        var history = new List<(string Url, string Title)>();
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Url, Title FROM History ORDER BY VisitTime DESC LIMIT 100;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                history.Add((reader.GetString(0), reader.GetString(1)));
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load history", ex);
        }
        return history;
    }
}
