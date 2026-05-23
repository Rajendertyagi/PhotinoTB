using Microsoft.Data.Sqlite;
using System.Data;

namespace TB.Infrastructure;

public interface IDbConnectionFactory { IDbConnection CreateConnection(); }
public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;
    public DbConnectionFactory(PathResolver path) => _connStr = $"Data Source={path.DatabasePath}";
    public IDbConnection CreateConnection() => new SqliteConnection(_connStr);
}
