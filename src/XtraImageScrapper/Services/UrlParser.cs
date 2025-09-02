using XtraImageScrapper.Models;

namespace XtraImageScrapper.Services;

public interface IUrlParser
{
    Task<List<string>> ParseUrlsAsync(string input);
    bool IsValidUrl(string url);
    bool IsValidImageExtension(string url, string[] allowedExtensions);
}

public class UrlParser : IUrlParser
{
    public async Task<List<string>> ParseUrlsAsync(string input)
    {
        var urls = new List<string>();

        // Check if input is a file path
        if (File.Exists(input))
        {
            var lines = await File.ReadAllLinesAsync(input);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
                {
                    if (IsValidUrl(trimmedLine))
                    {
                        urls.Add(trimmedLine);
                    }
                }
            }
        }
        // Check if input is a URL
        else if (IsValidUrl(input))
        {
            urls.Add(input);
        }

        return urls;
    }

    public bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public bool IsValidImageExtension(string url, string[] allowedExtensions)
    {
        try
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath;
            var extension = Path.GetExtension(path).ToLowerInvariant();
            
            return allowedExtensions.Contains(extension);
        }
        catch
        {
            return false;
        }
    }
}