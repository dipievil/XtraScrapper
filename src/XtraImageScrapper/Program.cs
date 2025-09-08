using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using XtraImageScrapper.Models;
using XtraImageScrapper.Services;

namespace XtraImageScrapper;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();
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
                services.AddHttpClient<IScreenScraperService, ScreenScraperService>();
                
                services.AddSingleton<Settings>(provider =>
                {
                    var config = context.Configuration.Get<AppSettings>();
                    return config?.Settings ?? new Settings();
                });
                
                services.AddSingleton<IDatabaseService>(provider =>
                {
                    var settings = provider.GetRequiredService<Settings>();
                    return new DatabaseService(settings.DatabasePath);
                });
                
                services.AddScoped<IRomScanner, RomScanner>();
                services.AddScoped<IImageDownloader, ImageDownloader>();
                services.AddScoped<XtraImageScrapperApp>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddFile();
            });
}

public class XtraImageScrapperApp
{
    private readonly IRomScanner _romScanner;
    private readonly IImageDownloader _imageDownloader;
    private readonly IDatabaseService _databaseService;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<XtraImageScrapperApp> _logger;
    private readonly Settings _settings;

    public XtraImageScrapperApp(
        IRomScanner romScanner,
        IImageDownloader imageDownloader,
        IDatabaseService databaseService,
        IStringLocalizerFactory localizerFactory,
        ILogger<XtraImageScrapperApp> logger,
        Settings settings)
    {
        _romScanner = romScanner;
        _imageDownloader = imageDownloader;
        _databaseService = databaseService;
        // Create a shared resource localizer from Resources/Resources.resx
        var assemblyName = typeof(Program).Assembly.GetName().Name ?? string.Empty;
        _localizer = localizerFactory.Create("Resources", assemblyName);
        _logger = logger;
        _settings = settings;
    }

    public async Task<int> RunAsync(string[] args)
    {
        var cmdArgs = ParseCommandLineArgs(args);

        if (cmdArgs.ShowHelp)
        {
            ShowHelp();
            return 0;
        }

        Console.WriteLine(_localizer["StartingMessage"]);

        try
        {
            // Initialize database
            await _databaseService.InitializeAsync();

            // Determine ROMs folder
            var romsFolder = cmdArgs.RomsFolder ?? _settings.RomsFolder;
            if (!Directory.Exists(romsFolder))
            {
                Console.WriteLine(_localizer["ErrorInvalidFolder", romsFolder]);
                return 1;
            }

            // Load folder configuration
            var folderConfig = await LoadFolderConfigAsync(cmdArgs);

            // Show summary of settings before proceeding (without counting files to avoid delay)
            Console.WriteLine();
            Console.WriteLine("## Configurações do processamento");
            Console.WriteLine($"- Pasta de Roms: {romsFolder}");
            Console.WriteLine($"- Arquivo de dados: {_settings.DatabasePath}");
            Console.WriteLine($"- Arquivo de log: {_settings.LogFilePath}");
            Console.WriteLine("- Outras configurações:");
            Console.WriteLine($"  - MaxRequestsPerSecond: {_settings.MaxRequestsPerSecond}");
            Console.WriteLine($"  - TimeoutSeconds: {_settings.TimeoutSeconds}");
            Console.WriteLine();
            Console.WriteLine("Outras informações: imagens serão baixadas/atualizadas conforme as regras do folder-config.");

            // If user didn't pass --y, ask for confirmation
            var autoYes = cmdArgs.User == "__auto_yes__";
            if (!autoYes)
            {
                Console.Write("Continuar? (Y/n): ");
                var answer = Console.ReadLine();
                if (!string.IsNullOrEmpty(answer) && answer.Trim().ToLowerInvariant() == "n")
                {
                    Console.WriteLine("Operação cancelada pelo usuário.");
                    return 0;
                }
            }

            // Scan ROMs
            Console.WriteLine(_localizer["ScanningFolder", romsFolder]);
            var romFiles = await _romScanner.ScanRomsAsync(romsFolder);
            var romList = romFiles.ToList();

            Console.WriteLine(_localizer["FoundRoms", romList.Count]);

            // Process each ROM
            int processedCount = 0;
            foreach (var romFile in romList)
            {
                Console.WriteLine(_localizer["ProcessingRom", romFile.FileName]);
                
                try
                {
                    await _imageDownloader.DownloadImagesAsync(romFile, folderConfig);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ROM: {RomFile}", romFile.FileName);
                    Console.WriteLine(_localizer["DownloadFailed", romFile.FileName]);
                }
            }

            Console.WriteLine(_localizer["ProcessingComplete", processedCount]);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application error");
            Console.WriteLine($"❌ Error: {ex.Message}");
            return 1;
        }
    }

    private CommandLineArgs ParseCommandLineArgs(string[] args)
    {
        var cmdArgs = new CommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--folder" when i + 1 < args.Length:
                    cmdArgs.RomsFolder = args[++i];
                    break;
                case "--imagefolder" when i + 1 < args.Length:
                    cmdArgs.ImageFolder = args[++i];
                    break;
                case "--boxfolder" when i + 1 < args.Length:
                    cmdArgs.BoxFolder = args[++i];
                    break;
                case "--printfolder" when i + 1 < args.Length:
                    cmdArgs.PrintFolder = args[++i];
                    break;
                case "--thumbfolder" when i + 1 < args.Length:
                    cmdArgs.ThumbFolder = args[++i];
                    break;
                case "--splashfolder" when i + 1 < args.Length:
                    cmdArgs.SplashFolder = args[++i];
                    break;
                case "--previewfolder" when i + 1 < args.Length:
                    cmdArgs.PreviewFolder = args[++i];
                    break;
                case "--folderconfig" when i + 1 < args.Length:
                    cmdArgs.FolderConfig = args[++i];
                    break;
                case "--user" when i + 1 < args.Length:
                    cmdArgs.User = args[++i];
                    break;
                case "--password" when i + 1 < args.Length:
                    cmdArgs.Password = args[++i];
                    break;
                case "--apikey" when i + 1 < args.Length:
                    cmdArgs.ApiKey = args[++i];
                    break;
                case "--help":
                case "-h":
                case "/?":
                    cmdArgs.ShowHelp = true;
                    break;
                case "--y":
                    // auto-yes flag to skip prompts
                    // stored in User property as a simple hack (avoid broad model change)
                    cmdArgs.User = "__auto_yes__";
                    break;
            }
        }

        return cmdArgs;
    }

    private async Task<FolderConfig> LoadFolderConfigAsync(CommandLineArgs cmdArgs)
    {
        var folderConfig = new FolderConfig();

        // Load from JSON file if specified
        var configPath = cmdArgs.FolderConfig ?? _settings.FolderConfigPath;
        if (File.Exists(configPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(configPath);
                var loadedConfig = JsonSerializer.Deserialize<FolderConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (loadedConfig != null)
                {
                    folderConfig = loadedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to load folder config from {ConfigPath}: {Error}", configPath, ex.Message);
            }
        }

        // Override with command line arguments
        if (!string.IsNullOrEmpty(cmdArgs.ImageFolder))
            folderConfig.ImageFolder = cmdArgs.ImageFolder;
        if (!string.IsNullOrEmpty(cmdArgs.BoxFolder))
            folderConfig.BoxFolder = cmdArgs.BoxFolder;
        if (!string.IsNullOrEmpty(cmdArgs.PrintFolder))
            folderConfig.PrintFolder = cmdArgs.PrintFolder;
        if (!string.IsNullOrEmpty(cmdArgs.ThumbFolder))
            folderConfig.ThumbFolder = cmdArgs.ThumbFolder;
        if (!string.IsNullOrEmpty(cmdArgs.SplashFolder))
            folderConfig.SplashFolder = cmdArgs.SplashFolder;
        if (!string.IsNullOrEmpty(cmdArgs.PreviewFolder))
            folderConfig.PreviewFolder = cmdArgs.PreviewFolder;

        return folderConfig;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("XtraImageScrapper - ROM Image Downloader");
        Console.WriteLine("Usage: XtraImageScrapper.exe [options]");
        Console.WriteLine("");
        Console.WriteLine("Options:");
        Console.WriteLine("  --folder <path>        Path to ROMs folder");
        Console.WriteLine("                         If not used, uses appsettings.json");
        Console.WriteLine("");
        Console.WriteLine("  --imagefolder <path>   Base folder for images");
        Console.WriteLine("  --boxfolder <path>     Folder for box images");
        Console.WriteLine("  --printfolder <path>   Folder for screenshot images");
        Console.WriteLine("  --thumbfolder <path>   Folder for thumbnail images");
        Console.WriteLine("  --splashfolder <path>  Folder for splash images");
        Console.WriteLine("  --previewfolder <path> Folder for preview images");
        Console.WriteLine("");
        Console.WriteLine("  --folderconfig <path>  JSON file with folder configuration");
        Console.WriteLine("                         Overrides individual folder parameters");
        Console.WriteLine("");
        Console.WriteLine("  --user <username>      ScreenScraper username");
        Console.WriteLine("  --password <password>  ScreenScraper password");
        Console.WriteLine("  --apikey <key>         ScreenScraper API key");
        Console.WriteLine("");
        Console.WriteLine("  --help, -h, /?         Show this help message");
        Console.WriteLine("");
        Console.WriteLine("Examples:");
        Console.WriteLine("  XtraImageScrapper.exe --folder \"C:\\ROMs\"");
        Console.WriteLine("  XtraImageScrapper.exe --folderconfig \"mustaros-config.json\"");
        Console.WriteLine("  XtraImageScrapper.exe --user \"myuser\" --password \"mypass\"");
    }
}

// Extension method for file logging
public static class LoggingExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
        return builder;
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger();
    }

    public void Dispose() { }
}

public class FileLogger : ILogger
{
    private readonly string _logPath = $"XtraImageScrapper_{DateTime.Now:yyyyMMdd_HHmmss}.log";

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}";
        if (exception != null)
        {
            message += Environment.NewLine + exception.ToString();
        }

        File.AppendAllText(_logPath, message + Environment.NewLine);
    }
}