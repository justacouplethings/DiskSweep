using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using DiskSweep.Models;
using DiskSweep.Services;

namespace DiskSweep.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly DriveService _driveService = new();
    private readonly CleanerService _cleanerService = new();
    private readonly LargeFileScannerService _largeFileScanner = new();
    private readonly SettingsService _settingsService = new();

    private CancellationTokenSource? _cts;
    private bool _isScanning;
    private bool _isCleaning;
    private string _statusMessage = "Ready. Click 'Scan System' to inspect storage.";
    private string _liveActionText = "Ready";
    private string _liveFreedText = "0 B Freed";
    private string _liveCountText = "0 Files";
    private string _liveFileName = "";
    private double _progressValue;
    private bool _progressIsIndeterminate;
    private DriveItem? _selectedDrive;
    private CleanCategory? _selectedCategory;
    private CleanItem? _selectedCleanItem;
    private LargeFileItem? _selectedLargeFile;
    private int _selectedTabIndex = 0;

    public ObservableCollection<DriveItem> Drives { get; } = new();
    public ObservableCollection<CleanCategory> Categories { get; } = new();
    public ObservableCollection<LargeFileItem> LargeFiles { get; } = new();
    public ObservableCollection<string> LogEntries { get; } = new();

    public ICommand ScanCleanableCommand { get; }
    public ICommand CleanSelectedCommand { get; }
    public ICommand RefreshDrivesCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand DeselectAllCommand { get; }
    public ICommand SelectRecommendedCommand { get; }
    public ICommand CancelOperationCommand { get; }
    public ICommand ScanLargeFilesCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand DeleteLargeFileCommand { get; }
    public ICommand OpenDriveInExplorerCommand { get; }
    public ICommand SelectCleanItemCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand SaveSettingsCommand { get; }

    public MainViewModel()
    {
        ScanCleanableCommand = new RelayCommand(async () => await ScanCleanableAsync(), () => !IsBusy);
        CleanSelectedCommand = new RelayCommand(async () => await CleanSelectedAsync(), () => !IsBusy && TotalSelectedBytes > 0);
        RefreshDrivesCommand = new RelayCommand(() => RefreshDrives(), () => !IsBusy);
        SelectAllCommand = new RelayCommand(() => SetSelection(true));
        DeselectAllCommand = new RelayCommand(() => SetSelection(false));
        SelectRecommendedCommand = new RelayCommand(() => SetRecommendedSelection());
        CancelOperationCommand = new RelayCommand(() => CancelCurrentOperation(), () => IsBusy);
        ScanLargeFilesCommand = new RelayCommand(async () => await ScanLargeFilesAsync(), () => !IsBusy);
        OpenFolderCommand = new RelayCommand(param => OpenFolderInExplorer(param));
        DeleteLargeFileCommand = new RelayCommand(param => DeleteLargeFile(param as LargeFileItem));
        OpenDriveInExplorerCommand = new RelayCommand(param => OpenDrive(param as DriveItem ?? SelectedDrive));
        SelectCleanItemCommand = new RelayCommand(param =>
        {
            if (param is CleanItem ci)
            {
                SelectedCleanItem = ci;
            }
        });
        OpenSettingsCommand = new RelayCommand(() => SelectedTabIndex = 3);
        SaveSettingsCommand = new RelayCommand(() => SaveSettings());

        InitializeData();
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (_selectedTabIndex != value)
            {
                _selectedTabIndex = value;
                OnPropertyChanged();
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
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    public bool IsCleaning
    {
        get => _isCleaning;
        set
        {
            if (_isCleaning != value)
            {
                _isCleaning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }

    public bool IsBusy => IsScanning || IsCleaning;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string LiveActionText
    {
        get => _liveActionText;
        set
        {
            if (_liveActionText != value)
            {
                _liveActionText = value;
                OnPropertyChanged();
            }
        }
    }

    public string LiveFreedText
    {
        get => _liveFreedText;
        set
        {
            if (_liveFreedText != value)
            {
                _liveFreedText = value;
                OnPropertyChanged();
            }
        }
    }

    public string LiveCountText
    {
        get => _liveCountText;
        set
        {
            if (_liveCountText != value)
            {
                _liveCountText = value;
                OnPropertyChanged();
            }
        }
    }

    public string LiveFileName
    {
        get => _liveFileName;
        set
        {
            if (_liveFileName != value)
            {
                _liveFileName = value;
                OnPropertyChanged();
            }
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            if (Math.Abs(_progressValue - value) > 0.001)
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }
    }

    public bool ProgressIsIndeterminate
    {
        get => _progressIsIndeterminate;
        set
        {
            if (_progressIsIndeterminate != value)
            {
                _progressIsIndeterminate = value;
                OnPropertyChanged();
            }
        }
    }

    public DriveItem? SelectedDrive
    {
        get => _selectedDrive;
        set
        {
            if (_selectedDrive != value)
            {
                _selectedDrive = value;
                OnPropertyChanged();
            }
        }
    }

    public CleanCategory? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }
    }

    public CleanItem? SelectedCleanItem
    {
        get => _selectedCleanItem;
        set
        {
            if (_selectedCleanItem != value)
            {
                _selectedCleanItem = value;
                OnPropertyChanged();
            }
        }
    }

    public LargeFileItem? SelectedLargeFile
    {
        get => _selectedLargeFile;
        set
        {
            if (_selectedLargeFile != value)
            {
                _selectedLargeFile = value;
                OnPropertyChanged();
            }
        }
    }

    public long TotalSelectedBytes => Categories
        .SelectMany(c => c.Items)
        .Where(i => i.IsSelected)
        .Sum(i => i.Size);

    public int TotalSelectedFiles => Categories
        .SelectMany(c => c.Items)
        .Where(i => i.IsSelected)
        .Sum(i => i.ItemCount);

    public string TotalSelectedFormatted => DriveItem.FormatBytes(TotalSelectedBytes);
    public string TotalSelectedSummary => $"{TotalSelectedFormatted} ({TotalSelectedFiles:N0} files selected)";

    public int TotalDrivesCount => Drives.Count;
    public string TotalDriveCapacity => DriveItem.FormatBytes(Drives.Sum(d => d.TotalBytes));
    public string TotalDriveFree => DriveItem.FormatBytes(Drives.Sum(d => d.FreeBytes));

    // Settings properties
    public string[] AvailableThemes => ThemeManager.AvailableThemes;

    public string SelectedTheme
    {
        get => _settingsService.Current.Theme;
        set
        {
            if (_settingsService.Current.Theme != value)
            {
                _settingsService.Current.Theme = value;
                _settingsService.SaveSettings(_settingsService.Current);
                ThemeManager.ApplyTheme(value);
                OnPropertyChanged();
                AddLog($"Theme switched to: {value}");
            }
        }
    }

    public bool SafeMode24Hours
    {
        get => _settingsService.Current.SafeMode24Hours;
        set
        {
            if (_settingsService.Current.SafeMode24Hours != value)
            {
                _settingsService.Current.SafeMode24Hours = value;
                _settingsService.SaveSettings(_settingsService.Current);
                OnPropertyChanged();
            }
        }
    }

    public bool MoveToRecycleBinInsteadOfPermanent
    {
        get => _settingsService.Current.MoveToRecycleBinInsteadOfPermanent;
        set
        {
            if (_settingsService.Current.MoveToRecycleBinInsteadOfPermanent != value)
            {
                _settingsService.Current.MoveToRecycleBinInsteadOfPermanent = value;
                _settingsService.SaveSettings(_settingsService.Current);
                OnPropertyChanged();
            }
        }
    }

    public bool AutoScanOnStartup
    {
        get => _settingsService.Current.AutoScanOnStartup;
        set
        {
            if (_settingsService.Current.AutoScanOnStartup != value)
            {
                _settingsService.Current.AutoScanOnStartup = value;
                _settingsService.SaveSettings(_settingsService.Current);
                OnPropertyChanged();
            }
        }
    }

    public int LargeFileSizeThresholdMB
    {
        get => _settingsService.Current.LargeFileSizeThresholdMB;
        set
        {
            if (_settingsService.Current.LargeFileSizeThresholdMB != value)
            {
                _settingsService.Current.LargeFileSizeThresholdMB = value;
                _settingsService.SaveSettings(_settingsService.Current);
                OnPropertyChanged();
            }
        }
    }

    public bool ShowConfirmDialog
    {
        get => _settingsService.Current.ShowConfirmDialog;
        set
        {
            if (_settingsService.Current.ShowConfirmDialog != value)
            {
                _settingsService.Current.ShowConfirmDialog = value;
                _settingsService.SaveSettings(_settingsService.Current);
                OnPropertyChanged();
            }
        }
    }

    private void InitializeData()
    {
        ThemeManager.ApplyTheme(_settingsService.Current.Theme);
        RefreshDrives();

        var categories = _cleanerService.InitializeCategories();
        foreach (var cat in categories)
        {
            foreach (var item in cat.Items)
            {
                item.SelectionChanged += (_, _) => UpdateSelectionTotals();
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(CleanItem.Size))
                    {
                        UpdateSelectionTotals();
                    }
                };
            }
            Categories.Add(cat);
        }

        AddLog($"DiskSweep initialized. Active theme: {SelectedTheme}. Monitored drives: {Drives.Count}.");
    }

    public void SaveSettings()
    {
        _settingsService.SaveSettings(_settingsService.Current);
        MessageBox.Show("Settings saved and applied successfully.", "DiskSweep Settings", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void RefreshDrives()
    {
        Drives.Clear();
        var driveList = _driveService.GetDrives();
        foreach (var d in driveList)
        {
            Drives.Add(d);
        }

        if (SelectedDrive == null && Drives.Count > 0)
        {
            SelectedDrive = Drives[0];
        }

        OnPropertyChanged(nameof(TotalDrivesCount));
        OnPropertyChanged(nameof(TotalDriveCapacity));
        OnPropertyChanged(nameof(TotalDriveFree));
    }

    public async Task ScanCleanableAsync()
    {
        if (IsBusy) return;

        IsScanning = true;
        ProgressIsIndeterminate = true;
        LiveActionText = "Analyzing cleanable storage...";
        LiveFreedText = "Scanning...";
        LiveCountText = "";
        LiveFileName = "";
        StatusMessage = "Analyzing cleanable cache and temporary targets...";
        AddLog("Starting storage sweep analysis...");

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            int totalItems = Categories.Sum(c => c.Items.Count);
            int scanned = 0;

            foreach (var cat in Categories)
            {
                if (ct.IsCancellationRequested) break;
                foreach (var item in cat.Items)
                {
                    if (ct.IsCancellationRequested) break;
                    LiveActionText = $"Inspecting: {item.Name}";
                    StatusMessage = $"Scanning {item.Name}...";
                    await _cleanerService.ScanItemAsync(item, ct);
                    scanned++;
                    ProgressValue = (double)scanned / totalItems * 100.0;
                }
                cat.RefreshSummary();
            }

            UpdateSelectionTotals();
            RefreshDrives();
            AddLog($"Analysis complete. Found {TotalSelectedFormatted} cleanable in {TotalSelectedFiles:N0} files.");
            LiveActionText = "Scan Complete";
            LiveFreedText = TotalSelectedFormatted;
            LiveCountText = $"{TotalSelectedFiles:N0} Files";
            LiveFileName = "";
            StatusMessage = $"Analysis complete. {TotalSelectedFormatted} cleanable.";
        }
        catch (OperationCanceledException)
        {
            AddLog("Scan cancelled by user.");
            LiveActionText = "Cancelled";
            StatusMessage = "Scan cancelled.";
        }
        catch (Exception ex)
        {
            AddLog($"Scan error: {ex.Message}");
            LiveActionText = "Error";
            StatusMessage = "Scan failed.";
        }
        finally
        {
            ProgressIsIndeterminate = false;
            ProgressValue = 0;
            IsScanning = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public async Task CleanSelectedAsync()
    {
        if (IsBusy) return;

        var selectedItems = Categories
            .SelectMany(c => c.Items)
            .Where(i => i.IsSelected && (i.Size > 0 || i.IsRecycleBin))
            .ToList();

        if (selectedItems.Count == 0)
        {
            MessageBox.Show("No items are selected or all selected items are already empty.", "DiskSweep", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        long bytesToFree = selectedItems.Sum(i => i.Size);

        if (ShowConfirmDialog)
        {
            var result = MessageBox.Show(
                $"Clean the {selectedItems.Count} selected categories?\n\nEstimated space to reclaim: {DriveItem.FormatBytes(bytesToFree)}\n\nLocked or active files will be safely skipped.",
                "Confirm Disk Cleanup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;
        }

        IsCleaning = true;
        ProgressIsIndeterminate = false;
        ProgressValue = 0;
        LiveActionText = "Purging cleanable files...";
        LiveFreedText = "0 B Freed";
        LiveCountText = "0 Files";
        LiveFileName = "";
        StatusMessage = "Purging cleanable files...";
        AddLog($"Commencing deletion across {selectedItems.Count} targets...");

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        int totalDeleted = 0;
        long totalFreed = 0;
        int totalSkipped = 0;

        try
        {
            var progress = new Progress<CleanerService.CleanProgress>(p =>
            {
                long currentFreed = totalFreed + p.BytesFreed;
                int currentDel = totalDeleted + p.FilesDeleted;
                LiveFreedText = $"{DriveItem.FormatBytes(currentFreed)} Freed";
                LiveCountText = $"{currentDel:N0} Deleted";
                LiveFileName = p.CurrentItem;
            });

            for (int i = 0; i < selectedItems.Count; i++)
            {
                if (ct.IsCancellationRequested) break;
                var item = selectedItems[i];
                LiveActionText = $"Cleaning {item.Name}...";
                StatusMessage = $"Cleaning {item.Name}...";

                var (del, freed, skip) = await _cleanerService.CleanItemAsync(item, _settingsService.Current, progress, ct);
                totalDeleted += del;
                totalFreed += freed;
                totalSkipped += skip;

                LiveFreedText = $"{DriveItem.FormatBytes(totalFreed)} Freed";
                LiveCountText = $"{totalDeleted:N0} Deleted";

                AddLog($"Cleaned {item.Name}: {del:N0} files removed, {DriveItem.FormatBytes(freed)} freed, {skip:N0} locked/skipped.");
                ProgressValue = (double)(i + 1) / selectedItems.Count * 100.0;
            }

            foreach (var cat in Categories)
            {
                cat.RefreshSummary();
            }

            UpdateSelectionTotals();
            RefreshDrives();

            string summaryMsg = $"Cleanup finished! Freed {DriveItem.FormatBytes(totalFreed)} ({totalDeleted:N0} files removed, {totalSkipped:N0} safely skipped).";
            AddLog(summaryMsg);
            LiveActionText = "Cleanup Complete";
            LiveFreedText = $"{DriveItem.FormatBytes(totalFreed)} Freed";
            LiveCountText = $"{totalDeleted:N0} Deleted";
            LiveFileName = "";
            StatusMessage = summaryMsg;

            MessageBox.Show(summaryMsg, "DiskSweep Cleanup Finished", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            AddLog("Cleanup cancelled by user.");
            LiveActionText = "Cancelled";
            StatusMessage = "Cleanup cancelled.";
        }
        catch (Exception ex)
        {
            AddLog($"Cleanup error: {ex.Message}");
            LiveActionText = "Error";
            StatusMessage = "Cleanup failed.";
        }
        finally
        {
            ProgressIsIndeterminate = false;
            ProgressValue = 0;
            IsCleaning = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public async Task ScanLargeFilesAsync()
    {
        if (IsBusy) return;

        string targetRoot = SelectedDrive?.Name ?? "C:\\";

        IsScanning = true;
        ProgressIsIndeterminate = true;
        LiveActionText = $"Scanning {targetRoot} for large files...";
        StatusMessage = $"Scanning {targetRoot} for top large files...";
        AddLog($"Starting deep large-file scan on {targetRoot} (Threshold: >{LargeFileSizeThresholdMB} MB)...");

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        try
        {
            LargeFiles.Clear();
            var progress = new Progress<LargeFileScannerService.ScanProgress>(p =>
            {
                LiveActionText = $"Analyzing: {Path.GetFileName(p.CurrentFolder)}";
                LiveCountText = $"{p.FilesScanned:N0} files";
                LiveFreedText = $"{p.MatchesFound} found";
                StatusMessage = $"Analyzing {p.CurrentFolder} ({p.FilesScanned:N0} scanned, {p.MatchesFound} found)...";
            });

            long minSize = (long)LargeFileSizeThresholdMB * 1024 * 1024;
            var results = await _largeFileScanner.ScanTopLargeFilesAsync(targetRoot, minSize, 100, progress, ct);

            foreach (var f in results)
            {
                LargeFiles.Add(f);
            }

            AddLog($"Large files scan completed. Found {LargeFiles.Count} files >= {LargeFileSizeThresholdMB} MB on {targetRoot}.");
            LiveActionText = "Scan Complete";
            LiveCountText = $"{LargeFiles.Count} files";
            LiveFreedText = "";
            StatusMessage = $"Large files scan completed ({LargeFiles.Count} files listed).";
        }
        catch (OperationCanceledException)
        {
            AddLog("Large file scan cancelled.");
            LiveActionText = "Cancelled";
            StatusMessage = "Scan cancelled.";
        }
        catch (Exception ex)
        {
            AddLog($"Large file scan error: {ex.Message}");
            LiveActionText = "Error";
            StatusMessage = "Scan error.";
        }
        finally
        {
            ProgressIsIndeterminate = false;
            IsScanning = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public void CancelCurrentOperation()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            LiveActionText = "Cancelling...";
            StatusMessage = "Cancelling operation...";
            AddLog("Cancellation requested.");
        }
    }

    private void SetSelection(bool isSelected)
    {
        foreach (var cat in Categories)
        {
            cat.IsSelected = isSelected;
        }
        UpdateSelectionTotals();
    }

    private void SetRecommendedSelection()
    {
        foreach (var cat in Categories)
        {
            foreach (var item in cat.Items)
            {
                item.IsSelected = item.Safety == SafetyLevel.Safe || item.Safety == SafetyLevel.Recommended;
                if (item.IsRecycleBin)
                {
                    item.IsSelected = false;
                }
            }
        }
        UpdateSelectionTotals();
    }

    private void UpdateSelectionTotals()
    {
        OnPropertyChanged(nameof(TotalSelectedBytes));
        OnPropertyChanged(nameof(TotalSelectedFiles));
        OnPropertyChanged(nameof(TotalSelectedFormatted));
        OnPropertyChanged(nameof(TotalSelectedSummary));
    }

    private void OpenFolderInExplorer(object? param)
    {
        string? path = null;
        if (param is CleanItem ci) path = ci.TargetPath;
        else if (param is LargeFileItem lfi) path = lfi.DirectoryPath;
        else if (param is string p) path = p;

        if (string.IsNullOrEmpty(path)) return;

        try
        {
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"\"{path}\"") { UseShellExecute = true });
            }
            else if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{path}\"") { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open folder: {ex.Message}", "DiskSweep", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void DeleteLargeFile(LargeFileItem? item)
    {
        if (item == null) return;

        var res = MessageBox.Show(
            $"Move file to Recycle Bin?\n\n{item.FileName}\nSize: {item.SizeFormatted}\nPath: {item.FilePath}",
            "Delete File",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (res == MessageBoxResult.Yes)
        {
            bool ok = NativeMethods.MoveToRecycleBin(item.FilePath);
            if (ok || !File.Exists(item.FilePath))
            {
                LargeFiles.Remove(item);
                AddLog($"Deleted file: {item.FilePath} ({item.SizeFormatted})");
                RefreshDrives();
            }
            else
            {
                MessageBox.Show("Could not delete file. It may be locked by another process.", "DiskSweep", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OpenDrive(DriveItem? drive)
    {
        if (drive == null) return;
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", drive.Name) { UseShellExecute = true });
        }
        catch { }
    }

    public void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogEntries.Insert(0, $"[{timestamp}] {message}");
        while (LogEntries.Count > 500)
        {
            LogEntries.RemoveAt(LogEntries.Count - 1);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
