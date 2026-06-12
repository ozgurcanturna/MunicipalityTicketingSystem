using System.Collections.Concurrent;
using Npgsql;

namespace SharedKernel.Infrastructure.Persistence;

public static class PostgresDatabaseInitializer
{
    private static readonly ConcurrentDictionary<string, byte> EnsuredDatabases = new();
    private static readonly object SyncRoot = new();

    public static void EnsureDatabaseExists(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database ?? string.Empty;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("PostgreSQL bağlantısında database adı zorunludur.");
        }

        var cacheKey = $"{builder.Host}:{builder.Port}/{databaseName}";
        if (EnsuredDatabases.ContainsKey(cacheKey))
        {
            return;
        }

        lock (SyncRoot)
        {
            if (EnsuredDatabases.ContainsKey(cacheKey))
            {
                return;
            }

            EnsureDatabaseExistsCore(builder, databaseName);
            EnsuredDatabases[cacheKey] = 0;
        }
    }

    private static void EnsureDatabaseExistsCore(NpgsqlConnectionStringBuilder builder, string databaseName)
    {
        var adminBuilder = new NpgsqlConnectionStringBuilder(builder.ConnectionString)
        {
            Database = "postgres"
        };

        using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
        connection.Open();

        using (var checkCommand = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @databaseName", connection))
        {
            checkCommand.Parameters.AddWithValue("databaseName", databaseName);
            var exists = checkCommand.ExecuteScalar() is not null;

            if (exists)
            {
                return;
            }
        }

        var escapedDatabaseName = databaseName.Replace("\"", "\"\"");
        using var createCommand = new NpgsqlCommand($"CREATE DATABASE \"{escapedDatabaseName}\"", connection);
        createCommand.ExecuteNonQuery();
    }
}