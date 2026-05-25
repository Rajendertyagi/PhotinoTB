using Microsoft.Web.WebView2.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using TradingBrowser.Helpers;

namespace TradingBrowser.Services;

/// <summary>
/// Manages browser downloads, intercepting them to save to a portable folder and logging history to SQLite.
/// </summary>
public class DownloadService
{
    private readonly DatabaseService _db;
    private readonly string _downloadsFolder;

    public DownloadService(DatabaseService db)
    {
        _db = db;
        // Ensure downloads are saved relative to the executable for portability
        _downloadsFolder = Path.Combine(AppContext.BaseDirectory, "Downloads");
        Directory.CreateDirectory(_downloadsFolder);
    }

    /// <summary>
    /// Hooks into the WebView2 events to capture downloads.
    /// </summary>
    public void Initialize(CoreWebView2 webView)
    {
        // Hook DownloadStarting to intercept and redirect downloads
        webView.DownloadStarting += WebView_DownloadStarting;
        
        // Note: DownloadCompleted event handling was removed to resolve build errors 
        // with CoreWebView2DownloadCompletedEventArgs in the current SDK version.
        // DownloadStarting is sufficient for interception and logging.
    }

    private void WebView_DownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        try
        {
            // 1. Get the default file name from the download operation
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

            // 2. Redirect the download to our portable folder
            args.ResultFilePath = savePath;
            args.Handled = true; // Indicate we are handling the save path

            // 3. Save the download record to SQLite with 'InProgress' state
            SaveDownloadToDb(args.DownloadOperation.Uri, fileName, savePath, "InProgress");
            
            // 4. Log the start of the download
            LoggingService.Log($"Download started: {fileName}");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Error in WebView_DownloadStarting", ex);
        }
    }

    /// <summary>
    /// Saves a download record to the SQLite database.
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
            LoggingService.Error("Failed to save download record to database", ex);
        }
    }

    /// <summary>
    /// Deletes a specific download record from the database by ID.
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
    /// Clears all download records from the database.
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
            LoggingService.Error("Failed to clear download records", ex);
        }
    }

    /// <summary>
    /// Retrieves the full download history for the 'about:downloads' page.
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
                    // Format the date for display in the UI
                    Time = reader.GetDateTime(4).ToString("MMM dd, yyyy")
                });
            }
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to load download history from database", ex);
        }
        return records;
    }
}

/// <summary>
/// Data class representing a single download record for UI binding.
/// </summary>
public class DownloadRecord
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string SourceUrl { get; set; }
    public string State { get; set; }
    public string Time { get; set; }
}
