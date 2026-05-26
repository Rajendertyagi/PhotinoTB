using Microsoft.Data.Sqlite;
using System;
using System.IO;
using TradingBrowser.Helpers;

namespace TradingBrowser.Services;

/// <summary>
/// Manages the SQLite database connection and schema initialization.
/// Ensures all required tables exist before the app interacts with data.
/// </summary>
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        // Create UserData directory if it doesn't exist for portable storage
        Directory.CreateDirectory(PathHelper.UserDataFolder);
        
        // Configure SQLite connection string with ReadWriteCreate mode and connection pooling
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = PathHelper.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        }.ToString();

        // Run schema initialization on construction
        InitializeDatabase();
    }

    /// <summary>
    /// Opens and configures the database, creating tables if they don't already exist.
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            // Enable Write-Ahead Logging (WAL) for better concurrent read/write performance
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA journal_mode=WAL;";
            cmd.ExecuteNonQuery();

            // Define SQL schema for all application tables
            // FIX: Added 'Position INTEGER' to the Sessions table
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Settings (Key TEXT PRIMARY KEY, Value TEXT);
                CREATE TABLE IF NOT EXISTS History (Id INTEGER PRIMARY KEY AUTOINCREMENT, Url TEXT, Title TEXT, VisitTime DATETIME);
                CREATE TABLE IF NOT EXISTS Bookmarks (Id INTEGER PRIMARY KEY AUTOINCREMENT, Url TEXT, Title TEXT, Position INTEGER);
                CREATE TABLE IF NOT EXISTS Sessions (Id INTEGER PRIMARY KEY AUTOINCREMENT, TabId TEXT, Url TEXT, Title TEXT, IsActive BOOLEAN, Position INTEGER);
                CREATE TABLE IF NOT EXISTS Downloads (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT,
                    SourceUrl TEXT,
                    SavePath TEXT,
                    State TEXT,
                    StartTime DATETIME
                );
            ";
            cmd.ExecuteNonQuery();
            
            // MIGRATION FIX: If the user already has an old database without the Position column, 
            // this safely adds it without crashing or deleting their data.
            try
            {
                cmd.CommandText = "ALTER TABLE Sessions ADD COLUMN Position INTEGER;";
                cmd.ExecuteNonQuery();
            }
            catch 
            { 
                // SQLite throws an exception if the column already exists. We safely ignore it.
            }

            LoggingService.Log("Database schema initialized successfully.");
        }
        catch (Exception ex)
        {
            LoggingService.Error("Failed to initialize database schema", ex);
        }
    }

    /// <summary>
    /// Returns a new open SQLite connection for data operations.
    /// </summary>
    public SqliteConnection GetConnection() => new SqliteConnection(_connectionString);
}
