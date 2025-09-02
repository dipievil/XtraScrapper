using XtraRCleaner.Models;

namespace XtraRCleaner.Services;

public interface IDatParser
{
    Task<Dictionary<string, RomEntry>> ParseDatFileAsync(string filePath);
}

public class DatParser : IDatParser
{
    public async Task<Dictionary<string, RomEntry>> ParseDatFileAsync(string filePath)
    {
        var roms = new Dictionary<string, RomEntry>(StringComparer.OrdinalIgnoreCase);
        
        if (!File.Exists(filePath))
            return roms;

        var lines = await File.ReadAllLinesAsync(filePath);
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("clrmamepro") || line.StartsWith("game"))
                continue;

            // Parse ROM entries like: rom ( name "game.rom" size 12345 crc 12345678 )
            if (line.Trim().StartsWith("rom") && line.Contains("crc"))
            {
                var romEntry = ParseRomLine(line);
                if (romEntry != null && !string.IsNullOrEmpty(romEntry.Crc))
                {
                    roms[romEntry.Crc.ToUpper()] = romEntry;
                }
            }
        }

        return roms;
    }

    private RomEntry? ParseRomLine(string line)
    {
        try
        {
            var entry = new RomEntry();
            
            // Extract name
            var nameMatch = System.Text.RegularExpressions.Regex.Match(line, @"name\s+""([^""]+)""");
            if (nameMatch.Success)
                entry.Name = nameMatch.Groups[1].Value;

            // Extract CRC
            var crcMatch = System.Text.RegularExpressions.Regex.Match(line, @"crc\s+([a-fA-F0-9]+)");
            if (crcMatch.Success)
                entry.Crc = crcMatch.Groups[1].Value.ToUpper();

            // Extract size
            var sizeMatch = System.Text.RegularExpressions.Regex.Match(line, @"size\s+(\d+)");
            if (sizeMatch.Success)
                entry.Size = sizeMatch.Groups[1].Value;

            return string.IsNullOrEmpty(entry.Crc) ? null : entry;
        }
        catch
        {
            return null;
        }
    }
}
