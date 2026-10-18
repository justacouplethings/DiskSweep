namespace DiskSweep.Core.Models;

public class CleanerSettings
{
    public string Theme { get; set; } = "Charcoal";
    public bool SafeMode24Hours { get; set; } = true;
    public bool MoveToRecycleBinInsteadOfPermanent { get; set; } = false;
    public bool AutoScanOnStartup { get; set; } = true;
    public int LargeFileSizeThresholdMB { get; set; } = 50;
    public bool ShowConfirmDialog { get; set; } = true;
}
