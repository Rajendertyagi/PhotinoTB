using System;

namespace TB_Browser.Models;

/// <summary>
/// Data model for browser history entries.
/// Mapped to SQLite 'History' table.
/// </summary>
public class HistoryEntry
{
    public string? Id { get; set; } // Optional PK
    public string? Url { get; set; }
    public string? Title { get; set; }
    public DateTime LastVisited { get; set; }
    public DateTime FirstVisited { get; set; }
    public int VisitCount { get; set; }
    public int TypedCount { get; set; }
}
