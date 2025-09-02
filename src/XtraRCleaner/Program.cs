using XtraRCleaner.Models;
using XtraRCleaner.Services;
using CRCChecker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace XtraRCleaner;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
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
                services.AddLocalization(options => options.ResourcesPath = "Resources");
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
    private readonly IStringLocalizer<XtraRCleanerApp> _localizer;
    private readonly ILogger<XtraRCleanerApp> _logger;
    private readonly AppSettings _settings;

    public XtraRCleanerApp(
        IDatParser datParser,
        IRomProcessor romProcessor,
        IStringLocalizer<XtraRCleanerApp> localizer,
        ILogger<XtraRCleanerApp> logger,
        Microsoft.Extensions.Options.IOptions<AppSettings> settings)
    {
        _datParser = datParser;
        _romProcessor = romProcessor;
        _localizer = localizer;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<int> RunAsync(string[] args)
    {
        Console.WriteLine("XtraRCleaner - Limpador de ROMs");
        Console.WriteLine();

        var (isValid, outputPath, mode) = ParseArguments(args);
        
        if (!isValid)
        {
            Console.WriteLine("Argumentos inválidos. Use: XtraRCleaner --output <caminho> [--backup] [--purge]");
            return 1;
        }

        _logger.LogInformation("Iniciando processo de limpeza...");
        Console.WriteLine("Iniciando processo de limpeza...");

        // Parse DAT file
        var datPath = Path.GetFullPath(_settings.Settings.DatFilePath);
        if (!File.Exists(datPath))
        {
            var error = $"Arquivo DAT não encontrado: {datPath}";
            Console.WriteLine(error);
            _logger.LogError(error);
            return 1;
        }

        var datRoms = await _datParser.ParseDatFileAsync(datPath);
        _logger.LogInformation("Loaded {Count} ROMs from DAT file", datRoms.Count);

        // Process ROMs
        var progress = new Progress<string>(message =>
        {
            Console.WriteLine($"Processando: {message}");
        });

        var (processed, unique, duplicates) = await _romProcessor.ProcessRomsAsync(
            outputPath!, mode, datRoms, progress);

        var completedMessage = $"Processo concluído! Processados: {processed}, Únicos: {unique}, Duplicados: {duplicates}";
        Console.WriteLine();
        Console.WriteLine(completedMessage);
        _logger.LogInformation(completedMessage);

        return 0;
    }

    private (bool isValid, string? outputPath, ProcessMode mode) ParseArguments(string[] args)
    {
        string? outputPath = null;
        var mode = ProcessMode.Move;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--output" when i + 1 < args.Length:
                    outputPath = args[++i];
                    break;
                case "--backup":
                    mode = ProcessMode.Backup;
                    break;
                case "--purge":
                    mode = ProcessMode.Purge;
                    break;
            }
        }

        return (outputPath != null, outputPath, mode);
    }
}

// Extension method for file logging
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
        _logFilePath = string.Format("CRCChecker_{0:yyyyMMdd_HHmmss}.log", DateTime.Now);
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
