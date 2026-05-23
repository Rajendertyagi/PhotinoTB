using Dapper;
using TB_Browser.Infrastructure;

namespace TB_Browser.Infrastructure;

public class DbInitializer
{
    private readonly IDbConnectionFactory _factory;

    public DbInitializer(IDbConnectionFactory factory) => _factory = factory;

    public void Initialize()
    {
        using var conn = _factory.CreateConnection();
        conn.Open();

        // Bookmarks Table (Added UNIQUE constraint for UPSERT support)
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS Bookmarks (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL UNIQUE,
                Title TEXT NOT NULL,
                FaviconUrl TEXT,
                Folder TEXT DEFAULT 'General',
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                LastModified DATETIME DEFAULT CURRENT_TIMESTAMP,
                VisitCount INTEGER DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS IX_Bookmarks_Url ON Bookmarks(Url);
        ");

        // History Table (Added UNIQUE constraint for UPSERT support)
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL UNIQUE,
                Title TEXT,
                VisitCount INTEGER DEFAULT 1,
                LastVisited DATETIME DEFAULT CURRENT_TIMESTAMP,
                FirstVisited DATETIME DEFAULT CURRENT_TIMESTAMP,
                TypedCount INTEGER DEFAULT 0
            );
            CREATE INDEX IF NOT EXISTS IX_History_Url ON History(Url);
            CREATE INDEX IF NOT EXISTS IX_History_LastVisited ON History(LastVisited);
        ");
        
        // Session State (Simple JSON blob for tabs)
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS SessionState (
                Id INTEGER PRIMARY KEY CHECK (Id = 1),
                TabsJson TEXT,
                LastSaved DATETIME DEFAULT CURRENT_TIMESTAMP
            );
        ");
    }
}
