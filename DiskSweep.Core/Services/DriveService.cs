using System.IO;
using DiskSweep.Core.Models;

namespace DiskSweep.Core.Services;

public class DriveService
{
    private static readonly HashSet<string> IgnoredLinuxFs = new(StringComparer.OrdinalIgnoreCase)
    {
        "tmpfs", "devtmpfs", "squashfs", "overlay", "sysfs", "proc", "cgroup", "cgroup2",
        "pstore", "efivarfs", "bpf", "autofs", "tracefs", "securityfs", "hugetlbfs", "mqueue"
    };

    public List<DriveItem> GetDrives()
    {
        var list = new List<DriveItem>();
        try
        {
            var drives = DriveInfo.GetDrives();
            foreach (var d in drives)
            {
                if (!d.IsReady) continue;

                // Linux virtual filesystem filter
                if (PlatformInfo.IsLinux)
                {
                    if (IgnoredLinuxFs.Contains(d.DriveFormat)) continue;
                    if (d.TotalSize == 0) continue;
                    if (d.Name.StartsWith("/sys") || d.Name.StartsWith("/proc") || d.Name.StartsWith("/dev")) continue;
                }

                // macOS read-only APFS system volume snapshot filter
                if (PlatformInfo.IsMacOS)
                {
                    if (d.TotalSize == 0) continue;
                    // Avoid duplicating APFS synthesized containers
                    if (d.Name.StartsWith("/System/Volumes/Update") || d.Name.StartsWith("/System/Volumes/xarts")) continue;
                }

                string label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "" : d.VolumeLabel;

                // Provide sensible friendly volume label on Unix
                if (string.IsNullOrWhiteSpace(label))
                {
                    if (d.Name == "/" || d.Name == "\\")
                    {
                        label = PlatformInfo.IsMacOS ? "Macintosh HD" : "Root System";
                    }
                    else if (d.Name.StartsWith("/Volumes/"))
                    {
                        label = Path.GetFileName(d.Name.TrimEnd('/'));
                    }
                    else if (d.Name.StartsWith("/home"))
                    {
                        label = "Home Partition";
                    }
                }

                var item = new DriveItem
                {
                    Name = d.Name,
                    VolumeLabel = label,
                    DriveType = d.DriveType.ToString(),
                    FileSystem = d.DriveFormat,
                    TotalBytes = d.TotalSize,
                    FreeBytes = d.AvailableFreeSpace
                };
                list.Add(item);
            }
        }
        catch { }

        return list;
    }
}
