using System.IO;
using System.Text.Json;

namespace DiskSweep.Services;

public class AppSettings
{
    public string Theme { get; set; } = "Charcoal";
    public bool SafeMode24Hours { get; set; } = false;
    public bool MoveToRecycleBinInsteadOfPermanent { get; set; } = false;
    public bool AutoScanOnStartup { get; set; } = true;
    public int LargeFileSizeThresholdMB { get; set; } = 50;
    public bool ShowConfirmDialog { get; set; } = true;
}

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _currentSettings;

    public SettingsService()
    {
        string appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "DiskSweep");
        Directory.CreateDirectory(appData);
        _settingsPath = Path.Combine(appData, "settings.json");
        _currentSettings = LoadSettings();
    }

    public AppSettings Current => _currentSettings;

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                string json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    _currentSettings = loaded;
                    return _currentSettings;
                }
            }
        }
        catch { }

        _currentSettings = new AppSettings();
        return _currentSettings;
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            _currentSettings = settings;
            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch { }
    }
}
