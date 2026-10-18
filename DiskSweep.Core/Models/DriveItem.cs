namespace DiskSweep.Core.Models;

public class DriveItem
{
    public string Name { get; set; } = string.Empty;
    public string VolumeLabel { get; set; } = string.Empty;
    public string DriveType { get; set; } = string.Empty;
    public string FileSystem { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public long FreeBytes { get; set; }
    public long UsedBytes => TotalBytes - FreeBytes;

    public double FreePercentage => TotalBytes > 0 ? (double)FreeBytes / TotalBytes * 100.0 : 0.0;
    public double UsedPercentage => TotalBytes > 0 ? (double)UsedBytes / TotalBytes * 100.0 : 0.0;

    public string TotalDisplay => FormatBytes(TotalBytes);
    public string FreeDisplay => FormatBytes(FreeBytes);
    public string UsedDisplay => FormatBytes(UsedBytes);

    public string DisplayTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(VolumeLabel))
            {
                return $"{VolumeLabel} ({Name.TrimEnd('\\', '/')})";
            }

            if (Name == "/" || Name == "\\")
            {
                return "Root Filesystem (/)";
            }

            if (Name.StartsWith("/Volumes/", StringComparison.OrdinalIgnoreCase))
            {
                string volName = System.IO.Path.GetFileName(Name.TrimEnd('/'));
                return $"{volName} (External)";
            }

            return $"Storage Volume ({Name.TrimEnd('\\', '/')})";
        }
    }

    public string StatusText => $"{FreeDisplay} free of {TotalDisplay} ({FreePercentage:F1}% free)";

    public static string FormatBytes(long bytes)
    {
        if (bytes < 0) bytes = 0;
        string[] units = ["B", "KB", "MB", "GB", "TB", "PB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < units.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {units[order]}";
    }
}
