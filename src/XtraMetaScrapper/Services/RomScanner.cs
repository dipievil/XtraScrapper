using System.IO.Compression;
using XtraMetaScrapper.Models;

namespace XtraMetaScrapper.Services;

public interface IRomScanner
{
    Task<IEnumerable<RomFile>> ScanRomsAsync(string romsPath);
    string ExtractSystemName(string filePath, string baseRomsPath);
}

public class RomScanner : IRomScanner
{
    private readonly string[] _supportedExtensions = 
    {
        ".zip", ".rom", ".sms", ".gg", ".bin", ".md", ".gen", ".smd", 
        ".nes", ".fds", ".gb", ".gbc", ".gba", ".n64", ".z64", ".v64"
    };

    public async Task<IEnumerable<RomFile>> ScanRomsAsync(string romsPath)
    {
        var romFiles = new List<RomFile>();

        if (!Directory.Exists(romsPath))
        {
            return romFiles;
        }

        var allFiles = Directory.GetFiles(romsPath, "*.*", SearchOption.AllDirectories)
            .Where(file => _supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()));

        foreach (var filePath in allFiles)
        {
            var romFile = new RomFile
            {
                FilePath = filePath,
                FileName = Path.GetFileNameWithoutExtension(filePath),
                System = ExtractSystemName(filePath, romsPath),
                Size = new FileInfo(filePath).Length
            };

            // Calculate CRC32 for identification
            romFile.Crc32 = await CalculateCrc32Async(filePath);
            
            romFiles.Add(romFile);
        }

        return romFiles;
    }

    public string ExtractSystemName(string filePath, string baseRomsPath)
    {
        var relativePath = Path.GetRelativePath(baseRomsPath, filePath);
        var firstFolder = relativePath.Split(Path.DirectorySeparatorChar)[0];
        
        // If the file is directly in the roms folder, try to guess system from extension
        if (firstFolder == Path.GetFileName(filePath))
        {
            return GuessSystemFromExtension(Path.GetExtension(filePath));
        }

        return firstFolder;
    }

    private string GuessSystemFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".sms" => "MasterSystem",
            ".gg" => "GameGear",
            ".md" or ".gen" or ".smd" or ".bin" => "MegaDrive",
            ".nes" or ".fds" => "NES",
            ".gb" => "GameBoy",
            ".gbc" => "GameBoyColor",
            ".gba" => "GameBoyAdvance",
            ".n64" or ".z64" or ".v64" => "Nintendo64",
            _ => "Unknown"
        };
    }

    private async Task<string> CalculateCrc32Async(string filePath)
    {
        try
        {
            if (Path.GetExtension(filePath).ToLowerInvariant() == ".zip")
            {
                return await CalculateZipCrc32Async(filePath);
            }
            else
            {
                return await CalculateFileCrc32Async(filePath);
            }
        }
        catch
        {
            return "00000000";
        }
    }

    private async Task<string> CalculateZipCrc32Async(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.Entries.FirstOrDefault(e => 
            _supportedExtensions.Contains(Path.GetExtension(e.Name).ToLowerInvariant()));

        if (entry == null) return "00000000";

        using var stream = entry.Open();
        return await CalculateStreamCrc32Async(stream);
    }

    private async Task<string> CalculateFileCrc32Async(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return await CalculateStreamCrc32Async(stream);
    }

    private async Task<string> CalculateStreamCrc32Async(Stream stream)
    {
        uint crc = 0xFFFFFFFF;
        var buffer = new byte[8192];
        
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            for (int i = 0; i < bytesRead; i++)
            {
                crc = Crc32Table[(crc ^ buffer[i]) & 0xFF] ^ (crc >> 8);
            }
        }

        return (~crc).ToString("X8");
    }

    private static readonly uint[] Crc32Table = GenerateCrc32Table();

    private static uint[] GenerateCrc32Table()
    {
        var table = new uint[256];
        const uint polynomial = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ polynomial;
                else
                    crc >>= 1;
            }
            table[i] = crc;
        }

        return table;
    }
}
