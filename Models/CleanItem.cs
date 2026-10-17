using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiskSweep.Models;

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
                OnPropertyChanged(nameof(CountFormatted));
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
            }
        }
    }

    public string SizeFormatted => DriveItem.FormatBytes(Size);
    public string CountFormatted => $"{ItemCount:N0} files";

    public event EventHandler? SelectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
