using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace TB_Browser.Infrastructure;

public interface IDbConnectionFactory
{
    DbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly PathResolver _pathResolver;

    public DbConnectionFactory(PathResolver pathResolver)
    {
        _pathResolver = pathResolver;
    }

    public DbConnection CreateConnection()
    {
        // Connection String:
        // - Default Timeout: 30s
        // - Pooling: true (default)
        // - Foreign Keys: On (Crucial for relational integrity)
        // - Cache: Shared (Better performance)
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _pathResolver.DatabasePath,
            DefaultTimeout = 30,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        return new SqliteConnection(connectionString);
    }
}
