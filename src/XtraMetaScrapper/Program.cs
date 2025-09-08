using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using XtraMetaScrapper.Models;
using XtraMetaScrapper.Services;

namespace XtraMetaScrapper;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            var host = CreateHostBuilder(args).Build();
            var app = host.Services.GetRequiredService<XtraMetaScrapperApp>();
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
                services.AddScoped<IMetadataExtractor, MetadataExtractor>();
                services.AddScoped<XtraMetaScrapperApp>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddFile();
            });
}

public class XtraMetaScrapperApp
{
    private readonly IRomScanner _romScanner;
    private readonly IMetadataExtractor _metadataExtractor;
    private readonly IDatabaseService _databaseService;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<XtraMetaScrapperApp> _logger;
    private readonly Settings _settings;

    public XtraMetaScrapperApp(
        IRomScanner romScanner,
        IMetadataExtractor metadataExtractor,
        IDatabaseService databaseService,
        IStringLocalizerFactory localizerFactory,
        ILogger<XtraMetaScrapperApp> logger,
        Settings settings)
    {
        _romScanner = romScanner;
        _metadataExtractor = metadataExtractor;
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
            if (string.IsNullOrEmpty(romsFolder) || !Directory.Exists(romsFolder))
            {
                // Inform user the folder is missing and provide guidance (do not create automatically)
                Console.WriteLine(_localizer["ErrorInvalidFolder", romsFolder ?? "null"]);
                Console.WriteLine(_localizer["ErrorInvalidFolderCreatePrompt"]);
                return 1;
            }

            // Load output configuration
            var outputConfig = await LoadOutputConfigAsync(cmdArgs);

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
                    await _metadataExtractor.ExtractMetadataAsync(romFile, outputConfig);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing ROM: {RomFile}", romFile.FileName);
                    Console.WriteLine(_localizer["ExtractionFailed", romFile.FileName]);
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
                case "--outputfolder" when i + 1 < args.Length:
                    cmdArgs.OutputFolder = args[++i];
                    break;
                case "--jsonfolder" when i + 1 < args.Length:
                    cmdArgs.JsonFolder = args[++i];
                    break;
                case "--xmlfolder" when i + 1 < args.Length:
                    cmdArgs.XmlFolder = args[++i];
                    break;
                case "--csvfolder" when i + 1 < args.Length:
                    cmdArgs.CsvFolder = args[++i];
                    break;
                case "--outputconfig" when i + 1 < args.Length:
                    cmdArgs.OutputConfig = args[++i];
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
            }
        }

        return cmdArgs;
    }

    private async Task<OutputConfig> LoadOutputConfigAsync(CommandLineArgs cmdArgs)
    {
        var outputConfig = new OutputConfig();

        // Load from JSON file if specified
        var configPath = cmdArgs.OutputConfig ?? _settings.OutputConfigPath;
        if (File.Exists(configPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(configPath);
                var loadedConfig = JsonSerializer.Deserialize<OutputConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (loadedConfig != null)
                {
                    outputConfig = loadedConfig;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to load output config from {ConfigPath}: {Error}", configPath, ex.Message);
            }
        }

        // Override with command line arguments
        if (!string.IsNullOrEmpty(cmdArgs.OutputFolder))
            outputConfig.OutputFolder = cmdArgs.OutputFolder;
        if (!string.IsNullOrEmpty(cmdArgs.JsonFolder))
            outputConfig.JsonFolder = cmdArgs.JsonFolder;
        if (!string.IsNullOrEmpty(cmdArgs.XmlFolder))
            outputConfig.XmlFolder = cmdArgs.XmlFolder;
        if (!string.IsNullOrEmpty(cmdArgs.CsvFolder))
            outputConfig.CsvFolder = cmdArgs.CsvFolder;

        return outputConfig;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("XtraMetaScrapper - ROM Metadata Extractor");
        Console.WriteLine("Usage: XtraMetaScrapper.exe [options]");
        Console.WriteLine("");
        Console.WriteLine("Options:");
        Console.WriteLine("  --folder <path>        Path to ROMs folder");
        Console.WriteLine("                         If not used, uses appsettings.json");
        Console.WriteLine("");
        Console.WriteLine("  --outputfolder <path>  Base folder for metadata output");
        Console.WriteLine("  --jsonfolder <path>    Folder for JSON metadata files");
        Console.WriteLine("  --xmlfolder <path>     Folder for XML metadata files");
        Console.WriteLine("  --csvfolder <path>     Folder for CSV metadata files");
        Console.WriteLine("");
        Console.WriteLine("  --outputconfig <path>  JSON file with output configuration");
        Console.WriteLine("                         Overrides individual folder parameters");
        Console.WriteLine("");
        Console.WriteLine("  --user <username>      ScreenScraper username");
        Console.WriteLine("  --password <password>  ScreenScraper password");
        Console.WriteLine("  --apikey <key>         ScreenScraper API key");
        Console.WriteLine("");
        Console.WriteLine("  --help, -h, /?         Show this help message");
        Console.WriteLine("");
        Console.WriteLine("Examples:");
        Console.WriteLine("  XtraMetaScrapper.exe --folder \"C:\\ROMs\"");
        Console.WriteLine("  XtraMetaScrapper.exe --outputconfig \"output-config.json\"");
        Console.WriteLine("  XtraMetaScrapper.exe --user \"myuser\" --password \"mypass\"");
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
    private readonly string _logPath = $"XtraMetaScrapper_{DateTime.Now:yyyyMMdd_HHmmss}.log";

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
