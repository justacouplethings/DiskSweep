using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DiskSweep.Core.Services;

public static class TrashService
{
    #region Win32 P/Invoke
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, uint dwFlags);

    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pFrom;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpszProgressTitle;
    }

    private const uint FO_DELETE = 0x0003;
    private const ushort FOF_ALLOWUNDO = 0x0040;
    private const ushort FOF_NOCONFIRMATION = 0x0010;
    private const ushort FOF_NOERRORUI = 0x0400;
    private const ushort FOF_SILENT = 0x0004;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);
    #endregion

    public static bool MoveToTrash(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || (!File.Exists(path) && !Directory.Exists(path)))
        {
            return false;
        }

        try
        {
            if (PlatformInfo.IsWindows)
            {
                var shf = new SHFILEOPSTRUCT
                {
                    wFunc = FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_SILENT
                };
                return SHFileOperation(ref shf) == 0;
            }
            else if (PlatformInfo.IsMacOS)
            {
                // Native macOS Trash in ~/.Trash
                string home = PlatformInfo.UserHomePath;
                string trashDir = Path.Combine(home, ".Trash");
                if (!Directory.Exists(trashDir))
                {
                    Directory.CreateDirectory(trashDir);
                }

                string name = Path.GetFileName(path);
                string dest = Path.Combine(trashDir, name);
                if (File.Exists(dest) || Directory.Exists(dest))
                {
                    dest = Path.Combine(trashDir, $"{Path.GetFileNameWithoutExtension(name)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(name)}");
                }

                if (File.Exists(path))
                {
                    File.Move(path, dest);
                    return true;
                }
                else if (Directory.Exists(path))
                {
                    Directory.Move(path, dest);
                    return true;
                }
            }
            else if (PlatformInfo.IsLinux)
            {
                // FreeDesktop XDG Trash Specification (~/.local/share/Trash)
                string home = PlatformInfo.UserHomePath;
                string xdgTrash = Path.Combine(home, ".local", "share", "Trash");
                string filesDir = Path.Combine(xdgTrash, "files");
                string infoDir = Path.Combine(xdgTrash, "info");

                Directory.CreateDirectory(filesDir);
                Directory.CreateDirectory(infoDir);

                string fileName = Path.GetFileName(path);
                string dest = Path.Combine(filesDir, fileName);
                if (File.Exists(dest) || Directory.Exists(dest))
                {
                    fileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(fileName)}";
                    dest = Path.Combine(filesDir, fileName);
                }

                // Write .trashinfo metadata
                string infoFile = Path.Combine(infoDir, $"{fileName}.trashinfo");
                string metadata = $"[Trash Info]\nPath={Uri.EscapeDataString(path)}\nDeletionDate={DateTime.Now:yyyy-MM-ddTHH:mm:ss}\n";
                File.WriteAllText(infoFile, metadata);

                if (File.Exists(path))
                {
                    File.Move(path, dest);
                    return true;
                }
                else if (Directory.Exists(path))
                {
                    Directory.Move(path, dest);
                    return true;
                }
            }
        }
        catch { }

        return false;
    }

    public static (long size, long count) GetTrashStats()
    {
        try
        {
            if (PlatformInfo.IsWindows)
            {
                var rbInfo = new SHQUERYRBINFO();
                rbInfo.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));
                int hr = SHQueryRecycleBin(null, ref rbInfo);
                if (hr == 0)
                {
                    return (rbInfo.i64Size, rbInfo.i64NumItems);
                }
            }
            else if (PlatformInfo.IsMacOS)
            {
                string trashDir = Path.Combine(PlatformInfo.UserHomePath, ".Trash");
                return GetDirectoryMetrics(trashDir);
            }
            else if (PlatformInfo.IsLinux)
            {
                string filesDir = Path.Combine(PlatformInfo.UserHomePath, ".local", "share", "Trash", "files");
                return GetDirectoryMetrics(filesDir);
            }
        }
        catch { }

        return (0, 0);
    }

    public static int EmptyTrash()
    {
        try
        {
            if (PlatformInfo.IsWindows)
            {
                int hr = SHEmptyRecycleBin(IntPtr.Zero, null, SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);
                return hr == 0 ? 1 : 0;
            }
            else if (PlatformInfo.IsMacOS)
            {
                string trashDir = Path.Combine(PlatformInfo.UserHomePath, ".Trash");
                return DeleteDirectoryContents(trashDir);
            }
            else if (PlatformInfo.IsLinux)
            {
                string filesDir = Path.Combine(PlatformInfo.UserHomePath, ".local", "share", "Trash", "files");
                string infoDir = Path.Combine(PlatformInfo.UserHomePath, ".local", "share", "Trash", "info");
                int c = DeleteDirectoryContents(filesDir);
                DeleteDirectoryContents(infoDir);
                return c;
            }
        }
        catch { }

        return 0;
    }

    private static (long size, long count) GetDirectoryMetrics(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return (0, 0);

        long totalSize = 0;
        long totalCount = 0;

        try
        {
            var di = new DirectoryInfo(dirPath);
            foreach (var fi in di.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try
                {
                    totalSize += fi.Length;
                    totalCount++;
                }
                catch { }
            }
        }
        catch { }

        return (totalSize, totalCount);
    }

    private static int DeleteDirectoryContents(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return 0;

        int deleted = 0;
        try
        {
            var di = new DirectoryInfo(dirPath);
            foreach (var fi in di.EnumerateFiles())
            {
                try
                {
                    fi.Delete();
                    deleted++;
                }
                catch { }
            }
            foreach (var sub in di.EnumerateDirectories())
            {
                try
                {
                    sub.Delete(true);
                    deleted++;
                }
                catch { }
            }
        }
        catch { }

        return deleted;
    }
}
