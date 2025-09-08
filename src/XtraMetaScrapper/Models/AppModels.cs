namespace XtraMetaScrapper.Models;

public class AppSettings
{
    public Settings Settings { get; set; } = new();
}

public class Settings
{
    public string RomsFolder { get; set; } = "./roms";
    public string DatabasePath { get; set; } = "XtraMetaScrapper.db";
    public string LogFilePath { get; set; } = "XtraMetaScrapper_{0:yyyyMMdd_HHmmss}.log";
    public string OutputConfigPath { get; set; } = "output-config.json";
    public string ScreenScraperUser { get; set; } = "";
    public string ScreenScraperPassword { get; set; } = "";
    public string ScreenScraperApiKey { get; set; } = "";
    public int MaxRequestsPerSecond { get; set; } = 1;
    public int TimeoutSeconds { get; set; } = 30;
}

public class CommandLineArgs
{
    public string? RomsFolder { get; set; }
    public string? OutputFolder { get; set; }
    public string? JsonFolder { get; set; }
    public string? XmlFolder { get; set; }
    public string? CsvFolder { get; set; }
    public string? OutputConfig { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public bool ShowHelp { get; set; }
}

public class OutputConfig
{
    public string OutputFolder { get; set; } = "./metadata/{SYSTEM}";
    public string JsonFolder { get; set; } = "./metadata/{SYSTEM}/json";
    public string XmlFolder { get; set; } = "./metadata/{SYSTEM}/xml";
    public string CsvFolder { get; set; } = "./metadata/{SYSTEM}/csv";
    public bool ExportJson { get; set; } = true;
    public bool ExportXml { get; set; } = false;
    public bool ExportCsv { get; set; } = false;
}

public class RomFile
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string System { get; set; } = "";
    public string Crc32 { get; set; } = "";
    public long Size { get; set; }
}

public class GameMetadata
{
    public int Id { get; set; }
    public string RomPath { get; set; } = "";
    public string GameName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string Developer { get; set; } = "";
    public string Genre { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public string Rating { get; set; } = "";
    public string Players { get; set; } = "";
    public string System { get; set; } = "";
    public string Region { get; set; } = "";
    public string Language { get; set; } = "";
    public DateTime ScrapedAt { get; set; }
    public bool Exported { get; set; }
}

// ScreenScraper API Response Models (same as XtraImageScrapper)
public class ScreenScraperResponse
{
    public Header? Header { get; set; }
    public Response? Response { get; set; }
}

public class Header
{
    public string? APIversion { get; set; }
    public string? DateTime { get; set; }
    public string? CommandRequested { get; set; }
    public string? Success { get; set; }
    public string? Error { get; set; }
}

public class Response
{
    public Jeu? Jeu { get; set; }
}

public class Jeu
{
    public string? Id { get; set; }
    public string? Romnom { get; set; }
    public string? Systeme { get; set; }
    public Nom[]? Noms { get; set; }
    public Synopsis[]? Synopsis { get; set; }
    public Classification[]? Classifications { get; set; }
    public Date[]? Dates { get; set; }
    public Editeur? Editeur { get; set; }
    public Developpeur? Developpeur { get; set; }
    public Joueur[]? Joueurs { get; set; }
    public Note[]? Notes { get; set; }
}

public class Nom
{
    public string? Region { get; set; }
    public string? Text { get; set; }
}

public class Synopsis
{
    public string? Langue { get; set; }
    public string? Text { get; set; }
}

public class Classification
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

public class Date
{
    public string? Region { get; set; }
    public string? Text { get; set; }
}

public class Editeur
{
    public string? Id { get; set; }
    public string? Text { get; set; }
}

public class Developpeur
{
    public string? Id { get; set; }
    public string? Text { get; set; }
}

public class Joueur
{
    public string? Text { get; set; }
}

public class Note
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}
