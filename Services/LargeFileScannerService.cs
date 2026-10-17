using System.IO;
using DiskSweep.Models;

namespace DiskSweep.Services;

public class LargeFileScannerService
{
    public record ScanProgress(string CurrentFolder, int FilesScanned, int MatchesFound);

    public async Task<List<LargeFileItem>> ScanTopLargeFilesAsync(
        string rootPath, 
        long minSizeBytes, 
        int maxResults, 
        IProgress<ScanProgress>? progress, 
        CancellationToken ct)
    {
        return await Task.Run(() =>
        {
            var results = new List<LargeFileItem>();
            var stack = new Stack<string>();
            stack.Push(rootPath);

            int filesScanned = 0;

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
                            if (file.Length >= minSizeBytes)
                            {
                                results.Add(new LargeFileItem
                                {
                                    FilePath = file.FullName,
                                    FileName = file.Name,
                                    DirectoryPath = file.DirectoryName ?? "",
                                    Extension = file.Extension.ToLowerInvariant(),
                                    Size = file.Length,
                                    LastModified = file.LastWriteTime,
                                    DriveLetter = Path.GetPathRoot(file.FullName) ?? ""
                                });
                            }
                        }
                        catch { }
                    }

                    foreach (var sub in dirInfo.EnumerateDirectories())
                    {
                        if (ct.IsCancellationRequested) break;
                        try
                        {
                            // Skip reparse points / junctions
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

            return results
                .OrderByDescending(f => f.Size)
                .Take(maxResults)
                .ToList();
        }, ct);
    }
}
