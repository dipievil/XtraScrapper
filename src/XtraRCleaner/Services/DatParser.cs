using XtraRCleaner.Models;
using System.Xml.Linq;

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

        var content = await File.ReadAllTextAsync(filePath);
        
        // Check if it's XML format
        if (content.TrimStart().StartsWith("<?xml"))
        {
            return await ParseXmlDatAsync(filePath);
        }
        
        // Parse ClrMamePro format
        return await ParseClrMameProDatAsync(filePath);
    }

    private async Task<Dictionary<string, RomEntry>> ParseXmlDatAsync(string filePath)
    {
        var roms = new Dictionary<string, RomEntry>(StringComparer.OrdinalIgnoreCase);
        
        try
        {
            var doc = await Task.Run(() => XDocument.Load(filePath));
            
            var games = doc.Descendants("game");
            foreach (var game in games)
            {
                var romElement = game.Element("rom");
                if (romElement != null)
                {
                    var crc = romElement.Attribute("crc")?.Value;
                    var name = romElement.Attribute("name")?.Value ?? game.Attribute("name")?.Value;
                    var size = romElement.Attribute("size")?.Value;
                    
                    if (!string.IsNullOrEmpty(crc))
                    {
                        var entry = new RomEntry
                        {
                            Name = name ?? "",
                            Crc = crc.ToUpper(),
                            Size = size ?? ""
                        };
                        
                        roms[entry.Crc] = entry;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao parsear XML DAT: {ex.Message}");
        }
        
        return roms;
    }

    private async Task<Dictionary<string, RomEntry>> ParseClrMameProDatAsync(string filePath)
    {
        var roms = new Dictionary<string, RomEntry>(StringComparer.OrdinalIgnoreCase);
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
