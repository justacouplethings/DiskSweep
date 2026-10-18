using System.IO;
using DiskSweep.Models;

namespace DiskSweep.Services;

public class LargeFileScannerService
{
    public record ScanProgress(string CurrentFolder, int FilesScanned, int MatchesFound);

    public record ScanResult(List<LargeFileItem> Files, List<LargeFolderItem> TopFolders);

    public async Task<ScanResult> ScanSpaceAsync(
        IEnumerable<string> rootPaths, 
        long minSizeBytes, 
        int maxResults, 
        IProgress<ScanProgress>? progress, 
        CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            var results = new List<LargeFileItem>();
            var folderMap = new Dictionary<string, (long bytes, int count)>(StringComparer.OrdinalIgnoreCase);

            int filesScanned = 0;

            foreach (var root in rootPaths)
            {
                if (ct.IsCancellationRequested) break;
                if (!Directory.Exists(root)) continue;

                var stack = new Stack<string>();
                stack.Push(root);

                while (stack.Count > 0)
                {
                    if (ct.IsCancellationRequested) break;

                    string currentDir = stack.Pop();
                    progress?.Report(new ScanProgress(currentDir, filesScanned, results.Count));

                    try
                    {
                        var dirInfo = new DirectoryInfo(currentDir);

                        foreach (var file in dirInfo.EnumerateFiles())
                        {
                            if (ct.IsCancellationRequested) break;
                            filesScanned++;

                            try
                            {
                                long len = file.Length;
                                if (len >= minSizeBytes)
                                {
                                    var (context, isRisk) = ClassifyPath(file.FullName);

                                    results.Add(new LargeFileItem
                                    {
                                        FilePath = file.FullName,
                                        FileName = file.Name,
                                        DirectoryPath = file.DirectoryName ?? "",
                                        Extension = file.Extension.ToLowerInvariant(),
                                        Size = len,
                                        LastModified = file.LastWriteTime,
                                        DriveLetter = Path.GetPathRoot(file.FullName) ?? "",
                                        ParentContext = context,
                                        IsHighRisk = isRisk
                                    });

                                    // Aggregate folder
                                    string groupingFolder = GetMeaningfulParentFolder(file.FullName);
                                    if (!folderMap.TryGetValue(groupingFolder, out var agg))
                                    {
                                        agg = (0, 0);
                                    }
                                    folderMap[groupingFolder] = (agg.bytes + len, agg.count + 1);
                                }
                            }
                            catch { }
                        }

                        foreach (var sub in dirInfo.EnumerateDirectories())
                        {
                            if (ct.IsCancellationRequested) break;
                            try
                            {
                                if ((sub.Attributes & FileAttributes.ReparsePoint) == 0 &&
                                    (sub.Attributes & FileAttributes.System) == 0)
                                {
                                    stack.Push(sub.FullName);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            var sortedFiles = results
                .OrderByDescending(f => f.Size)
                .Take(maxResults)
                .ToList();

            var topFolders = folderMap
                .Select(kv =>
                {
                    var (context, isRisk) = ClassifyPath(kv.Key);
                    return new LargeFolderItem
                    {
                        FolderPath = kv.Key,
                        FolderName = Path.GetFileName(kv.Key),
                        TotalBytes = kv.Value.bytes,
                        FileCount = kv.Value.count,
                        Category = context,
                        IsHighRisk = isRisk
                    };
                })
                .OrderByDescending(f => f.TotalBytes)
                .Take(50)
                .ToList();

            return new ScanResult(sortedFiles, topFolders);
        }, ct);
    }

    public static (string Context, bool IsHighRisk) ClassifyPath(string path)
    {
        string lower = path.ToLowerInvariant();

        // Game installations
        if (lower.Contains("epic games") || lower.Contains("fortnite") ||
            lower.Contains("steamapps") || lower.Contains("steamlibrary") ||
            lower.Contains("riot games") || lower.Contains("valorant") ||
            lower.Contains("ubisoft") || lower.Contains("battle.net") ||
            lower.Contains("electronic arts") || lower.Contains("origin games") ||
            lower.Contains("gog galaxy") || lower.Contains("genshin") ||
            lower.EndsWith(".ucas") || lower.EndsWith(".pak") || lower.EndsWith(".rpf"))
        {
            return ("🎮 Game Installation (Corruption Risk)", true);
        }

        // Windows System
        if (lower.Contains(@"\windows\") || lower.Contains(@"\system32\") ||
            lower.Contains(@"system volume information") || lower.Contains(@"$recycle.bin") ||
            lower.Contains(@"\programdata\microsoft\"))
        {
            return ("🛡️ Windows System Asset", true);
        }

        // Program Files Applications
        if (lower.Contains(@"\program files\") || lower.Contains(@"\program files (x86)\"))
        {
            return ("⚙️ Application Installation", true);
        }

        // Virtual disks & ISOs
        if (lower.EndsWith(".iso") || lower.EndsWith(".vmdk") || lower.EndsWith(".vdi") ||
            lower.EndsWith(".vhdx") || lower.Contains("virtualbox") || lower.Contains("vmware"))
        {
            return ("💻 Virtual Machine Disk / ISO", false);
        }

        // User Data
        return ("📁 User Data / Media (Safe to Review)", false);
    }

    private static string GetMeaningfulParentFolder(string filePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(dir)) return filePath;

            // Go up until reaching a main directory under Program Files, Steam, or user root
            var parent = Directory.GetParent(dir);
            if (parent != null && parent.Parent != null)
            {
                return parent.FullName;
            }
            return dir;
        }
        catch
        {
            return Path.GetDirectoryName(filePath) ?? filePath;
        }
    }
}
