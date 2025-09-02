namespace XtraRCleaner.Models;

public class Settings
{
    public string DatFilePath { get; set; } = "games.dat";
    public string OldRomsPath { get; set; } = ".\\old";
    public string NewRomsPath { get; set; } = ".\\new";
    public string CheckedRomsPath { get; set; } = ".\\checked";
    public string LogFilePath { get; set; } = "CRCChecker_{0:yyyyMMdd_HHmmss}.log";
}

public class AppSettings
{
    public Settings Settings { get; set; } = new();
}

public class RomEntry
{
    public string Name { get; set; } = string.Empty;
    public string Crc { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
}

public enum ProcessMode
{
    Move,
    Backup,
    Purge
}

public enum ProcessResult
{
    Ok,
    AlreadyExists,
    Copied,
    Deleted,
    NotInDat,
    Error
}
