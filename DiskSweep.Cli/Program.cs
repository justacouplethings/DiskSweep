using DiskSweep.Core.Models;
using DiskSweep.Core.Services;
using Spectre.Console;

namespace DiskSweep.Cli;

public static class Program
{
    private static readonly DriveService _driveService = new();
    private static readonly CleanerService _cleanerService = new();
    private static readonly LargeFileScannerService _scannerService = new();
    private static readonly SettingsService _settingsService = new();

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Length == 0)
        {
            await RunInteractiveDashboardAsync();
            return 0;
        }

        string cmd = args[0].ToLowerInvariant();
        switch (cmd)
        {
            case "drives":
            case "drive":
            case "df":
                ShowDrivesTable();
                return 0;

            case "clean":
            case "sweep":
                bool all = args.Contains("--all") || args.Contains("-a");
                bool dryRun = args.Contains("--dry-run");
                bool trash = args.Contains("--trash");
                await RunCleanCliAsync(all, dryRun, trash);
                return 0;

            case "analyze":
            case "scan":
                string? path = args.FirstOrDefault(a => !a.StartsWith("-") && a != cmd);
                int minSize = 50;
                var sizeArg = args.FirstOrDefault(a => a.StartsWith("--min-size="));
                if (sizeArg != null && int.TryParse(sizeArg.Split('=')[1], out int s)) minSize = s;
                await RunAnalyzeCliAsync(path, minSize);
                return 0;

            case "--version":
            case "-v":
                AnsiConsole.MarkupLine("[bold dodgerblue1]DiskSweep[/] version [green]1.2.0[/] (Cross-Platform)");
                AnsiConsole.MarkupLine($"Operating System: [yellow]{PlatformInfo.OSName}[/] | Elevated: {(PlatformInfo.IsElevated ? "[green]YES (Root/Admin)[/]" : "[grey]NO (Standard User)[/]")}");
                return 0;

            case "--help":
            case "-h":
            case "help":
            default:
                ShowHelp();
                return 0;
        }
    }

    private static void ShowBanner()
    {
        AnsiConsole.Write(
            new FigletText("DiskSweep")
                .Color(Color.DeepSkyBlue1));

        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        grid.AddRow(
            $"[grey]Platform:[/] [yellow]{PlatformInfo.OSName}[/]",
            $"[grey]Privileges:[/] {(PlatformInfo.IsElevated ? "[green]Elevated (Administrator/Root)[/]" : "[yellow]Standard User[/]")}");
        grid.AddRow(
            "[grey]Engine:[/] [dodgerblue1]DiskSweep Core v1.2.0[/]",
            "[grey]Architecture:[/] [cyan]Windows | macOS | Linux[/]");

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold deepskyblue1]System Storage & Maintenance Console[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.DeepSkyBlue1));
        AnsiConsole.WriteLine();
    }

    private static void ShowHelp()
    {
        ShowBanner();
        AnsiConsole.MarkupLine("[bold underline]Usage:[/] [cyan]disksweep[/] [grey][[command]] [[options]][/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Commands:[Basic][/]");

        var table = new Table().Border(TableBorder.Simple);
        table.AddColumn("[bold]Command[/]");
        table.AddColumn("[bold]Description[/]");
        table.AddRow("[cyan](no arguments)[/]", "Launch the interactive visual terminal dashboard");
        table.AddRow("[cyan]drives[/]", "Display storage capacity and health meters for all drives");
        table.AddRow("[cyan]clean[/]", "Run multi-target cleanup of caches, temporary files, and logs");
        table.AddRow("[cyan]analyze [[path]][/]", "Find space-hog files and major directory clusters");
        table.AddRow("[cyan]--version[/]", "Print version and operating system diagnostic information");
        table.AddRow("[cyan]--help[/]", "Display this help menu");
        AnsiConsole.Write(table);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Options for 'clean':[/]");
        AnsiConsole.MarkupLine("  [grey]--all[/]         Clean all safe targets without prompting");
        AnsiConsole.MarkupLine("  [grey]--dry-run[/]     Scan and report cleanable bytes without deleting");
        AnsiConsole.MarkupLine("  [grey]--trash[/]       Move items to Trash/Recycle Bin instead of permanent deletion");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Options for 'analyze':[/]");
        AnsiConsole.MarkupLine("  [grey]--min-size=MB[/]  Minimum file size threshold in MB (default: 50)");
    }

    #region Drives
    private static void ShowDrivesTable()
    {
        var drives = _driveService.GetDrives();
        if (drives.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No accessible drives found.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[bold]Drive / Mount[/]");
        table.AddColumn("[bold]Label[/]");
        table.AddColumn("[bold]Format[/]");
        table.AddColumn("[bold]Total[/]");
        table.AddColumn("[bold]Free[/]");
        table.AddColumn("[bold]Free %[/]");
        table.AddColumn("[bold]Capacity Meter[/]");
        table.AddColumn("[bold]Health[/]");

        foreach (var d in drives)
        {
            string bar = GenerateCapacityBar(d.UsedPercentage, 16);
            string healthBadge;
            if (d.FreePercentage >= 20.0)
            {
                healthBadge = "[green]HEALTHY[/]";
            }
            else if (d.FreePercentage >= 10.0)
            {
                healthBadge = "[yellow]WARNING[/]";
            }
            else
            {
                healthBadge = "[red]CRITICAL[/]";
            }

            table.AddRow(
                $"[bold cyan]{d.Name}[/]",
                $"[grey]{(string.IsNullOrEmpty(d.VolumeLabel) ? "-" : d.VolumeLabel)}[/]",
                $"[grey]{d.FileSystem}[/]",
                d.TotalDisplay,
                $"[bold]{d.FreeDisplay}[/]",
                $"{d.FreePercentage:F1}%",
                bar,
                healthBadge);
        }

        AnsiConsole.Write(table);

        long totalAll = drives.Sum(d => d.TotalBytes);
        long freeAll = drives.Sum(d => d.FreeBytes);
        double totalFreePct = totalAll > 0 ? (double)freeAll / totalAll * 100.0 : 0;

        AnsiConsole.MarkupLine($"[grey]Total Storage Pool:[/] [bold]{DriveItem.FormatBytes(totalAll)}[/] | [grey]Free Capacity:[/] [bold green]{DriveItem.FormatBytes(freeAll)}[/] ({totalFreePct:F1}% free)");
    }

    private static string GenerateCapacityBar(double usedPercent, int width)
    {
        int filled = (int)Math.Round((usedPercent / 100.0) * width);
        filled = Math.Clamp(filled, 0, width);
        int empty = width - filled;

        string color = usedPercent > 90.0 ? "red" : (usedPercent > 80.0 ? "yellow" : "green");
        return $"[{color}]{new string('█', filled)}[/][grey]{new string('░', empty)}[/]";
    }
    #endregion

    #region Clean
    private static async Task RunCleanCliAsync(bool all, bool dryRun, bool trash)
    {
        var categories = _cleanerService.InitializeCategories();
        AnsiConsole.MarkupLine("[bold dodgerblue1]Scanning cleanable target locations...[/]");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Enumerating temporary files, caches, and logs...", async ctx =>
            {
                foreach (var cat in categories)
                {
                    foreach (var item in cat.Items)
                    {
                        ctx.Status($"Scanning {cat.Name} -> {item.Name}...");
                        await _cleanerService.ScanItemAsync(item, CancellationToken.None);
                    }
                }
            });

        var scanTable = new Table().Border(TableBorder.Rounded);
        scanTable.AddColumn("[bold]Category[/]");
        scanTable.AddColumn("[bold]Target Name[/]");
        scanTable.AddColumn("[bold]Safety[/]");
        scanTable.AddColumn("[bold]Size[/]");
        scanTable.AddColumn("[bold]Files[/]");

        long totalScanBytes = 0;
        int totalScanFiles = 0;

        foreach (var cat in categories)
        {
            foreach (var item in cat.Items)
            {
                if (item.Size == 0 && item.ItemCount == 0) continue;
                totalScanBytes += item.Size;
                totalScanFiles += item.ItemCount;

                string safetyMarkup = item.Safety switch
                {
                    SafetyLevel.Safe => "[green]SAFE[/]",
                    SafetyLevel.Recommended => "[yellow]NORMAL[/]",
                    SafetyLevel.Caution => "[red]CAUTION[/]",
                    _ => "[grey]REVIEW[/]"
                };

                scanTable.AddRow(
                    $"[cyan]{cat.Name}[/]",
                    item.Name,
                    safetyMarkup,
                    item.SizeFormatted,
                    item.ItemCount.ToString("N0"));
            }
        }

        AnsiConsole.Write(scanTable);
        AnsiConsole.MarkupLine($"[bold]Discovered Cleanable Junk:[/] [green]{DriveItem.FormatBytes(totalScanBytes)}[/] ({totalScanFiles:N0} files)");

        if (totalScanBytes == 0)
        {
            AnsiConsole.MarkupLine("[green]System is already sparkling clean! No unnecessary files detected.[/]");
            return;
        }

        if (dryRun)
        {
            AnsiConsole.MarkupLine("[yellow]Dry-run requested: no files were deleted.[/]");
            return;
        }

        List<CleanItem> targetsToClean;
        if (all)
        {
            targetsToClean = categories.SelectMany(c => c.Items).Where(i => i.Size > 0).ToList();
        }
        else if (Console.IsInputRedirected)
        {
            AnsiConsole.MarkupLine("[grey]Non-interactive environment detected. Run with '--all' to execute cleanup directly.[/]");
            return;
        }
        else
        {
            var prompt = new MultiSelectionPrompt<CleanItem>()
                .Title("Select categories to clean (Spacebar to toggle, Enter to confirm):")
                .PageSize(15)
                .UseConverter(i => $"{i.CategoryName} -> {i.Name} ({i.SizeFormatted}, {i.ItemCount} files)");

            foreach (var item in categories.SelectMany(c => c.Items).Where(i => i.Size > 0))
            {
                prompt.AddChoice(item);
            }

            targetsToClean = AnsiConsole.Prompt(prompt);
        }

        if (targetsToClean.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No targets selected for cleanup.[/]");
            return;
        }

        long selectedBytes = targetsToClean.Sum(t => t.Size);
        if (!Console.IsInputRedirected && !all)
        {
            if (!AnsiConsole.Confirm($"Permanently delete [green]{DriveItem.FormatBytes(selectedBytes)}[/] of junk across {targetsToClean.Count} targets?"))
            {
                AnsiConsole.MarkupLine("[grey]Cleanup cancelled by user.[/]");
                return;
            }
        }

        int totalDeleted = 0;
        long totalFreed = 0;
        int totalSkipped = 0;

        await AnsiConsole.Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var cleanTask = ctx.AddTask("[green]Sweeping system junk...[/]", maxValue: targetsToClean.Count);

                foreach (var item in targetsToClean)
                {
                    cleanTask.Description = $"Cleaning {item.Name}...";
                    var result = await _cleanerService.CleanItemAsync(
                        item,
                        safeMode24Hours: _settingsService.Current.SafeMode24Hours,
                        moveToRecycleBin: trash || _settingsService.Current.MoveToRecycleBinInsteadOfPermanent,
                        progress: null,
                        CancellationToken.None);

                    totalDeleted += result.DeletedFiles;
                    totalFreed += result.FreedBytes;
                    totalSkipped += result.SkippedFiles;
                    cleanTask.Increment(1);
                }
            });

        AnsiConsole.MarkupLine($"[bold green]Sweep Complete![/] [bold]{DriveItem.FormatBytes(totalFreed)}[/] freed ({totalDeleted:N0} files removed, {totalSkipped:N0} active locked files skipped).");
    }
    #endregion

    #region Analyze & Delete
    private static async Task RunAnalyzeCliAsync(string? targetPath, int minSizeMB)
    {
        long minSizeBytes = (long)minSizeMB * 1024 * 1024;
        List<string> roots = new();

        if (!string.IsNullOrWhiteSpace(targetPath))
        {
            roots.Add(targetPath);
        }
        else
        {
            roots.AddRange(_driveService.GetDrives().Select(d => d.Name));
        }

        AnsiConsole.MarkupLine($"[bold dodgerblue1]Scanning for large files (>= {minSizeMB} MB)...[/]");

        LargeFileScannerService.ScanResult result = null!;

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .StartAsync("Cataloging storage consumption across drives...", async ctx =>
            {
                var prog = new Progress<LargeFileScannerService.ScanProgress>(p =>
                {
                    ctx.Status($"Scanning: [grey]{Truncate(p.CurrentFolder, 45)}[/] ({p.MatchesFound} large files found)");
                });

                result = await _scannerService.ScanSpaceAsync(roots, minSizeBytes, 100, prog, CancellationToken.None);
            });

        if (result.Files.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No files found exceeding the size threshold.[/]");
            return;
        }

        // 1. Show Major Folders
        AnsiConsole.MarkupLine("\n[bold underline]📁 Major Directory Clusters Consuming Space:[/]");
        var folderTable = new Table().Border(TableBorder.Rounded);
        folderTable.AddColumn("[bold]Folder Name[/]");
        folderTable.AddColumn("[bold]Size[/]");
        folderTable.AddColumn("[bold]Files[/]");
        folderTable.AddColumn("[bold]Safety Status[/]");
        folderTable.AddColumn("[bold]Full Path[/]");

        foreach (var folder in result.TopFolders.Take(15))
        {
            string safety = folder.IsHighRisk ? "[red]PROTECTED APPLICATION[/]" : "[green]USER FOLDER[/]";
            folderTable.AddRow(
                $"[bold cyan]{folder.FolderName}[/]",
                $"[bold]{folder.TotalSizeFormatted}[/]",
                folder.FileCount.ToString("N0"),
                safety,
                $"[grey]{folder.FolderPath}[/]");
        }
        AnsiConsole.Write(folderTable);

        // 2. Show Large Files
        AnsiConsole.MarkupLine("\n[bold underline]📄 Top Individual Large Files:[/]");
        var fileTable = new Table().Border(TableBorder.Rounded);
        fileTable.AddColumn("[bold]#[/]");
        fileTable.AddColumn("[bold]File Name[/]");
        fileTable.AddColumn("[bold]Size[/]");
        fileTable.AddColumn("[bold]Safety Status[/]");
        fileTable.AddColumn("[bold]Context / Application[/]");
        fileTable.AddColumn("[bold]Path[/]");

        for (int i = 0; i < Math.Min(result.Files.Count, 25); i++)
        {
            var f = result.Files[i];
            string safety = f.IsHighRisk ? "[red]CORRUPTION RISK[/]" : "[green]SAFE USER FILE[/]";
            fileTable.AddRow(
                $"[grey]{i + 1}[/]",
                $"[bold]{f.FileName}[/]",
                $"[bold yellow]{f.SizeFormatted}[/]",
                safety,
                f.ParentContext,
                $"[grey]{Truncate(f.DirectoryPath, 40)}[/]");
        }
        AnsiConsole.Write(fileTable);

        // 3. Direct Deletion Option in Terminal
        AnsiConsole.WriteLine();
        if (!Console.IsInputRedirected && AnsiConsole.Confirm("Would you like to select and [red]delete[/] any of these files right now?"))
        {
            var prompt = new MultiSelectionPrompt<LargeFileItem>()
                .Title("Select files to delete (Spacebar to toggle, Enter to confirm):")
                .PageSize(15)
                .UseConverter(f => $"{(f.IsHighRisk ? "[red]⚠️[/]" : "[green]✓[/]")} {f.FileName} ({f.SizeFormatted}) - [grey]{Truncate(f.DirectoryPath, 35)}[/]");

            foreach (var f in result.Files.Take(30))
            {
                prompt.AddChoice(f);
            }

            var chosen = AnsiConsole.Prompt(prompt);
            if (chosen.Count > 0)
            {
                // Anti-Corruption Guard
                var highRiskItems = chosen.Where(f => f.IsHighRisk).ToList();
                if (highRiskItems.Count > 0)
                {
                    AnsiConsole.MarkupLine("\n[bold red]⚠️  CRITICAL GAME & APPLICATION CORRUPTION WARNING ⚠️[/]");
                    AnsiConsole.MarkupLine("[yellow]You have selected files that belong to installed games or system applications:[/]");
                    foreach (var h in highRiskItems)
                    {
                        AnsiConsole.MarkupLine($" - [bold red]{h.FileName}[/] ({h.ParentContext}) in [grey]{h.DirectoryPath}[/]");
                    }
                    AnsiConsole.MarkupLine("\n[red]Deleting game chunks (.pak/.ucas/binaries) WILL CORRUPT THE GAME.[/]");
                    AnsiConsole.MarkupLine("To properly free space, uninstall the game via Steam, Epic Games, or System Settings.");

                    if (!AnsiConsole.Confirm("[bold red]Are you ABSOLUTELY POSITIVE you still want to delete these files?[/]"))
                    {
                        AnsiConsole.MarkupLine("[grey]Deletion aborted.[/]");
                        return;
                    }
                }

                int deleted = 0;
                long bytes = 0;
                foreach (var f in chosen)
                {
                    bool ok = TrashService.MoveToTrash(f.FilePath);
                    if (!ok && File.Exists(f.FilePath))
                    {
                        try { File.Delete(f.FilePath); ok = true; } catch { }
                    }

                    if (ok || !File.Exists(f.FilePath))
                    {
                        deleted++;
                        bytes += f.Size;
                        AnsiConsole.MarkupLine($"[green]✓ Deleted:[/] {f.FileName} ({f.SizeFormatted})");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]✗ Failed to delete:[/] {f.FileName} (file may be in use)");
                    }
                }

                AnsiConsole.MarkupLine($"\n[bold green]Finished:[/] Removed {deleted} files, reclaimed [bold green]{DriveItem.FormatBytes(bytes)}[/] storage.");
            }
        }
    }
    #endregion

    #region Interactive Dashboard
    private static async Task RunInteractiveDashboardAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            ShowBanner();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold dodgerblue1]What would you like to do?[/]")
                    .PageSize(10)
                    .AddChoices(
                        "1. 📊 Inspect Storage Drives & Capacity",
                        "2. ⚡ Deep Clean Junk & Caches",
                        "3. 🔎 Space Analyzer & Direct File Deletion",
                        "4. ⚙️ Settings & Configuration",
                        "5. 🚪 Exit"));

            switch (choice[0])
            {
                case '1':
                    AnsiConsole.Clear();
                    ShowBanner();
                    ShowDrivesTable();
                    WaitKey();
                    break;

                case '2':
                    AnsiConsole.Clear();
                    ShowBanner();
                    await RunCleanCliAsync(all: false, dryRun: false, trash: false);
                    WaitKey();
                    break;

                case '3':
                    AnsiConsole.Clear();
                    ShowBanner();
                    await RunAnalyzeCliAsync(null, 50);
                    WaitKey();
                    break;

                case '4':
                    AnsiConsole.Clear();
                    ShowBanner();
                    RunSettingsMenu();
                    break;

                case '5':
                    AnsiConsole.MarkupLine("[bold dodgerblue1]Goodbye![/]");
                    return;
            }
        }
    }

    private static void RunSettingsMenu()
    {
        var s = _settingsService.Current;
        var toggle = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Settings Configuration:[/]")
                .AddChoices(
                    $"1. Toggle 24-Hour Safe Mode Cushion: {(s.SafeMode24Hours ? "[green]ENABLED[/]" : "[red]DISABLED[/]")}",
                    $"2. Toggle Trash vs Permanent Deletion: {(s.MoveToRecycleBinInsteadOfPermanent ? "[cyan]MOVE TO TRASH[/]" : "[red]PERMANENT DELETE[/]")}",
                    "3. Return to Main Dashboard"));

        if (toggle.StartsWith("1."))
        {
            s.SafeMode24Hours = !s.SafeMode24Hours;
            _settingsService.SaveSettings(s);
            AnsiConsole.MarkupLine("[green]Updated Safe Mode setting.[/]");
            Thread.Sleep(800);
        }
        else if (toggle.StartsWith("2."))
        {
            s.MoveToRecycleBinInsteadOfPermanent = !s.MoveToRecycleBinInsteadOfPermanent;
            _settingsService.SaveSettings(s);
            AnsiConsole.MarkupLine("[green]Updated Deletion Destination setting.[/]");
            Thread.Sleep(800);
        }
    }

    private static void WaitKey()
    {
        AnsiConsole.WriteLine();
        if (!Console.IsInputRedirected)
        {
            AnsiConsole.MarkupLine("[grey]Press any key to return to menu...[/]");
            try { Console.ReadKey(true); } catch { }
        }
    }

    private static string Truncate(string str, int maxLen)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLen) return str;
        return "..." + str[^maxLen..];
    }
    #endregion
}
