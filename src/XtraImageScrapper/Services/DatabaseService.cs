using Microsoft.Data.Sqlite;
using XtraImageScrapper.Models;

namespace XtraImageScrapper.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<bool> IsImageCachedAsync(string romPath, string imageType);
    Task CacheImageAsync(ScrapedImage image);
    Task<IEnumerable<ScrapedImage>> GetCachedImagesAsync(string romPath);
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
            CREATE TABLE IF NOT EXISTS ScrapedImages (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RomPath TEXT NOT NULL,
                ImageType TEXT NOT NULL,
                ImageUrl TEXT NOT NULL,
                LocalPath TEXT NOT NULL,
                ScrapedAt TEXT NOT NULL,
                Downloaded INTEGER NOT NULL DEFAULT 0,
                UNIQUE(RomPath, ImageType)
            )";

        using var command = new SqliteCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsImageCachedAsync(string romPath, string imageType)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT COUNT(*) FROM ScrapedImages WHERE RomPath = @romPath AND ImageType = @imageType AND Downloaded = 1";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@romPath", romPath);
        command.Parameters.AddWithValue("@imageType", imageType);

        var count = (long)(await command.ExecuteScalarAsync() ?? 0);
        return count > 0;
    }

    public async Task CacheImageAsync(ScrapedImage image)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT OR REPLACE INTO ScrapedImages 
            (RomPath, ImageType, ImageUrl, LocalPath, ScrapedAt, Downloaded)
            VALUES (@romPath, @imageType, @imageUrl, @localPath, @scrapedAt, @downloaded)";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@romPath", image.RomPath);
        command.Parameters.AddWithValue("@imageType", image.ImageType);
        command.Parameters.AddWithValue("@imageUrl", image.ImageUrl);
        command.Parameters.AddWithValue("@localPath", image.LocalPath);
        command.Parameters.AddWithValue("@scrapedAt", image.ScrapedAt.ToString("O"));
        command.Parameters.AddWithValue("@downloaded", image.Downloaded ? 1 : 0);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<ScrapedImage>> GetCachedImagesAsync(string romPath)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var sql = "SELECT Id, RomPath, ImageType, ImageUrl, LocalPath, ScrapedAt, Downloaded FROM ScrapedImages WHERE RomPath = @romPath";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@romPath", romPath);

        var images = new List<ScrapedImage>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            images.Add(new ScrapedImage
            {
                Id = reader.GetInt32(0),
                RomPath = reader.GetString(1),
                ImageType = reader.GetString(2),
                ImageUrl = reader.GetString(3),
                LocalPath = reader.GetString(4),
                ScrapedAt = DateTime.Parse(reader.GetString(5)),
                Downloaded = reader.GetInt32(6) == 1
            });
        }

        return images;
    }
}