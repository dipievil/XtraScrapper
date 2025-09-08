using System.Text.Json;
using Microsoft.Extensions.Logging;
using XtraMetaScrapper.Models;

namespace XtraMetaScrapper.Services;

public interface IMetadataExtractor
{
    Task ExtractMetadataAsync(RomFile romFile, OutputConfig outputConfig);
}

public class MetadataExtractor : IMetadataExtractor
{
    private readonly IScreenScraperService _screenScraperService;
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<MetadataExtractor> _logger;

    public MetadataExtractor(
        IScreenScraperService screenScraperService,
        IDatabaseService databaseService,
        ILogger<MetadataExtractor> logger)
    {
        _screenScraperService = screenScraperService;
        _databaseService = databaseService;
        _logger = logger;
    }

    public async Task ExtractMetadataAsync(RomFile romFile, OutputConfig outputConfig)
    {
        _logger.LogInformation("Processing ROM: {RomName} (System: {System})", 
            romFile.FileName, romFile.System);

        // Check if we already have cached metadata
        var cachedMetadata = await _databaseService.GetCachedMetadataAsync(romFile.FilePath);
        if (cachedMetadata.Any(m => m.Exported))
        {
            _logger.LogInformation("Metadata already extracted for: {RomName}", romFile.FileName);
            return;
        }

        // Search for the game on ScreenScraper
        var gameInfo = await _screenScraperService.SearchGameAsync(
            romFile.FileName, romFile.Crc32, romFile.System);

        if (gameInfo?.Response?.Jeu == null)
        {
            _logger.LogWarning("No metadata found for ROM: {RomName}", romFile.FileName);
            return;
        }

        // Extract metadata from ScreenScraper response
        var metadata = ExtractGameMetadata(romFile, gameInfo.Response.Jeu);

        // Export metadata to files
        await ExportMetadataAsync(metadata, outputConfig);

        // Cache the metadata
        await _databaseService.CacheMetadataAsync(metadata);
    }

    private GameMetadata ExtractGameMetadata(RomFile romFile, Jeu jeu)
    {
        var metadata = new GameMetadata
        {
            RomPath = romFile.FilePath,
            System = romFile.System,
            ScrapedAt = DateTime.Now,
            Exported = true
        };

        // Game name (prefer region-specific name)
        var gameName = jeu.Noms?.FirstOrDefault(n => n.Region == "wor")?.Text ??
                       jeu.Noms?.FirstOrDefault(n => n.Region == "us")?.Text ??
                       jeu.Noms?.FirstOrDefault()?.Text ??
                       romFile.FileName;
        metadata.GameName = gameName;

        // Description/Synopsis
        var description = jeu.Synopsis?.FirstOrDefault(s => s.Langue == "en")?.Text ??
                         jeu.Synopsis?.FirstOrDefault(s => s.Langue == "pt")?.Text ??
                         jeu.Synopsis?.FirstOrDefault()?.Text ?? "";
        metadata.Description = description;

        // Publisher and Developer
        metadata.Publisher = jeu.Editeur?.Text ?? "";
        metadata.Developer = jeu.Developpeur?.Text ?? "";

        // Genre/Classification
        var genre = jeu.Classifications?.FirstOrDefault(c => c.Type == "genre")?.Text ?? "";
        metadata.Genre = genre;

        // Release date
        var releaseDate = jeu.Dates?.FirstOrDefault(d => d.Region == "wor")?.Text ??
                         jeu.Dates?.FirstOrDefault(d => d.Region == "us")?.Text ??
                         jeu.Dates?.FirstOrDefault()?.Text ?? "";
        metadata.ReleaseDate = releaseDate;

        // Rating
        var rating = jeu.Notes?.FirstOrDefault(n => n.Type == "note")?.Text ?? "";
        metadata.Rating = rating;

        // Number of players
        var players = jeu.Joueurs?.FirstOrDefault()?.Text ?? "";
        metadata.Players = players;

        // Set default region and language
        metadata.Region = "World";
        metadata.Language = "EN";

        return metadata;
    }

    private async Task ExportMetadataAsync(GameMetadata metadata, OutputConfig outputConfig)
    {
        var romNameWithoutExtension = Path.GetFileNameWithoutExtension(metadata.RomPath);
        var sanitizedRomName = SanitizeFilename(romNameWithoutExtension);

        // Export JSON
        if (outputConfig.ExportJson)
        {
            var jsonFolder = GetFolderPath(outputConfig.JsonFolder, metadata.System);
            Directory.CreateDirectory(jsonFolder);
            
            var jsonPath = Path.Combine(jsonFolder, $"{sanitizedRomName}.json");
            var jsonData = JsonSerializer.Serialize(metadata, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            await File.WriteAllTextAsync(jsonPath, jsonData);
            _logger.LogInformation("Exported JSON metadata: {JsonPath}", jsonPath);
        }

        // Export XML
        if (outputConfig.ExportXml)
        {
            var xmlFolder = GetFolderPath(outputConfig.XmlFolder, metadata.System);
            Directory.CreateDirectory(xmlFolder);
            
            var xmlPath = Path.Combine(xmlFolder, $"{sanitizedRomName}.xml");
            var xmlData = CreateGameXml(metadata);
            
            await File.WriteAllTextAsync(xmlPath, xmlData);
            _logger.LogInformation("Exported XML metadata: {XmlPath}", xmlPath);
        }

        // Export CSV (append to system file)
        if (outputConfig.ExportCsv)
        {
            var csvFolder = GetFolderPath(outputConfig.CsvFolder, metadata.System);
            Directory.CreateDirectory(csvFolder);
            
            var csvPath = Path.Combine(csvFolder, $"{metadata.System}_metadata.csv");
            await AppendToCsvAsync(csvPath, metadata);
            _logger.LogInformation("Exported CSV metadata: {CsvPath}", csvPath);
        }
    }

    private string GetFolderPath(string template, string systemName)
    {
        return template.Replace("{SYSTEM}", systemName);
    }

    private string SanitizeFilename(string filename)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(filename
            .Where(c => !invalidChars.Contains(c))
            .ToArray());
        
        return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
    }

    private string CreateGameXml(GameMetadata metadata)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<game>
    <name>{EscapeXml(metadata.GameName)}</name>
    <description>{EscapeXml(metadata.Description)}</description>
    <publisher>{EscapeXml(metadata.Publisher)}</publisher>
    <developer>{EscapeXml(metadata.Developer)}</developer>
    <genre>{EscapeXml(metadata.Genre)}</genre>
    <releasedate>{EscapeXml(metadata.ReleaseDate)}</releasedate>
    <rating>{EscapeXml(metadata.Rating)}</rating>
    <players>{EscapeXml(metadata.Players)}</players>
    <system>{EscapeXml(metadata.System)}</system>
    <region>{EscapeXml(metadata.Region)}</region>
    <language>{EscapeXml(metadata.Language)}</language>
    <rompath>{EscapeXml(metadata.RomPath)}</rompath>
    <scrapedat>{metadata.ScrapedAt:yyyy-MM-dd HH:mm:ss}</scrapedat>
</game>";
    }

    private string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private async Task AppendToCsvAsync(string csvPath, GameMetadata metadata)
    {
        var isNewFile = !File.Exists(csvPath);
        
        using var writer = new StreamWriter(csvPath, append: true);
        
        // Write header if new file
        if (isNewFile)
        {
            await writer.WriteLineAsync("RomPath,GameName,Description,Publisher,Developer,Genre,ReleaseDate,Rating,Players,System,Region,Language,ScrapedAt");
        }
        
        // Write data
        var csvLine = string.Join(",", 
            EscapeCsv(metadata.RomPath),
            EscapeCsv(metadata.GameName),
            EscapeCsv(metadata.Description),
            EscapeCsv(metadata.Publisher),
            EscapeCsv(metadata.Developer),
            EscapeCsv(metadata.Genre),
            EscapeCsv(metadata.ReleaseDate),
            EscapeCsv(metadata.Rating),
            EscapeCsv(metadata.Players),
            EscapeCsv(metadata.System),
            EscapeCsv(metadata.Region),
            EscapeCsv(metadata.Language),
            metadata.ScrapedAt.ToString("yyyy-MM-dd HH:mm:ss"));
        
        await writer.WriteLineAsync(csvLine);
    }

    private string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text)) return "\"\"";
        
        if (text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r'))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }
        
        return $"\"{text}\"";
    }
}
