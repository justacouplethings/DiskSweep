using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DiskSweep.Models;

namespace DiskSweep.Converters;

public class BytesToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is long b) return DriveItem.FormatBytes(b);
        if (value is int i) return DriveItem.FormatBytes(i);
        return "0 B";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class FreePercentToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double pct)
        {
            if (pct < 10.0) return new SolidColorBrush(Color.FromRgb(229, 20, 0));       // Red
            if (pct < 20.0) return new SolidColorBrush(Color.FromRgb(240, 163, 10));     // Amber
            return new SolidColorBrush(Color.FromRgb(0, 163, 0));                        // Healthy Green
        }
        return new SolidColorBrush(Color.FromRgb(0, 122, 204));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class SafetyToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SafetyLevel level)
        {
            return level switch
            {
                SafetyLevel.Safe => new SolidColorBrush(Color.FromRgb(78, 201, 176)),       // Mint/Green
                SafetyLevel.Recommended => new SolidColorBrush(Color.FromRgb(220, 160, 60)), // Amber
                SafetyLevel.Caution => new SolidColorBrush(Color.FromRgb(244, 67, 54)),      // Red/Caution
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class SafetyToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SafetyLevel level)
        {
            return level switch
            {
                SafetyLevel.Safe => "SAFE",
                SafetyLevel.Recommended => "NORMAL",
                SafetyLevel.Caution => "CAUTION",
                _ => ""
            };
        }
        return "";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool b ? !b : false;
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

