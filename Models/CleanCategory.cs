using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DiskSweep.Models;

public class CleanCategory : INotifyPropertyChanged
{
    private bool? _isSelected = true;
    private bool _isUpdatingChildren;

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

                if (!_isUpdatingChildren && value.HasValue)
                {
                    _isUpdatingChildren = true;
                    foreach (var item in Items)
                    {
                        item.IsSelected = value.Value;
                    }
                    _isUpdatingChildren = false;
                }
            }
        }
    }

    public long TotalSize => Items.Sum(i => i.Size);
    public int TotalItemCount => Items.Sum(i => i.ItemCount);

    public string TotalSizeFormatted => DriveItem.FormatBytes(TotalSize);
    public string ItemCountFormatted => $"{TotalItemCount:N0} files";

    public void AddItem(CleanItem item)
    {
        item.CategoryName = Name;
        item.SelectionChanged += OnItemSelectionChanged;
        item.PropertyChanged += OnItemPropertyChanged;
        Items.Add(item);
        RefreshSummary();
    }

    private void OnItemSelectionChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingChildren) return;

        bool allChecked = Items.All(i => i.IsSelected);
        bool allUnchecked = Items.All(i => !i.IsSelected);

        _isUpdatingChildren = true;
        if (allChecked)
            IsSelected = true;
        else if (allUnchecked)
            IsSelected = false;
        else
            IsSelected = null;
        _isUpdatingChildren = false;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CleanItem.Size) or nameof(CleanItem.ItemCount))
        {
            RefreshSummary();
        }
    }

    public void RefreshSummary()
    {
        OnPropertyChanged(nameof(TotalSize));
        OnPropertyChanged(nameof(TotalSizeFormatted));
        OnPropertyChanged(nameof(TotalItemCount));
        OnPropertyChanged(nameof(ItemCountFormatted));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
