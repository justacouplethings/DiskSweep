namespace DiskSweep.Models;

public class LargeFileItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string DriveLetter { get; set; } = string.Empty;

    public string SizeFormatted => DriveItem.FormatBytes(Size);
    public string LastModifiedFormatted => LastModified.ToString("yyyy-MM-dd HH:mm:ss");
}
