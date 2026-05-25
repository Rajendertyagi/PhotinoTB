using Microsoft.Web.WebView2.Core;
using Microsoft.Data.Sqlite;
using System;
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
        webView.DownloadStarting += WebView_DownloadStarting;
        // Note: DownloadCompleted is an event on the CoreWebView2 itself in newer SDKs, 
        // or we can track via the DownloadOperation object. 
        // For simplicity in this build, we track state via the 'DownloadStarting' and update on completion via a separate hook if needed,
        // but WebView2 1.0.3967+ supports CoreWebView2.DownloadCompleted.
        if (webView is CoreWebView2_2 webView2) 
        {
            webView2.DownloadCompleted += WebView_DownloadCompleted;
        }
    }

    private void WebView_DownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        // 1. Redirect download to our portable folder
        string fileName = Path.GetFileName(args.ResultFilePath);
        string savePath = Path.Combine(_downloadsFolder, fileName);
        
        // Handle duplicate filenames if they exist
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

        args.ResultFilePath = savePath;
        args.Handled = true; // We are handling the saving logic

        // 2. Save to Database
        SaveDownloadToDb(args.DownloadOperation.Uri, fileName, savePath, "InProgress");
        
        // 3. Update UI (Optional: Log to console or trigger a UI refresh if we had a live download bar)
        LoggingService.Log($"Download started: {fileName}");
    }

    private void WebView_DownloadCompleted(CoreWebView2 sender, CoreWebView2DownloadCompletedEventArgs args)
    {
        string state = args.IsSuccess ? "Completed" : "Failed";
        LoggingService.Log($"Download {state}: {Path.GetFileName(args.DownloadOperation.ResultFilePath)}");
        // Update DB status
        UpdateDownloadStatus(args.DownloadOperation.ResultFilePath, state);
    }

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

    private void UpdateDownloadStatus(string path, string state)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Downloads SET State = @state WHERE SavePath = @path;";
            cmd.Parameters.AddWithValue("@state", state);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to update download status", ex);
        }
    }

    /// <summary>
    /// Retrieves the full download history for the 'about:downloads' page.
    /// </summary>
    public System.Collections.Generic.List<DownloadRecord> GetHistory()
    {
        var records = new System.Collections.Generic.List<DownloadRecord>();
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
/// Simple data class for download records.
/// </summary>
public class DownloadRecord
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string SourceUrl { get; set; }
    public string State { get; set; }
    public string Time { get; set; }
}
