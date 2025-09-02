namespace XtraImageScrapper.Models;

public class AppSettings
{
    public Settings Settings { get; set; } = new();
}

public class Settings
{
    public string RomsFolder { get; set; } = "./roms";
    public string DatabasePath { get; set; } = "XtraImageScrapper.db";
    public string LogFilePath { get; set; } = "XtraImageScrapper_{0:yyyyMMdd_HHmmss}.log";
    public string FolderConfigPath { get; set; } = "folder-config.json";
    public string ScreenScraperUser { get; set; } = "";
    public string ScreenScraperPassword { get; set; } = "";
    public string ScreenScraperApiKey { get; set; } = "";
    public int MaxRequestsPerSecond { get; set; } = 1;
    public int TimeoutSeconds { get; set; } = 30;
}

public class CommandLineArgs
{
    public string? RomsFolder { get; set; }
    public string? ImageFolder { get; set; }
    public string? BoxFolder { get; set; }
    public string? PrintFolder { get; set; }
    public string? ThumbFolder { get; set; }
    public string? FolderConfig { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    public string? ApiKey { get; set; }
    public bool ShowHelp { get; set; }
}

public class FolderConfig
{
    public string ImageFolder { get; set; } = "./roms/{SYSTEM}/images";
    public string BoxFolder { get; set; } = "./roms/{SYSTEM}/images";
    public string PrintFolder { get; set; } = "./roms/{SYSTEM}/images";
    public string MainImagesFolder { get; set; } = "./roms/{SYSTEM}/images";
    public string ThumbFolder { get; set; } = "./roms/{SYSTEM}/images";
}

public class RomFile
{
    public string FilePath { get; set; } = "";
    public string FileName { get; set; } = "";
    public string System { get; set; } = "";
    public string Crc32 { get; set; } = "";
    public long Size { get; set; }
}

public class ScrapedImage
{
    public int Id { get; set; }
    public string RomPath { get; set; } = "";
    public string ImageType { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string LocalPath { get; set; } = "";
    public DateTime ScrapedAt { get; set; }
    public bool Downloaded { get; set; }
}

public class ScreenScraperResponse
{
    public Header? Header { get; set; }
    public Response? Response { get; set; }
}

public class Header
{
    public string? APIversion { get; set; }
    public DateTime DateTime { get; set; }
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
    public int Id { get; set; }
    public string? Nom { get; set; }
    public Media[]? Medias { get; set; }
}

public class Media
{
    public string? Type { get; set; }
    public string? Url { get; set; }
    public string? Format { get; set; }
}