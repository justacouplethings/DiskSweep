using System.Runtime.InteropServices;

namespace DiskSweep.Core.Services;

public static class PlatformInfo
{
    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsMacOS => OperatingSystem.IsMacOS();
    public static bool IsLinux => OperatingSystem.IsLinux();

    public static string OSName
    {
        get
        {
            if (OperatingSystem.IsWindows()) return "Windows";
            if (OperatingSystem.IsMacOS()) return "macOS";
            if (OperatingSystem.IsLinux()) return "Linux";
            return RuntimeInformation.OSDescription;
        }
    }

    public static string UserHomePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static bool IsElevated
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    var principal = new System.Security.Principal.WindowsPrincipal(identity);
                    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // On macOS / Linux: check if effective UID is 0 (root)
                try
                {
                    return Environment.UserName.Equals("root", StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
