using XtraRCleaner.Models;
using CRCChecker;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace XtraRCleaner.Services;

public interface IRomProcessor
{
    Task<(int processed, int unique, int duplicates)> ProcessRomsAsync(
        string inputPath,
        string outputPath, 
        ProcessMode mode, 
        Dictionary<string, RomEntry> datRoms,
        IProgress<string>? progress = null);
}

public class RomProcessor : IRomProcessor
{
    private readonly ICrc32Service _crcService;
    private readonly ILogger<RomProcessor> _logger;
    private readonly HashSet<string> _processedCrcs = new();

    public RomProcessor(ICrc32Service crcService, ILogger<RomProcessor> logger)
    {
        _crcService = crcService;
        _logger = logger;
    }

    public async Task<(int processed, int unique, int duplicates)> ProcessRomsAsync(
        string inputPath,
        string outputPath, 
        ProcessMode mode, 
        Dictionary<string, RomEntry> datRoms,
        IProgress<string>? progress = null)
    {
        var newPath = Path.Combine(outputPath, "new");
        var checkedPath = Path.Combine(outputPath, "checked");
        
        // Create directories
        Directory.CreateDirectory(newPath);
        Directory.CreateDirectory(checkedPath);

        if (!Directory.Exists(inputPath))
        {
            _logger.LogError("Directory not found: {Path}", inputPath);
            return (0, 0, 0);
        }

        var files = Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories)
            .Where(f => IsRomFile(f))
            .ToArray();

        int processed = 0, unique = 0, duplicates = 0;

        foreach (var file in files)
        {
            try
            {
                var fileName = Path.GetFileName(file);
                progress?.Report($"Processing: {fileName}");
                
                var crc = await CalculateFileCrcAsync(file);
                var result = ProcessSingleRom(file, crc, datRoms, newPath, checkedPath, mode);
                
                processed++;
                
                switch (result)
                {
                    case ProcessResult.Ok:
                    case ProcessResult.Copied:
                        unique++;
                        break;
                    case ProcessResult.AlreadyExists:
                        duplicates++;
                        break;
                }

                LogResult(fileName, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {File}", file);
            }
        }

        return (processed, unique, duplicates);
    }

    private async Task<string> CalculateFileCrcAsync(string filePath)
    {
        _crcService.Reset();
        
        if (Path.GetExtension(filePath).ToLower() == ".zip")
        {
            return await CalculateZipCrcAsync(filePath);
        }
        
        using var stream = File.OpenRead(filePath);
        _crcService.Append(stream);
        var hash = _crcService.GetHashAndReset();
        return BitConverter.ToString(hash).Replace("-", "");
    }

    private async Task<string> CalculateZipCrcAsync(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.Entries.FirstOrDefault(e => IsRomFile(e.Name));
        
        if (entry == null)
            return string.Empty;

        using var stream = entry.Open();
        await Task.Run(() => _crcService.Append(stream));
        var hash = _crcService.GetHashAndReset();
        return BitConverter.ToString(hash).Replace("-", "");
    }

    private ProcessResult ProcessSingleRom(
        string sourceFile, 
        string crc, 
        Dictionary<string, RomEntry> datRoms,
        string newPath, 
        string checkedPath, 
        ProcessMode mode)
    {
        var fileName = Path.GetFileName(sourceFile);
        string destPath;
        
        // Check if ROM is in DAT
        if (!datRoms.ContainsKey(crc))
        {
            // Move unknown ROMs to 'new' folder
            destPath = Path.Combine(newPath, fileName);
            
            // Avoid overwriting
            if (File.Exists(destPath))
            {
                destPath = GetUniqueFileName(destPath);
            }

            switch (mode)
            {
                case ProcessMode.Move:
                    File.Move(sourceFile, destPath);
                    return ProcessResult.NotInDat;
                    
                case ProcessMode.Backup:
                    File.Copy(sourceFile, destPath);
                    return ProcessResult.NotInDat;
                    
                case ProcessMode.Purge:
                    File.Delete(sourceFile);
                    return ProcessResult.Deleted;
                    
                default:
                    return ProcessResult.Error;
            }
        }

        // Check if already processed
        if (_processedCrcs.Contains(crc))
        {
            if (mode == ProcessMode.Purge)
            {
                File.Delete(sourceFile);
                return ProcessResult.Deleted;
            }
            return ProcessResult.AlreadyExists;
        }

        _processedCrcs.Add(crc);
        
        destPath = Path.Combine(checkedPath, fileName);
        
        // Avoid overwriting
        if (File.Exists(destPath))
        {
            destPath = GetUniqueFileName(destPath);
        }

        switch (mode)
        {
            case ProcessMode.Move:
                File.Move(sourceFile, destPath);
                return ProcessResult.Ok;
                
            case ProcessMode.Backup:
                File.Copy(sourceFile, destPath);
                return ProcessResult.Copied;
                
            case ProcessMode.Purge:
                File.Move(sourceFile, destPath);
                return ProcessResult.Ok;
                
            default:
                return ProcessResult.Error;
        }
    }

    private string GetUniqueFileName(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath)!;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        
        int counter = 1;
        string newPath;
        
        do
        {
            newPath = Path.Combine(directory, $"{nameWithoutExt}_{counter}{extension}");
            counter++;
        } 
        while (File.Exists(newPath));
        
        return newPath;
    }

    private bool IsRomFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        return ext is ".rom" or ".sms" or ".gg" or ".zip" or ".bin";
    }

    private void LogResult(string fileName, ProcessResult result)
    {
        var status = result switch
        {
            ProcessResult.Ok => "ok",
            ProcessResult.AlreadyExists => "já existe",
            ProcessResult.Copied => "copiado",
            ProcessResult.Deleted => "deletado",
            ProcessResult.NotInDat => "não está no DAT",
            _ => "erro"
        };
        
        _logger.LogInformation("{FileName} >> {Status}", fileName, status);
    }
}
