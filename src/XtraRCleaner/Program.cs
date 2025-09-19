using XtraRCleaner.Models;
using XtraRCleaner.Services;
using CRCChecker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Resources;

namespace XtraRCleaner;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Set culture for testing - remove this in production or make it configurable
        if (args.Length > 0 && args[0] == "--en")
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en");
            args = args.Skip(1).ToArray();
        }
        
        try
        {
            var host = CreateHostBuilder(args).Build();
            
            using var scope = host.Services.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<XtraRCleanerApp>();
            
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
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
                services.AddSingleton<ICrc32Service, Crc32Service>();
                services.AddScoped<IDatParser, DatParser>();
                services.AddScoped<IRomProcessor, RomProcessor>();
                services.AddScoped<XtraRCleanerApp>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddFile();
            });
}

public class XtraRCleanerApp
{
    private readonly IDatParser _datParser;
    private readonly IRomProcessor _romProcessor;
    private readonly SimpleLocalizer _localizer;
    private readonly ILogger<XtraRCleanerApp> _logger;
    private readonly AppSettings _settings;

    public XtraRCleanerApp(
        IDatParser datParser,
        IRomProcessor romProcessor,
        ILogger<XtraRCleanerApp> logger,
        Microsoft.Extensions.Options.IOptions<AppSettings> settings)
    {
        _datParser = datParser;
        _romProcessor = romProcessor;
        _localizer = new SimpleLocalizer();
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<int> RunAsync(string[] args)
    {
        Console.WriteLine(_localizer["AppTitle"]);
        Console.WriteLine();

        var (isValid, inputPath, outputPath, datPath, mode) = ParseArguments(args);
        
        if (!isValid)
        {
            Console.WriteLine(_localizer["InvalidArguments"]);
            Console.WriteLine(_localizer["Usage"]);
            return 1;
        }

        _logger.LogInformation(_localizer["StartingProcess"]);
        Console.WriteLine(_localizer["StartingProcess"]);
        Console.WriteLine(_localizer["InputFolder", inputPath!]);
        Console.WriteLine(_localizer["OutputFolder", outputPath!]);
        Console.WriteLine(_localizer["DatFile", datPath!]);
        Console.WriteLine();

        // Parse DAT file
        var datFilePath = Path.GetFullPath(datPath!);
        if (!File.Exists(datFilePath))
        {
            var error = _localizer["DatFileNotFound", datFilePath];
            Console.WriteLine(error);
            _logger.LogError(error);
            return 1;
        }

        var datRoms = await _datParser.ParseDatFileAsync(datFilePath);
        _logger.LogInformation(_localizer["LoadedRomsFromDat", datRoms.Count]);

        // Process ROMs
        var progress = new Progress<string>(message =>
        {
            Console.WriteLine(_localizer["ProcessingFile", message]);
        });

        var (processed, unique, duplicates) = await _romProcessor.ProcessRomsAsync(
            inputPath!, outputPath!, mode, datRoms, progress);

        var completedMessage = _localizer["ProcessCompleted", processed, unique, duplicates];
        Console.WriteLine();
        Console.WriteLine(completedMessage);
        _logger.LogInformation(completedMessage);

        return 0;
    }

    private (bool isValid, string? inputPath, string? outputPath, string? datPath, ProcessMode mode) ParseArguments(string[] args)
    {
        string? inputPath = null;
        string? outputPath = null;
        string? datPath = null;
        var mode = ProcessMode.Move;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--input" when i + 1 < args.Length:
                    inputPath = args[++i];
                    break;
                case "--output" when i + 1 < args.Length:
                    outputPath = args[++i];
                    break;
                case "--dat" when i + 1 < args.Length:
                    datPath = args[++i];
                    break;
                case "--backup":
                    mode = ProcessMode.Backup;
                    break;
                case "--purge":
                    mode = ProcessMode.Purge;
                    break;
            }
        }

        return (inputPath != null && outputPath != null && datPath != null, inputPath, outputPath, datPath, mode);
    }
}

public static class LoggingExtensions
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
    {
        builder.AddProvider(new FileLoggerProvider());
        return builder;
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;

    public FileLoggerProvider()
    {
        _logFilePath = string.Format("XtraRCleaner_{0:yyyyMMdd_HHmmss}.log", DateTime.Now);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_logFilePath);
    }

    public void Dispose() { }
}

public class FileLogger : ILogger
{
    private readonly string _filePath;

    public FileLogger(string filePath)
    {
        _filePath = filePath;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)}";
        
        try
        {
            File.AppendAllText(_filePath, message + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors
        }
    }
}
