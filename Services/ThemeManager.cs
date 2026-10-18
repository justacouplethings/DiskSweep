using System.Windows;
using System.Windows.Media;

namespace DiskSweep.Services;

public static class ThemeManager
{
    public static readonly string[] AvailableThemes = ["Charcoal", "OLED", "MidnightNavy", "MatrixGreen"];

    public static void ApplyTheme(string themeName)
    {
        var res = Application.Current?.Resources;
        if (res == null) return;

        switch (themeName)
        {
            case "OLED":
                SetBrushes(res,
                    bg: "#000000",
                    surface: "#080808",
                    panel: "#101010",
                    header: "#181818",
                    border: "#262626",
                    accent: "#00FF9D",
                    accentHover: "#33FFB0",
                    textPrimary: "#FFFFFF",
                    textSecondary: "#888888");
                break;

            case "MidnightNavy":
                SetBrushes(res,
                    bg: "#0B0F17",
                    surface: "#111827",
                    panel: "#1F2937",
                    header: "#243042",
                    border: "#374151",
                    accent: "#3B82F6",
                    accentHover: "#60A5FA",
                    textPrimary: "#F3F4F6",
                    textSecondary: "#9CA3AF");
                break;

            case "MatrixGreen":
                SetBrushes(res,
                    bg: "#060A06",
                    surface: "#0D140D",
                    panel: "#141F14",
                    header: "#1A281A",
                    border: "#253B25",
                    accent: "#10B981",
                    accentHover: "#34D399",
                    textPrimary: "#E2F8E9",
                    textSecondary: "#7AA885");
                break;

            case "Charcoal":
            default:
                SetBrushes(res,
                    bg: "#181818",
                    surface: "#202020",
                    panel: "#282828",
                    header: "#2E2E2E",
                    border: "#383838",
                    accent: "#007ACC",
                    accentHover: "#1C97EA",
                    textPrimary: "#E6E6E6",
                    textSecondary: "#999999");
                break;
        }
    }

    private static void SetBrushes(
        ResourceDictionary res,
        string bg,
        string surface,
        string panel,
        string header,
        string border,
        string accent,
        string accentHover,
        string textPrimary,
        string textSecondary)
    {
        res["BgDarkBrush"] = BrushFromHex(bg);
        res["SurfaceDarkBrush"] = BrushFromHex(surface);
        res["PanelDarkBrush"] = BrushFromHex(panel);
        res["HeaderDarkBrush"] = BrushFromHex(header);
        res["BorderDarkBrush"] = BrushFromHex(border);
        res["AccentBlueBrush"] = BrushFromHex(accent);
        res["AccentHoverBrush"] = BrushFromHex(accentHover);
        res["TextPrimaryBrush"] = BrushFromHex(textPrimary);
        res["TextSecondaryBrush"] = BrushFromHex(textSecondary);
    }

    private static SolidColorBrush BrushFromHex(string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }
}
