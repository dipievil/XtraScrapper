using System.Text.Json;
using Microsoft.Extensions.Logging;
using XtraImageScrapper.Models;

namespace XtraImageScrapper.Services;

public interface IScreenScraperService
{
    Task<ScreenScraperResponse?> SearchGameAsync(string romName, string crc32, string systemName);
    Task<byte[]?> DownloadImageAsync(string imageUrl);
}

public class ScreenScraperService : IScreenScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScreenScraperService> _logger;
    private readonly Settings _settings;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private DateTime _lastRequest = DateTime.MinValue;

    public ScreenScraperService(
        HttpClient httpClient, 
        ILogger<ScreenScraperService> logger,
        Settings settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _settings = settings;
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "XtraImageScrapper/0.1.0");
    }

    public async Task<ScreenScraperResponse?> SearchGameAsync(string romName, string crc32, string systemName)
    {
        await EnforceRateLimit();

        try
        {
            var baseUrl = "https://www.screenscraper.fr/api2/jeuInfos.php";
            var systemId = GetScreenScraperSystemId(systemName);
            
            var queryParams = new List<string>
            {
                "devid=",
                "devpassword=",
                "softname=XtraImageScrapper",
                $"output=json",
                $"crc={crc32}",
                $"systemeid={systemId}",
                $"romnom={Uri.EscapeDataString(romName)}"
            };

            if (!string.IsNullOrEmpty(_settings.ScreenScraperUser))
            {
                queryParams.Add($"ssid={Uri.EscapeDataString(_settings.ScreenScraperUser)}");
            }
            
            if (!string.IsNullOrEmpty(_settings.ScreenScraperPassword))
            {
                queryParams.Add($"sspassword={Uri.EscapeDataString(_settings.ScreenScraperPassword)}");
            }

            var url = $"{baseUrl}?{string.Join("&", queryParams)}";
            
            _logger.LogInformation("Searching for game: {RomName} (CRC: {Crc32}, System: {SystemName})", 
                romName, crc32, systemName);

            var response = await _httpClient.GetStringAsync(url);
            var result = JsonSerializer.Deserialize<ScreenScraperResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Header?.Success == "true")
            {
                _logger.LogInformation("Found game in ScreenScraper: {GameName}", result.Response?.Jeu?.Nom);
                return result;
            }
            else
            {
                _logger.LogWarning("Game not found in ScreenScraper: {RomName} - {Error}", 
                    romName, result?.Header?.Error);
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error searching for game: {RomName}", romName);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for game: {RomName}", romName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error searching for game: {RomName}", romName);
            return null;
        }
    }

    public async Task<byte[]?> DownloadImageAsync(string imageUrl)
    {
        await EnforceRateLimit();

        try
        {
            _logger.LogInformation("Downloading image: {ImageUrl}", imageUrl);
            
            var response = await _httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading image: {ImageUrl}", imageUrl);
            return null;
        }
    }

    private async Task EnforceRateLimit()
    {
        await _rateLimitSemaphore.WaitAsync();
        
        try
        {
            var timeSinceLastRequest = DateTime.Now - _lastRequest;
            var minInterval = TimeSpan.FromSeconds(1.0 / _settings.MaxRequestsPerSecond);
            
            if (timeSinceLastRequest < minInterval)
            {
                var delay = minInterval - timeSinceLastRequest;
                await Task.Delay(delay);
            }
            
            _lastRequest = DateTime.Now;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    private string GetScreenScraperSystemId(string systemName)
    {
        // ScreenScraper system IDs - based on their API documentation
        return systemName.ToLowerInvariant() switch
        {
            "mastersystem" => "2",
            "gamegear" => "21", 
            "megadrive" => "1",
            "nes" => "3",
            "gameboy" => "9",
            "gameboycolor" => "10",
            "gameboyadvance" => "12",
            "nintendo64" => "14",
            "snes" => "4",
            "playstation" => "57",
            "saturn" => "22",
            "dreamcast" => "23",
            _ => "1" // Default to Mega Drive
        };
    }
}