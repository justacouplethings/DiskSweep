namespace DiskSweep.Core.Models;

public class LargeFileItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string DriveLetter { get; set; } = string.Empty;

    public string ParentContext { get; set; } = "User File";
    public bool IsHighRisk { get; set; } = false;

    public string SizeFormatted => DriveItem.FormatBytes(Size);
    public string LastModifiedFormatted => LastModified.ToString("yyyy-MM-dd HH:mm:ss");

    public string RiskBadge => IsHighRisk ? "CORRUPTION RISK" : "REVIEW";
    public string RiskBadgeColor => IsHighRisk ? "#F85149" : "#3FB950";
}
