using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Globalization;
using XtraImageScrapper.Models;
using XtraImageScrapper.Services;

namespace XtraImageScrapper;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Set culture for localization
        var culture = CultureInfo.CurrentCulture.Name.StartsWith("pt") ? "pt" : "en";
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);

        var host = CreateHostBuilder(args).Build();

        try
        {
            var app = host.Services.GetRequiredService<XtraImageScrapperApp>();
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fatal error: {ex.Message}");
            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<AppSettings>(context.Configuration);
                services.AddLocalization(options => options.ResourcesPath = "Resources");
                services.AddHttpClient<IImageDownloader, ImageDownloader>(client =>
                {
                    var settings = context.Configuration.Get<AppSettings>()?.Settings ?? new Settings();
                    client.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
                    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
                });
                services.AddScoped<IUrlParser, UrlParser>();
                services.AddScoped<XtraImageScrapperApp>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
}

public class XtraImageScrapperApp
{
    private readonly IImageDownloader _imageDownloader;
    private readonly IUrlParser _urlParser;
    private readonly IStringLocalizer<XtraImageScrapperApp> _localizer;
    private readonly ILogger<XtraImageScrapperApp> _logger;
    private readonly AppSettings _settings;

    public XtraImageScrapperApp(
        IImageDownloader imageDownloader,
        IUrlParser urlParser,
        IStringLocalizer<XtraImageScrapperApp> localizer,
        ILogger<XtraImageScrapperApp> logger,
        Microsoft.Extensions.Options.IOptions<AppSettings> settings)
    {
        _imageDownloader = imageDownloader;
        _urlParser = urlParser;
        _localizer = localizer;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<int> RunAsync(string[] args)
    {
        var cmdArgs = ParseArguments(args);

        if (cmdArgs.ShowHelp)
        {
            ShowHelp();
            return 0;
        }

        if (string.IsNullOrEmpty(cmdArgs.Url))
        {
            Console.WriteLine("❌ URL é obrigatória. Use --help para ver as opções.");
            return 1;
        }

        try
        {
            Console.WriteLine(_localizer["Info_Starting"]);
            
            var outputPath = cmdArgs.OutputPath ?? _settings.Settings.OutputPath;
            var maxConcurrent = cmdArgs.MaxConcurrentDownloads ?? _settings.Settings.MaxConcurrentDownloads;

            // Parse URLs
            var urls = await _urlParser.ParseUrlsAsync(cmdArgs.Url);
            
            if (urls.Count == 0)
            {
                Console.WriteLine("❌ Nenhuma URL válida encontrada.");
                return 1;
            }

            // Filter valid image URLs
            var imageUrls = urls.Where(url => _urlParser.IsValidImageExtension(url, _settings.Settings.AllowedExtensions)).ToList();
            
            if (imageUrls.Count == 0)
            {
                Console.WriteLine("❌ Nenhuma URL de imagem válida encontrada.");
                return 1;
            }

            Console.WriteLine($"📊 Total de URLs encontradas: {urls.Count}");
            Console.WriteLine($"🖼️ URLs de imagens válidas: {imageUrls.Count}");
            Console.WriteLine($"📁 Pasta de destino: {outputPath}");
            Console.WriteLine($"⚡ Downloads simultâneos: {maxConcurrent}");
            Console.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            var stats = new DownloadStatistics { TotalFiles = imageUrls.Count };

            // Download images with progress reporting
            var progress = new Progress<DownloadResult>(result =>
            {
                if (result.Success)
                {
                    stats.SuccessfulDownloads++;
                    stats.TotalBytesDownloaded += result.FileSizeBytes;
                    Console.WriteLine(_localizer["Success_Download"], Path.GetFileName(result.FilePath));
                }
                else
                {
                    stats.FailedDownloads++;
                    Console.WriteLine($"❌ Falha: {result.Url} - {result.ErrorMessage}");
                }
            });

            await _imageDownloader.DownloadImagesAsync(imageUrls, outputPath, maxConcurrent, progress);

            stopwatch.Stop();
            stats.ElapsedTime = stopwatch.Elapsed;

            // Show final statistics
            ShowStatistics(stats);

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during execution");
            Console.WriteLine($"❌ Erro: {ex.Message}");
            return 1;
        }
    }

    private CommandLineArgs ParseArguments(string[] args)
    {
        var cmdArgs = new CommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--url":
                    if (i + 1 < args.Length)
                    {
                        cmdArgs.Url = args[++i];
                    }
                    break;
                case "--output":
                    if (i + 1 < args.Length)
                    {
                        cmdArgs.OutputPath = args[++i];
                    }
                    break;
                case "--concurrent":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out var concurrent))
                    {
                        cmdArgs.MaxConcurrentDownloads = concurrent;
                    }
                    break;
                case "--help":
                case "-h":
                case "/?":
                    cmdArgs.ShowHelp = true;
                    break;
            }
        }

        return cmdArgs;
    }

    private void ShowHelp()
    {
        Console.WriteLine(_localizer["AppName"] + " - " + _localizer["AppDescription"]);
        Console.WriteLine(_localizer["Help_Usage"]);
        Console.WriteLine();
        Console.WriteLine(_localizer["Help_Options"]);
        Console.WriteLine(_localizer["Help_Url"]);
        Console.WriteLine("                     Se não usado, deve especificar via --url");
        Console.WriteLine();
        Console.WriteLine(_localizer["Help_Output"]);
        Console.WriteLine("                     Se não usado, usa appsettings.json");
        Console.WriteLine("                     Cria pasta se não existir");
        Console.WriteLine();
        Console.WriteLine(_localizer["Help_Concurrent"]);
        Console.WriteLine("                     Controla quantos downloads simultâneos");
        Console.WriteLine();
        Console.WriteLine(_localizer["Help_Help"]);
        Console.WriteLine();
        Console.WriteLine(_localizer["Help_Examples"]);
        Console.WriteLine(_localizer["Help_Example1"]);
        Console.WriteLine(_localizer["Help_Example2"]);
        Console.WriteLine("  XtraImageScrapper.exe --url \"urls.txt\" --concurrent 10");
    }

    private void ShowStatistics(DownloadStatistics stats)
    {
        Console.WriteLine();
        Console.WriteLine("📊 ESTATÍSTICAS FINAIS:");
        Console.WriteLine($"   Total de arquivos: {stats.TotalFiles}");
        Console.WriteLine($"   ✅ Sucessos: {stats.SuccessfulDownloads}");
        Console.WriteLine($"   ❌ Falhas: {stats.FailedDownloads}");
        Console.WriteLine($"   ⏱️ Tempo decorrido: {stats.ElapsedTime:mm\\:ss}");
        Console.WriteLine($"   📦 Total baixado: {FormatBytes(stats.TotalBytesDownloaded)}");
        Console.WriteLine();
        Console.WriteLine(_localizer["Info_Completed", stats.SuccessfulDownloads]);
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number = number / 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}
