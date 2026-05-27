using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Linq;
using TradingBrowser.Helpers;
using TradingBrowser.ViewModels;

namespace TradingBrowser.Services;

public class SessionService
{
    private readonly DatabaseService _db;

    public SessionService(DatabaseService db) => _db = db;

    public void SaveSession(IEnumerable<TabViewModel> tabs, string? activeTabId)
    {
        using var conn = _db.GetConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        using var deleteCmd = conn.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Sessions;";
        deleteCmd.ExecuteNonQuery();

        int pos = 0;
        foreach (var tab in tabs)
        {
            using var insertCmd = conn.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Sessions (TabId, Url, Title, IsActive, Position) VALUES (@id, @url, @title, @active, @pos);";
            insertCmd.Parameters.AddWithValue("@id", tab.Id.ToString());
            insertCmd.Parameters.AddWithValue("@url", tab.Url);
            insertCmd.Parameters.AddWithValue("@title", tab.Title);
            insertCmd.Parameters.AddWithValue("@active", tab.Id.ToString() == activeTabId);
            insertCmd.Parameters.AddWithValue("@pos", pos++);
            insertCmd.ExecuteNonQuery();
        }
        transaction.Commit();
    }

    // FIX: Method signature matches what MainWindow expects
    public List<TabViewModel> LoadSession(out string? activeTabId)
    {
        activeTabId = null;
        var tabs = new List<TabViewModel>();
        
        try
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TabId, Url, Title, IsActive FROM Sessions ORDER BY Position ASC;";
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                string tabId = reader.GetString(0);
                var tab = new TabViewModel
                {
                    // FIX: Properly restore the Id so active tab matching works
                    Id = System.Guid.Parse(tabId),
                    Url = reader.GetString(1),
                    Title = reader.GetString(2)
                };
                tabs.Add(tab);
                
                if (reader.GetBoolean(3)) 
                    activeTabId = tabId;
            }
            LoggingService.Info($"[Session] Loaded {tabs.Count} tabs from database");
        }
        catch (System.Exception ex)
        {
            LoggingService.Error("[Session] LoadSession failed", ex);
        }
        
        return tabs;
    }
}
