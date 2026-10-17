using System.IO;
using DiskSweep.Models;

namespace DiskSweep.Services;

public class DriveService
{
    public List<DriveItem> GetDrives()
    {
        var list = new List<DriveItem>();
        try
        {
            var drives = DriveInfo.GetDrives();
            foreach (var d in drives)
            {
                if (!d.IsReady) continue;

                var item = new DriveItem
                {
                    Name = d.Name,
                    VolumeLabel = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "" : d.VolumeLabel,
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
