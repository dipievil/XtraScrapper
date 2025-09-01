using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using CRCChecker;

public record GameInfo(string Name, string Crc);

public class CommandLineArgs
{
    public string? RomsFolder { get; set; }
    public string? DatFile { get; set; }
    public bool MoveToSystemFolder { get; set; }
    public bool ShowHelp { get; set; }
}



class Program
{
    static CommandLineArgs ParseCommandLineArgs(string[] args)
    {
        var cmdArgs = new CommandLineArgs();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--folder":
                    if (i + 1 < args.Length)
                    {
                        cmdArgs.RomsFolder = args[++i];
                    }
                    break;
                case "--dat":
                    if (i + 1 < args.Length)
                    {
                        cmdArgs.DatFile = args[++i];
                    }
                    break;
                case "--move-sys":
                    cmdArgs.MoveToSystemFolder = true;
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

    static void ShowHelp()
    {
        Console.WriteLine("XtraScrapper - ROM File Organizer");
        Console.WriteLine("Usage: XtraScrapper.exe [options]");
        Console.WriteLine("");
        Console.WriteLine("Options:");
        Console.WriteLine("  --folder <path>    Path to ROMs folder");
        Console.WriteLine("                     If not used, uses appsettings.json");
        Console.WriteLine("                     Creates folder if it doesn't exist");
        Console.WriteLine("");
        Console.WriteLine("  --dat <path>       Path to DAT file");
        Console.WriteLine("                     If not used, uses appsettings.json");
        Console.WriteLine("                     File must exist or app will exit with error");
        Console.WriteLine("");
        Console.WriteLine("  --move-sys         Move ROMs to system subfolder");
        Console.WriteLine("                     Creates subfolder based on DAT system name");
        Console.WriteLine("                     Folder name from <datafile><header><name>");
        Console.WriteLine("");
        Console.WriteLine("  --help, -h, /?     Show this help message");
        Console.WriteLine("");
        Console.WriteLine("Examples:");
        Console.WriteLine("  XtraScrapper.exe --folder \"C:\\ROMs\" --dat \"games.dat\"");
        Console.WriteLine("  XtraScrapper.exe --move-sys");
        Console.WriteLine("  XtraScrapper.exe --folder \"C:\\ROMs\" --move-sys");
    }

    static string ExtractSystemNameFromDat(string datFilePath)
    {
        try
        {
            XDocument datFile = XDocument.Load(datFilePath);
            var systemName = datFile.Descendants("header")
                                   .Elements("name")
                                   .FirstOrDefault()?.Value;
            
            if (!string.IsNullOrEmpty(systemName))
            {
                // Remove caracteres inválidos para nome de pasta
                var invalidChars = Path.GetInvalidFileNameChars();
                foreach (char c in invalidChars)
                {
                    systemName = systemName.Replace(c, '_');
                }
                return systemName;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao extrair nome do sistema do DAT: {ex.Message}");
        }
        
        return "Unknown_System";
    }
    static string CalculateFileCrc(string filePath, ICrc32Service crcService)
    {
        string fileExtension = Path.GetExtension(filePath).ToLower();
        
        if (fileExtension == ".zip")
        {
            return CalculateZipCrc(filePath, crcService);
        }
        else
        {
            return CalculateRegularFileCrc(filePath, crcService);
        }
    }

    static string CalculateZipCrc(string zipFilePath, ICrc32Service crcService)
    {
        using (var archive = ZipFile.OpenRead(zipFilePath))
        {
            // Procura por arquivos ROM comuns no ZIP (ignorando .txt, .nfo, etc.)
            var romExtensions = new[] { ".sms", ".rom", ".bin", ".sg", ".col" };
            var romEntry = archive.Entries.FirstOrDefault(entry => 
                romExtensions.Contains(Path.GetExtension(entry.Name).ToLower()) && 
                entry.Length > 0);
            
            if (romEntry == null)
            {
                // Se não encontrou extensão específica, pega o primeiro arquivo que não seja texto
                romEntry = archive.Entries.FirstOrDefault(entry => 
                    !Path.GetExtension(entry.Name).ToLower().StartsWith(".txt") &&
                    !Path.GetExtension(entry.Name).ToLower().StartsWith(".nfo") &&
                    entry.Length > 0);
            }
            
            if (romEntry != null)
            {
                using (var entryStream = romEntry.Open())
                {
                    crcService.Reset();
                    crcService.Append(entryStream);
                    byte[] hashBytes = crcService.GetHashAndReset();
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        
        return "00000000"; // CRC padrão se não encontrar arquivo válido
    }

    static string CalculateRegularFileCrc(string filePath, ICrc32Service crcService)
    {
        using (var fileStream = File.OpenRead(filePath))
        {
            crcService.Reset();
            crcService.Append(fileStream);
            byte[] hashBytes = crcService.GetHashAndReset();
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
    static void Main(string[] args)
    {
        var cmdArgs = ParseCommandLineArgs(args);
        
        if (cmdArgs.ShowHelp)
        {
            ShowHelp();
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        
        // Use argumentos de linha de comando ou fallback para appsettings
        string datFilePath = cmdArgs.DatFile ?? configuration["DatFilePath"] ?? "games.dat";
        string romsFolderPath = cmdArgs.RomsFolder ?? configuration["RomsFolderPath"] ?? "roms";

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<ICrc32Service, Crc32Service>();
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();
        var stringLocalizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        var crcService = serviceProvider.GetRequiredService<ICrc32Service>();

        IStringLocalizer localizer = stringLocalizerFactory.Create("Resources", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!);
        
        Console.WriteLine(localizer["StartingMessage"]);
        
        // Verificar se o arquivo DAT existe
        if (!File.Exists(datFilePath))
        {
            Console.WriteLine(localizer["ErrorDatNotFound", datFilePath]);
            Environment.Exit(1);
        }
        
        // Verificar/criar pasta de ROMs
        if (!Directory.Exists(romsFolderPath))
        {
            try
            {
                Directory.CreateDirectory(romsFolderPath);
                Console.WriteLine($"📁 Pasta criada: {romsFolderPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao criar pasta '{romsFolderPath}': {ex.Message}");
                Environment.Exit(1);
            }
        }

        // Se --move-sys foi especificado, criar subpasta do sistema
        string finalRomsPath = romsFolderPath;
        if (cmdArgs.MoveToSystemFolder)
        {
            string systemName = ExtractSystemNameFromDat(datFilePath);
            finalRomsPath = Path.Combine(romsFolderPath, systemName);
            
            if (!Directory.Exists(finalRomsPath))
            {
                try
                {
                    Directory.CreateDirectory(finalRomsPath);
                    Console.WriteLine($"📁 Subpasta do sistema criada: {finalRomsPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao criar subpasta do sistema '{finalRomsPath}': {ex.Message}");
                    Environment.Exit(1);
                }
            }
        }
        
        Console.WriteLine(localizer["LoadingDat", datFilePath]);
        XDocument datFile = XDocument.Load(datFilePath);

        var gamesList = datFile.Descendants("game")
                               .Select(g => new GameInfo(
                                   g.Attribute("name")?.Value ?? "Nome não encontrado", 
                                   g.Element("rom")?.Attribute("crc")?.Value?.ToLower() ?? "CRC não encontrado"
                               ))
                               .Where(g => g.Crc != "CRC não encontrado")
                               .ToList();

        Console.WriteLine($"\n{localizer["GamesLoaded", gamesList.Count]}");

        Console.WriteLine($"\n{localizer["SearchingRoms"]}");        

        string[] romFiles = Directory.GetFiles(romsFolderPath);
        Console.WriteLine($"{localizer["FilesFound", romFiles.Length, romsFolderPath]}\n");

        int renamedCount = 0;
        int movedCount = 0;
        var logEntries = new List<string>();
        string logFileName = $"XtraScrapper_{DateTime.Now:yyyyMMdd_HHmmss}.log";

        foreach (string filePath in romFiles)
        {
            string hashString = CalculateFileCrc(filePath, crcService);

            var matchingGame = gamesList.FirstOrDefault(game => game.Crc == hashString);

            if (matchingGame != null)
            {
                string oldFileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(oldFileName);
                string newFileName = $"{matchingGame.Name}{fileExtension}";
                
                // Determinar o caminho de destino
                string targetPath = cmdArgs.MoveToSystemFolder ? finalRomsPath : romsFolderPath;
                string newFilePath = Path.Combine(targetPath, newFileName);
                
                // Verificar se precisa renomear ou mover
                bool needsRename = oldFileName != newFileName;
                bool needsMove = cmdArgs.MoveToSystemFolder && Path.GetDirectoryName(filePath) != finalRomsPath;
                
                if (!needsRename && !needsMove)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(localizer["AlreadyCorrect", oldFileName]);
                    Console.ResetColor();
                    logEntries.Add($"{oldFileName} >> OK");
                }
                else
                {
                    try
                    {
                        File.Move(filePath, newFilePath);
                        
                        Console.ForegroundColor = ConsoleColor.Green;
                        if (needsRename && needsMove)
                        {
                            Console.WriteLine($"✅ Renomeado e movido: '{oldFileName}' >> '{newFileName}' [{Path.GetFileName(targetPath)}]");
                            logEntries.Add($"{oldFileName} >> {newFileName} [MOVED TO {Path.GetFileName(targetPath)}]");
                            renamedCount++;
                            movedCount++;
                        }
                        else if (needsRename)
                        {
                            Console.WriteLine(localizer["SuccessRenamed", oldFileName, newFileName]);
                            logEntries.Add($"{oldFileName} >> {newFileName}");
                            renamedCount++;
                        }
                        else if (needsMove)
                        {
                            Console.WriteLine($"📁 Movido: '{oldFileName}' >> [{Path.GetFileName(targetPath)}]");
                            logEntries.Add($"{oldFileName} >> MOVED TO {Path.GetFileName(targetPath)}");
                            movedCount++;
                        }
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ Erro ao processar '{oldFileName}': {ex.Message}");
                        Console.ResetColor();
                        logEntries.Add($"{oldFileName} >> ERROR: {ex.Message}");
                        
                        if (cmdArgs.MoveToSystemFolder && movedCount == 0 && renamedCount == 0)
                        {
                            Console.WriteLine("❌ Erro crítico no primeiro arquivo. Cancelando operação.");
                            Environment.Exit(1);
                        }
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(localizer["InfoNotFound", Path.GetFileName(filePath), hashString]);
                Console.ResetColor();
                logEntries.Add($"{Path.GetFileName(filePath)} >> NOT FOUND (CRC: {hashString})");
            }
        }
        
        Console.WriteLine($"\n{localizer["ProcessingComplete"]}");
        if (cmdArgs.MoveToSystemFolder)
        {
            Console.WriteLine($"📊 Relatório: {renamedCount} renomeados, {movedCount} movidos de {romFiles.Length} arquivos.");
        }
        else
        {
            Console.WriteLine(localizer["Report", renamedCount, romFiles.Length]);
        }
        
        // Salva o log da execução
        if (logEntries.Count > 0)
        {
            var logContent = new List<string>
            {
                $"XtraScrapper Log - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"DAT File: {datFilePath}",
                $"ROMs Folder: {romsFolderPath}",
                $"System Folder: {(cmdArgs.MoveToSystemFolder ? finalRomsPath : "Not used")}",
                $"Total Files: {romFiles.Length}",
                $"Renamed Files: {renamedCount}",
                $"Moved Files: {movedCount}",
                "",
                "Operations:"
            };
            
            logContent.AddRange(logEntries);
            
            File.WriteAllLines(logFileName, logContent);
            Console.WriteLine($"\n📝 Log salvo em: {logFileName}");
        }
    }
}