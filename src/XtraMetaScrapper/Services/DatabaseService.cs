using Microsoft.Data.Sqlite;
using XtraMetaScrapper.Models;

namespace XtraMetaScrapper.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<bool> IsMetadataCachedAsync(string romPath);
    Task CacheMetadataAsync(GameMetadata metadata);
    Task<IEnumerable<GameMetadata>> GetCachedMetadataAsync(string romPath);
    Task<IEnumerable<GameMetadata>> GetAllMetadataAsync();
}

public class DatabaseService : IDatabaseService
{
    private readonly string _connectionString;
    
    public DatabaseService(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var createTableSql = @"
            CREATE TABLE IF NOT EXISTS GameMetadata (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RomPath TEXT NOT NULL,
                GameName TEXT,
                Description TEXT,
                Publisher TEXT,
                Developer TEXT,
                Genre TEXT,
                ReleaseDate TEXT,
                Rating TEXT,
                Players TEXT,
                System TEXT,
                Region TEXT,
                Language TEXT,
                ScrapedAt TEXT NOT NULL,
                Exported INTEGER NOT NULL DEFAULT 0
            )";

        using var command = new SqliteCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsMetadataCachedAsync(string romPath)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM GameMetadata WHERE RomPath = @romPath";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@romPath", romPath);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task CacheMetadataAsync(GameMetadata metadata)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT OR REPLACE INTO GameMetadata 
            (RomPath, GameName, Description, Publisher, Developer, Genre, ReleaseDate, Rating, Players, System, Region, Language, ScrapedAt, Exported)
            VALUES (@romPath, @gameName, @description, @publisher, @developer, @genre, @releaseDate, @rating, @players, @system, @region, @language, @scrapedAt, @exported)";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@romPath", metadata.RomPath);
        command.Parameters.AddWithValue("@gameName", metadata.GameName ?? "");
        command.Parameters.AddWithValue("@description", metadata.Description ?? "");
        command.Parameters.AddWithValue("@publisher", metadata.Publisher ?? "");
        command.Parameters.AddWithValue("@developer", metadata.Developer ?? "");
        command.Parameters.AddWithValue("@genre", metadata.Genre ?? "");
        command.Parameters.AddWithValue("@releaseDate", metadata.ReleaseDate ?? "");
        command.Parameters.AddWithValue("@rating", metadata.Rating ?? "");
        command.Parameters.AddWithValue("@players", metadata.Players ?? "");
        command.Parameters.AddWithValue("@system", metadata.System ?? "");
        command.Parameters.AddWithValue("@region", metadata.Region ?? "");
        command.Parameters.AddWithValue("@language", metadata.Language ?? "");
        command.Parameters.AddWithValue("@scrapedAt", metadata.ScrapedAt.ToString("O"));
        command.Parameters.AddWithValue("@exported", metadata.Exported ? 1 : 0);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<GameMetadata>> GetCachedMetadataAsync(string romPath)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Id, RomPath, GameName, Description, Publisher, Developer, Genre, ReleaseDate, Rating, Players, System, Region, Language, ScrapedAt, Exported 
                    FROM GameMetadata WHERE RomPath = @romPath";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@romPath", romPath);

        var metadata = new List<GameMetadata>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            metadata.Add(new GameMetadata
            {
                Id = reader.GetInt32(0),
                RomPath = reader.GetString(1),
                GameName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Publisher = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Developer = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Genre = reader.IsDBNull(6) ? "" : reader.GetString(6),
                ReleaseDate = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Rating = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Players = reader.IsDBNull(9) ? "" : reader.GetString(9),
                System = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Region = reader.IsDBNull(11) ? "" : reader.GetString(11),
                Language = reader.IsDBNull(12) ? "" : reader.GetString(12),
                ScrapedAt = DateTime.Parse(reader.GetString(13)),
                Exported = reader.GetInt32(14) == 1
            });
        }

        return metadata;
    }

    public async Task<IEnumerable<GameMetadata>> GetAllMetadataAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"SELECT Id, RomPath, GameName, Description, Publisher, Developer, Genre, ReleaseDate, Rating, Players, System, Region, Language, ScrapedAt, Exported 
                    FROM GameMetadata ORDER BY GameName";
        using var command = new SqliteCommand(sql, connection);

        var metadata = new List<GameMetadata>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            metadata.Add(new GameMetadata
            {
                Id = reader.GetInt32(0),
                RomPath = reader.GetString(1),
                GameName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                Publisher = reader.IsDBNull(4) ? "" : reader.GetString(4),
                Developer = reader.IsDBNull(5) ? "" : reader.GetString(5),
                Genre = reader.IsDBNull(6) ? "" : reader.GetString(6),
                ReleaseDate = reader.IsDBNull(7) ? "" : reader.GetString(7),
                Rating = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Players = reader.IsDBNull(9) ? "" : reader.GetString(9),
                System = reader.IsDBNull(10) ? "" : reader.GetString(10),
                Region = reader.IsDBNull(11) ? "" : reader.GetString(11),
                Language = reader.IsDBNull(12) ? "" : reader.GetString(12),
                ScrapedAt = DateTime.Parse(reader.GetString(13)),
                Exported = reader.GetInt32(14) == 1
            });
        }

        return metadata;
    }
}
