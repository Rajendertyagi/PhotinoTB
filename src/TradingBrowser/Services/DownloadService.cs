using Microsoft.Web.WebView2.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using TradingBrowser.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace TradingBrowser.Services;

/// <summary>
/// Represents an active download with UI-bindable properties for progress and state.
/// </summary>
public partial class ActiveDownload : ObservableObject
{
    private readonly CoreWebView2DownloadOperation _operation;
    private readonly Action<string> _onStateChange;

    public string FileName { get; }
    public string SavePath { get; }
    public string Url { get; }

    [ObservableProperty] public partial double Progress { get; set; }
    [ObservableProperty] public partial string StatusText { get; set; } = "Downloading...";
    
    // Visibility properties to avoid needing XAML IValueConverters
    [ObservableProperty] public partial Visibility PauseVisibility { get; set; } = Visibility.Visible;
    [ObservableProperty] public partial Visibility ResumeVisibility { get; set; } = Visibility.Collapsed;
    [ObservableProperty] public partial Visibility CancelVisibility { get; set; } = Visibility.Visible;

    public ActiveDownload(CoreWebView2DownloadOperation operation, string url, string fileName, string savePath, Action<string> onStateChange)
    {
        _operation = operation;
        Url = url;
        FileName = fileName;
        SavePath = savePath;
        _onStateChange = onStateChange;

        _operation.BytesReceivedChanged += (s, e) => UpdateProgress();
        _operation.EstimatedEndTimeChanged += (s, e) => UpdateProgress();
        _operation.StateChanged += (s, e) => UpdateState();
        
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        var received = _operation.BytesReceived;
        var total = _operation.TotalBytesToReceive;
        
        if (total > 0)
        {
            Progress = (double)received / total * 100;
            StatusText = $"{FormatBytes(received)} / {FormatBytes(total)}";
        }
        else
        {
            StatusText = $"{FormatBytes(received)} / Unknown";
        }
    }

    private void UpdateState()
    {
        switch (_operation.State)
        {
            case CoreWebView2DownloadState.InProgress:
                PauseVisibility = Visibility.Visible;
                ResumeVisibility = Visibility.Collapsed;
                CancelVisibility = Visibility.Visible;
                StatusText = "Downloading...";
                break;
            case CoreWebView2DownloadState.Interrupted:
                PauseVisibility = Visibility.Collapsed;
                ResumeVisibility = Visibility.Visible;
                CancelVisibility = Visibility.Visible;
                StatusText = "Paused";
                _onStateChange("Paused");
                break;
            case CoreWebView2DownloadState.Completed:
                PauseVisibility = Visibility.Collapsed;
                ResumeVisibility = Visibility.Collapsed;
                CancelVisibility = Visibility.Collapsed;
                Progress = 100;
                StatusText = "Completed";
                _onStateChange("Completed");
                break;
        }
    }

    [RelayCommand]
    public void Pause()
    {
        if (_operation.State == CoreWebView2DownloadState.InProgress)
            _operation.Pause();
    }

    [RelayCommand]
    public void Resume()
    {
        if (_operation.State == CoreWebView2DownloadState.Interrupted)
            _operation.Resume();
    }

    [RelayCommand]
    public void Cancel()
    {
        _operation.Cancel();
        StatusText = "Cancelled";
        _onStateChange("Cancelled");
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}

/// <summary>
/// Manages browser downloads, intercepting them to save to a configurable folder 
/// and logging history to SQLite. Supports runtime path updates and active UI tracking.
/// </summary>
public class DownloadService
{
    private readonly DatabaseService _db;
    private string _downloadsFolder;
    
    /// <summary>
    /// Observable collection for the XAML UI to bind to for active downloads.
    /// </summary>
    public ObservableCollection<ActiveDownload> ActiveDownloads { get; } = new();

    public DownloadService(DatabaseService db)
    {
        _db = db;
        _downloadsFolder = Path.Combine(AppContext.BaseDirectory, "Downloads");
        Directory.CreateDirectory(_downloadsFolder);
    }

    public void Initialize(CoreWebView2 webView)
    {
        webView.DownloadStarting += WebView_DownloadStarting;
    }

    private void WebView_DownloadStarting(CoreWebView2 sender, CoreWebView2DownloadStartingEventArgs args)
    {
        try
        {
            string fileName = Path.GetFileName(args.ResultFilePath);
            string savePath = Path.Combine(_downloadsFolder, fileName);
            
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
            args.Handled = true; // Cancel default browser prompt
            
            SaveDownloadToDb(args.DownloadOperation.Uri, fileName, savePath, "InProgress");
            
            var activeDownload = new ActiveDownload(
                args.DownloadOperation, 
                args.DownloadOperation.Uri, 
                fileName, 
                savePath,
                (state) => UpdateDownloadStateInDb(savePath, state)
            );
            
            // Add to UI collection (WebView2 events fire on the UI thread)
            ActiveDownloads.Add(activeDownload);
            
            LoggingService.Log($"Download started: {fileName}");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Error intercepting download", ex);
        }
    }

    private void UpdateDownloadStateInDb(string savePath, string state)
    {
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Downloads SET State = @state WHERE SavePath = @path;";
            cmd.Parameters.AddWithValue("@state", state);
            cmd.Parameters.AddWithValue("@path", savePath);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to update download state", ex);
        }
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

public class DownloadRecord
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
}
