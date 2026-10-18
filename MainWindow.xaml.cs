using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using DiskSweep.Services;
using DiskSweep.ViewModels;

namespace DiskSweep;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        EnableDarkModeTitleBar();

        if (DataContext is MainViewModel vm && vm.AutoScanOnStartup)
        {
            _ = vm.ScanCleanableAsync();
        }
    }

    private void EnableDarkModeTitleBar()
    {
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                int darkMode = 1;
                NativeMethods.DwmSetWindowAttribute(
                    hwnd, 
                    NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE, 
                    ref darkMode, 
                    sizeof(int));
            }
        }
        catch { }
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;

        if (e.Key == Key.F5 && !vm.IsBusy)
        {
            _ = vm.ScanCleanableAsync();
            e.Handled = true;
        }
        else if (e.Key == Key.R && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            vm.RefreshDrives();
            e.Handled = true;
        }
        else if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            vm.SelectAllCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.OemComma && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            vm.SelectedTabIndex = 3;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && vm.IsBusy)
        {
            vm.CancelCurrentOperation();
            e.Handled = true;
        }
        else if (e.Key == Key.F1)
        {
            ShowAbout();
            e.Handled = true;
        }
    }

    private void MenuTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string theme && DataContext is MainViewModel vm)
        {
            vm.SelectedTheme = theme;
        }
    }

    private void ThemeRadio_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string theme && DataContext is MainViewModel vm)
        {
            vm.SelectedTheme = theme;
        }
    }

    private void MenuOpenDiskMgmt_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("diskmgmt.msc") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open Disk Management: {ex.Message}", "DiskSweep", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MenuOpenStorageSense_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:storagesense") { UseShellExecute = true });
        }
        catch { }
    }

    private void MenuOpenUserTemp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", Path.GetTempPath()) { UseShellExecute = true });
        }
        catch { }
    }

    private void MenuOpenWindowsTemp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
            Process.Start(new ProcessStartInfo("explorer.exe", winTemp) { UseShellExecute = true });
        }
        catch { }
    }

    private void MenuLaunchCleanmgr_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("cleanmgr.exe") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not launch cleanmgr: {ex.Message}", "DiskSweep", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        ShowAbout();
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "DiskSweep v1.1.0\nHigh-Performance Windows Storage Capacity & System Cleaner\n\nFeatures:\n• Non-colliding multi-volume storage & capacity monitor\n• Fast throttled cache, temp, and update cleanup\n• Stationary real-time freed storage feedback (zero text jitter)\n• 4 Visual Themes: Charcoal Dark, OLED Black, Midnight Navy, Matrix Green\n• 24-hour safety cushions & Recycle Bin fallback options\n• Deep space-hog file analyzer\n\nDesigned for speed, resilience, and clarity.",
            "About DiskSweep",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.LogEntries.Clear();
            vm.AddLog("Log cleared.");
        }
    }
}