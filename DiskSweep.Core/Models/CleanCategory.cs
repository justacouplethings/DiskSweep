using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiskSweep.Core.Models;

public class CleanCategory : INotifyPropertyChanged
{
    private bool? _isSelected = true;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public ObservableCollection<CleanItem> Items { get; } = new();

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();

                if (value.HasValue)
                {
                    foreach (var item in Items)
                    {
                        item.IsSelected = value.Value;
                    }
                }
            }
        }
    }

    public long TotalSize => Items.Sum(i => i.Size);
    public int TotalItemCount => Items.Sum(i => i.ItemCount);
    public string TotalSizeFormatted => DriveItem.FormatBytes(TotalSize);

    public void AddItem(CleanItem item)
    {
        item.CategoryName = Name;
        item.SelectionChanged += (_, _) => UpdateSelectionState();
        item.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(CleanItem.Size) or nameof(CleanItem.ItemCount))
            {
                OnPropertyChanged(nameof(TotalSize));
                OnPropertyChanged(nameof(TotalItemCount));
                OnPropertyChanged(nameof(TotalSizeFormatted));
            }
        };
        Items.Add(item);
        UpdateSelectionState();
    }

    public void UpdateSelectionState()
    {
        int selectedCount = Items.Count(i => i.IsSelected);
        if (selectedCount == 0)
        {
            _isSelected = false;
        }
        else if (selectedCount == Items.Count)
        {
            _isSelected = true;
        }
        else
        {
            _isSelected = null;
        }

        OnPropertyChanged(nameof(IsSelected));
        OnPropertyChanged(nameof(TotalSize));
        OnPropertyChanged(nameof(TotalItemCount));
        OnPropertyChanged(nameof(TotalSizeFormatted));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
