using System.Text.Json;
using Microsoft.Extensions.Logging;
using XtraImageScrapper.Models;

namespace XtraImageScrapper.Services;

public interface IImageDownloader
{
    Task DownloadImagesAsync(RomFile romFile, FolderConfig folderConfig);
}

public class ImageDownloader : IImageDownloader
{
    private readonly IScreenScraperService _screenScraperService;
    private readonly IDatabaseService _databaseService;
    private readonly ILogger<ImageDownloader> _logger;

    public ImageDownloader(
        IScreenScraperService screenScraperService,
        IDatabaseService databaseService,
        ILogger<ImageDownloader> logger)
    {
        _screenScraperService = screenScraperService;
        _databaseService = databaseService;
        _logger = logger;
    }

    public async Task DownloadImagesAsync(RomFile romFile, FolderConfig folderConfig)
    {
        _logger.LogInformation("Processing ROM: {RomName} (System: {System})", 
            romFile.FileName, romFile.System);

        // Check if we already have cached images
        var cachedImages = await _databaseService.GetCachedImagesAsync(romFile.FilePath);
        if (cachedImages.Any(i => i.Downloaded))
        {
            _logger.LogInformation("Images already downloaded for: {RomName}", romFile.FileName);
            return;
        }

        // Search for the game on ScreenScraper
        var gameInfo = await _screenScraperService.SearchGameAsync(
            romFile.FileName, romFile.Crc32, romFile.System);

        if (gameInfo?.Response?.Jeu?.Medias == null)
        {
            _logger.LogWarning("No media found for ROM: {RomName}", romFile.FileName);
            return;
        }

        // Download different types of images
        var imageTypes = new Dictionary<string, string>
        {
            ["box-2D"] = GetFolderPath(folderConfig.BoxFolder, romFile.System),
            ["ss"] = GetFolderPath(folderConfig.PrintFolder, romFile.System),
            ["titre"] = GetFolderPath(folderConfig.MainImagesFolder, romFile.System),
            ["wheel"] = GetFolderPath(folderConfig.ThumbFolder, romFile.System),
            ["fanart"] = GetFolderPath(folderConfig.SplashFolder, romFile.System),
            ["screenmarquee"] = GetFolderPath(folderConfig.PreviewFolder, romFile.System)
        };

        foreach (var media in gameInfo.Response.Jeu.Medias)
        {
            if (media.Type == null || media.Url == null) continue;

            if (imageTypes.TryGetValue(media.Type, out var targetFolder))
            {
                await DownloadSingleImageAsync(romFile, media, targetFolder, media.Type);
            }
        }
    }

    private async Task DownloadSingleImageAsync(RomFile romFile, Media media, string targetFolder, string imageType)
    {
        try
        {
            // Check if already cached
            if (await _databaseService.IsImageCachedAsync(romFile.FilePath, imageType))
            {
                _logger.LogDebug("Image already cached: {ImageType} for {RomName}", imageType, romFile.FileName);
                return;
            }

            // Ensure target directory exists
            Directory.CreateDirectory(targetFolder);

            // Generate filename
            var extension = GetImageExtension(media.Format ?? "png");
            var romNameWithoutExtension = Path.GetFileNameWithoutExtension(romFile.FileName);
            var filename = $"{SanitizeFilename(romNameWithoutExtension)}.{extension}";
            var localPath = Path.Combine(targetFolder, filename);

            // Skip if file already exists locally
            if (File.Exists(localPath))
            {
                _logger.LogInformation("Image file already exists: {LocalPath}", localPath);
                
                // Cache the existing file
                await _databaseService.CacheImageAsync(new ScrapedImage
                {
                    RomPath = romFile.FilePath,
                    ImageType = imageType,
                    ImageUrl = media.Url!,
                    LocalPath = localPath,
                    ScrapedAt = DateTime.Now,
                    Downloaded = true
                });
                return;
            }

            // Download the image
            var imageData = await _screenScraperService.DownloadImageAsync(media.Url!);
            if (imageData == null)
            {
                _logger.LogWarning("Failed to download image: {ImageType} for {RomName}", imageType, romFile.FileName);
                return;
            }

            // Save the image file
            await File.WriteAllBytesAsync(localPath, imageData);
            _logger.LogInformation("Downloaded image: {ImageType} for {RomName} -> {LocalPath}", 
                imageType, romFile.FileName, localPath);

            // Cache the download
            await _databaseService.CacheImageAsync(new ScrapedImage
            {
                RomPath = romFile.FilePath,
                ImageType = imageType,
                ImageUrl = media.Url!,
                LocalPath = localPath,
                ScrapedAt = DateTime.Now,
                Downloaded = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image: {ImageType} for {RomName}", imageType, romFile.FileName);
        }
    }

    private string GetFolderPath(string template, string systemName)
    {
        return template.Replace("{SYSTEM}", systemName);
    }

    private string GetImageExtension(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => "jpg",
            "png" => "png",
            "gif" => "gif",
            "webp" => "webp",
            _ => "png"
        };
    }

    private string SanitizeFilename(string filename)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(filename
            .Where(c => !invalidChars.Contains(c))
            .ToArray());
        
        return string.IsNullOrWhiteSpace(sanitized) ? "unknown" : sanitized;
    }
}