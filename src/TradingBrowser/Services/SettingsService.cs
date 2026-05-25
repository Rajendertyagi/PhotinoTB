using Microsoft.Data.Sqlite;
using TradingBrowser.Services;

namespace TradingBrowser.Helpers;

public static class SettingsService
{
    public static string Get(string key, string defaultValue)
    {
        try
        {
            using var conn = App.Db!.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Value FROM Settings WHERE Key = @key;";
            cmd.Parameters.AddWithValue("@key", key);
            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? defaultValue;
        }
        catch { return defaultValue; }
    }

    public static void Set(string key, string value)
    {
        try
        {
            using var conn = App.Db!.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@key, @value);";
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@value", value);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex) { LoggingService.Error("Failed to save setting", ex); }
    }
}
