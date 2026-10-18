using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiskSweep.Core.Models;

public class CleanItem : INotifyPropertyChanged
{
    private bool _isSelected = true;
    private long _size;
    private int _itemCount;
    private bool _isScanning;
    private string _status = "Pending";

    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SafetyLevel Safety { get; set; } = SafetyLevel.Safe;
    public bool IsRecycleBin { get; set; }
    public Func<CleanItem, CancellationToken, Task<int>>? CustomCleanAction { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public long Size
    {
        get => _size;
        set
        {
            if (_size != value)
            {
                _size = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SizeFormatted));
                OnPropertyChanged(nameof(StatusFormatted));
            }
        }
    }

    public int ItemCount
    {
        get => _itemCount;
        set
        {
            if (_itemCount != value)
            {
                _itemCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusFormatted));
            }
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            if (_isScanning != value)
            {
                _isScanning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusFormatted));
            }
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusFormatted));
            }
        }
    }

    public string SizeFormatted => DriveItem.FormatBytes(Size);

    public string StatusFormatted
    {
        get
        {
            if (IsScanning) return "Scanning...";
            if (Size > 0) return $"{SizeFormatted} ({ItemCount:N0} files)";
            return Status;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? SelectionChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
