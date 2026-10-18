using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiskSweep.Models;

public class LargeFolderItem : INotifyPropertyChanged
{
    private long _totalBytes;
    private int _fileCount;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string FolderPath { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public long TotalBytes
    {
        get => _totalBytes;
        set
        {
            if (_totalBytes != value)
            {
                _totalBytes = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalSizeFormatted));
            }
        }
    }

    public int FileCount
    {
        get => _fileCount;
        set
        {
            if (_fileCount != value)
            {
                _fileCount = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsHighRisk { get; set; }

    public string TotalSizeFormatted => DriveItem.FormatBytes(TotalBytes);
    public string RiskBadge => IsHighRisk ? "PROTECTED APPLICATION" : "USER FOLDER";
    public string RiskBadgeColor => IsHighRisk ? "#E3B341" : "#3FB950";

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
