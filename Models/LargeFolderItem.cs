namespace DiskSweep.Models;

public class LargeFolderItem
{
    public string FolderPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public int FileCount { get; set; }
    public bool IsHighRisk { get; set; }

    public string TotalSizeFormatted => DriveItem.FormatBytes(TotalBytes);
    public string RiskBadge => IsHighRisk ? "PROTECTED APPLICATION" : "USER FOLDER";
    public string RiskBadgeColor => IsHighRisk ? "#E3B341" : "#3FB950";
}
