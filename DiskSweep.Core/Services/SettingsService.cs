using System.IO;
using System.Text.Json;
using DiskSweep.Core.Models;

namespace DiskSweep.Core.Services;

public class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _settingsFilePath;

    public CleanerSettings Current { get; private set; }

    public SettingsService()
    {
        string baseDir;
        if (PlatformInfo.IsWindows)
        {
            baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiskSweep");
        }
        else
        {
            baseDir = Path.Combine(PlatformInfo.UserHomePath, ".config", "disksweep");
        }

        Directory.CreateDirectory(baseDir);
        _settingsFilePath = Path.Combine(baseDir, "settings.json");
        Current = LoadSettings();
    }

    public CleanerSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<CleanerSettings>(json);
                if (settings != null) return settings;
            }
        }
        catch { }

        return new CleanerSettings();
    }

    public void SaveSettings(CleanerSettings settings)
    {
        try
        {
            Current = settings;
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch { }
    }
}
