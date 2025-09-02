using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using XtraImageScrapper.Models;

namespace XtraImageScrapper.Services;

public interface IImageDownloader
{
    Task<DownloadResult> DownloadImageAsync(string url, string outputPath, CancellationToken cancellationToken = default);
    Task<List<DownloadResult>> DownloadImagesAsync(List<string> urls, string outputPath, int maxConcurrentDownloads, IProgress<DownloadResult>? progress = null, CancellationToken cancellationToken = default);
}

public class ImageDownloader : IImageDownloader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageDownloader> _logger;
    private readonly IStringLocalizer<ImageDownloader> _localizer;
    private readonly Settings _settings;

    public ImageDownloader(
        HttpClient httpClient,
        ILogger<ImageDownloader> logger,
        IStringLocalizer<ImageDownloader> localizer,
        Microsoft.Extensions.Options.IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _localizer = localizer;
        _settings = settings.Value.Settings;
    }

    public async Task<DownloadResult> DownloadImageAsync(string url, string outputPath, CancellationToken cancellationToken = default)
    {
        var result = new DownloadResult { Url = url };

        try
        {
            // Get filename from URL
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.AbsolutePath);
            
            // If no filename in URL, generate one
            if (string.IsNullOrEmpty(fileName) || !Path.HasExtension(fileName))
            {
                fileName = $"image_{Guid.NewGuid()}.jpg";
            }

            var filePath = Path.Combine(outputPath, fileName);

            // Check if file already exists
            if (File.Exists(filePath))
            {
                _logger.LogWarning(_localizer["Warn_FileExists"], fileName);
                result.Success = false;
                result.ErrorMessage = "File already exists";
                return result;
            }

            // Create directory if it doesn't exist
            Directory.CreateDirectory(outputPath);

            // Download the image
            using var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Check file size
            var contentLength = response.Content.Headers.ContentLength;
            if (contentLength.HasValue && contentLength.Value > _settings.MaxFileSizeMB * 1024 * 1024)
            {
                var sizeMB = contentLength.Value / (1024.0 * 1024.0);
                _logger.LogWarning(_localizer["Warn_FileTooLarge"], fileName, sizeMB.ToString("F2"));
                result.Success = false;
                result.ErrorMessage = $"File too large: {sizeMB:F2} MB";
                return result;
            }

            // Save the file
            using var fileStream = File.Create(filePath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            result.Success = true;
            result.FilePath = filePath;
            result.FileSizeBytes = new FileInfo(filePath).Length;

            _logger.LogInformation(_localizer["Success_Download"], fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer["Error_Download"], url, ex.Message);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    public async Task<List<DownloadResult>> DownloadImagesAsync(
        List<string> urls, 
        string outputPath, 
        int maxConcurrentDownloads, 
        IProgress<DownloadResult>? progress = null, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<DownloadResult>();
        using var semaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads);

        var tasks = urls.Select(async url =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await DownloadImageAsync(url, outputPath, cancellationToken);
                progress?.Report(result);
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var downloadResults = await Task.WhenAll(tasks);
        results.AddRange(downloadResults);

        return results;
    }
}