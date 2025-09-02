namespace XtraImageScrapper.Models;

public class AppSettings
{
    public Settings Settings { get; set; } = new();
}

public class Settings
{
    public string OutputPath { get; set; } = ".\\images";
    public int MaxConcurrentDownloads { get; set; } = 5;
    public string UserAgent { get; set; } = "XtraImageScrapper/0.0.1";
    public int TimeoutSeconds { get; set; } = 30;
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    public int MaxFileSizeMB { get; set; } = 10;
}

public class CommandLineArgs
{
    public string? Url { get; set; }
    public string? OutputPath { get; set; }
    public int? MaxConcurrentDownloads { get; set; }
    public bool ShowHelp { get; set; }
}

public class DownloadResult
{
    public string Url { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long FileSizeBytes { get; set; }
}

public class DownloadStatistics
{
    public int TotalFiles { get; set; }
    public int SuccessfulDownloads { get; set; }
    public int FailedDownloads { get; set; }
    public int SkippedFiles { get; set; }
    public long TotalBytesDownloaded { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}