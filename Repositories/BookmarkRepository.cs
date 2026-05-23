using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using TB_Browser.Infrastructure;
using TB_Browser.Models;

namespace TB_Browser.Repositories;

/// <summary>
/// Data access layer for Bookmarks.
/// Optimized for batch queue flushing via SQLite UPSERT.
/// </summary>
public class BookmarkRepository
{
    private readonly IDbConnectionFactory _factory;

    public BookmarkRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Bookmark>> GetAllAsync()
    {
        using var conn = _factory.CreateConnection();
        return await conn.QueryAsync<Bookmark>(
            "SELECT * FROM Bookmarks ORDER BY LastModified DESC, Title ASC");
    }

    /// <summary>
    /// Bulk upserts bookmarks from the in-memory queue.
    /// Uses SQLite ON CONFLICT to avoid SELECT-then-UPDATE race conditions.
    /// </summary>
    public async Task UpsertBatchAsync(IEnumerable<Bookmark> bookmarks)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO Bookmarks (Url, Title, FaviconUrl, Folder, CreatedAt, LastModified, VisitCount)
            VALUES (@Url, @Title, @FaviconUrl, @Folder, @CreatedAt, @LastModified, @VisitCount)
            ON CONFLICT(Url) DO UPDATE SET
                Title = excluded.Title,
                FaviconUrl = excluded.FaviconUrl,
                Folder = excluded.Folder,
                LastModified = excluded.LastModified,
                VisitCount = Bookmarks.VisitCount + 1;";
        
        await conn.ExecuteAsync(sql, bookmarks);
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM Bookmarks WHERE Id = @Id", new { Id = id });
    }
}
