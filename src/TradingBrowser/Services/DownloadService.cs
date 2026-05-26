using Microsoft.Web.WebView2.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using TradingBrowser.Helpers;

namespace TradingBrowser.Services;

/// <summary>
/// Manages browser downloads, intercepting them to save to a configurable folder 
/// and logging history to SQLite. Supports runtime path updates.
/// </summary>
public class DownloadService
{
    private readonly DatabaseService _db;
    private string _downloadsFolder; // Non-readonly to support runtime path updates

    /// <summary>
    /// Initializes the service with the database connection and default download path.
    /// </summary>
    public DownloadService(DatabaseService db)
    {
        _db = db;
        // Default to portable ./Downloads folder relative to executable
        _downloadsFolder = Path.Combine(AppContext.BaseDirectory, "Downloads");
        Directory.CreateDirectory(_downloadsFolder);
    }

    /// <summary>
    /// Hooks into WebView2 events to intercept and manage downloads.
    /// </summary>
    public void Initialize(CoreWebView2 webView)
    {
        webView.DownloadStarting += WebView_DownloadStarting;
    }

    /// <summary>
    /// Intercepts download start, redirects to portable folder, handles duplicates, and logs to SQLite.
    /// </summary>
    private void WebView_DownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        try
        {
            string fileName = Path.GetFileName(args.ResultFilePath);
            string savePath = Path.Combine(_downloadsFolder, fileName);
            
            // Handle duplicate filenames by appending a counter
            if (File.Exists(savePath))
            {
                string ext = Path.GetExtension(fileName);
                string name = Path.GetFileNameWithoutExtension(fileName);
                int counter = 1;
                while (File.Exists(savePath))
                {
                    savePath = Path.Combine(_downloadsFolder, $"{name} ({counter}){ext}");
                    counter++;
                }
            }

            // Redirect download to our managed folder
            args.ResultFilePath = savePath;
            args.Handled = true;
            
            // Log to SQLite immediately
            SaveDownloadToDb(args.DownloadOperation.Uri, fileName, savePath, "InProgress");
            LoggingService.Log($"Download started: {fileName}");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Error intercepting download", ex);
        }
    }

    /// <summary>
    /// Inserts a new download record into the SQLite database.
    /// </summary>
    private void SaveDownloadToDb(string url, string fileName, string path, string state)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Downloads (FileName, SourceUrl, SavePath, State, StartTime) 
                                VALUES (@file, @url, @path, @state, datetime('now'));";
            cmd.Parameters.AddWithValue("@file", fileName);
            cmd.Parameters.AddWithValue("@url", url);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@state", state);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to save download record", ex);
        }
    }

    /// <summary>
    /// Deletes a specific download record from SQLite by ID.
    /// </summary>
    public void DeleteDownload(int id)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Downloads WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to delete download record", ex);
        }
    }

    /// <summary>
    /// Clears all download history from SQLite. Does not delete actual files.
    /// </summary>
    public void ClearAllDownloads()
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Downloads;";
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to clear download history", ex);
        }
    }

    /// <summary>
    /// Updates the download directory at runtime. Creates the folder if it doesn't exist.
    /// </summary>
    public void UpdateDownloadPath(string newPath)
    {
        if (string.IsNullOrWhiteSpace(newPath)) return;
        
        try
        {
            Directory.CreateDirectory(newPath);
            _downloadsFolder = newPath;
            LoggingService.Log($"Download path updated to: {newPath}");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to update download path", ex);
        }
    }

    /// <summary>
    /// Retrieves the full download history ordered by most recent.
    /// </summary>
    public List<DownloadRecord> GetHistory()
    {
        var records = new List<DownloadRecord>();
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, FileName, SourceUrl, State, StartTime FROM Downloads ORDER BY StartTime DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                records.Add(new DownloadRecord
                {
                    Id = reader.GetInt32(0),
                    FileName = reader.GetString(1),
                    SourceUrl = reader.GetString(2),
                    State = reader.GetString(3),
                    StartTime = reader.GetDateTime(4),
                    Time = reader.GetDateTime(4).ToString("MMM dd, yyyy")
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load download history", ex);
        }
        return records;
    }
}

/// <summary>
/// Data transfer object for download records. Used for UI binding and HTML generation.
/// </summary>
public class DownloadRecord
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
}
