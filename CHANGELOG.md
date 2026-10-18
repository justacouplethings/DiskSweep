# Changelog

All notable changes to the **DiskSweep** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-10-18

### 🍎 Native macOS Support
- **Cross-Platform Storage Engine**: Real-time inspection and capacity metrics for APFS container partitions, Macintosh HD root volumes, and external drives attached under `/Volumes`.
- **macOS Targeted Cleaners**:
  - User and System Caches (`~/Library/Caches`, `/Library/Caches`).
  - Application Caches for Google Chrome, Apple Safari, Mozilla Firefox, Microsoft Edge, Discord, Spotify, and Homebrew.
  - Xcode Developer Cache (`~/Library/Developer/Xcode/DerivedData`, `iOS DeviceSupport`).
  - Crash logs and analytics (`~/Library/Logs`, `/Library/Logs/DiagnosticReports`).
  - Native macOS Trash (`~/.Trash`).
- **Apple Silicon & Intel Binaries**: Native standalone pre-compiled single-file binaries for `macos-arm64` (Apple Silicon M1/M2/M3/M4) and `macos-x64` (Intel Mac).

### 🐧 Native Linux Support
- **Unix Storage Engine & Mount Filtration**: Enumeration of physical ext4, btrfs, xfs, zfs, and f2fs filesystems while filtering out virtual pseudo-filesystems (`tmpfs`, `devtmpfs`, `sysfs`, `proc`, `squashfs`, `overlay`).
- **Linux Targeted Cleaners**:
  - Standard user cache (`~/.cache`).
  - Linux application caches for Chrome, Firefox, Discord, Spotify, and system thumbnailers.
  - System logs & diagnostic crash dumps (`/var/log`, `/var/crash`).
  - Package manager archive caches for APT (`/var/cache/apt/archives`), Pacman (`/var/cache/pacman/pkg`), and DNF/Fedora (`/var/cache/dnf`).
  - FreeDesktop XDG Trash standard compliance (`~/.local/share/Trash/files` and metadata `.trashinfo` purging).
- **Standalone Linux Binary**: Native single-file binary for `linux-x64` requiring zero external .NET dependencies.

### 🗑️ Space Analyzer Direct File & Folder Deletion
- **Direct Deletion Controls**: Added direct file deletion to the File Analyzer DataGrid via an inline `🗑️ Delete` button on every row, plus a global `🗑️ Delete Selected` toolbar button with confirmation dialog.
- **Folder Cleanup Actions**: Added `🗑️ Clean Folder` button to the `📁 Major Folders` DataGrid and right-click context menu, allowing users to safely purge entire unneeded directories without leaving the app.
- **Dynamic Parent Metric Synchronization**: Deleting a file dynamically decrements its parent folder's tracked size and file count in the UI, keeping the Major Folders view synchronized in real time.
- **Trash & Recycle Bin Routing with Fallback**: Deletions route through the platform's native trash bin (Recycle Bin on Windows, `~/.Trash` on macOS, XDG Trash on Linux) with automatic permanent deletion fallback when trashes are unavailable.
- **Cross-Platform Game Corruption Protection**: Extended the game and application protection interceptor across Windows (Steam, Epic, Riot, Xbox, Blizzard), macOS (`/Applications`, Steam app support), and Linux (`~/.local/share/Steam`, Flatpak, Snap, Wine/Proton prefixes).

### ⚡ Universal Cross-Platform CLI Suite (`disksweep`)
- **Interactive Terminal Dashboard**: Full interactive TUI powered by `Spectre.Console` featuring color-coded capacity meters, deep clean multi-select checklist, and space analyzer.
- **Scriptable Automation Commands**:
  - `disksweep drives`: Table overview of storage capacity, format, free percentage, and health status.
  - `disksweep clean [--all] [--dry-run] [--trash]`: Automated command-line junk removal with dry-run previews.
  - `disksweep analyze [path] [--min-size=MB]`: Scan and inspect space hogs directly in the terminal.
- **One-Liner Install Script**: Provided `install.sh` for instant curl/wget installation on macOS and Linux.

---

## [1.1.1] - 2026-10-18

### 🛡️ Space Analyzer 2.0 & Game Corruption Guard
- **Major Directory Aggregation**: Space Analyzer now groups large multi-gigabyte files into their parent directories (`📁 Major Folders` view). This prevents users from blindly deleting isolated game chunks (`.ucas`, `.pak`, `.rpf`, `.vpk`) without realizing they belong to an active game or software installation.
- **Safety Status & Risk Badges**: Implemented intelligent path classification for scanned items:
  - 🔴 **HIGH RISK**: Files located in Steam libraries, Epic Games, Riot Games, Blizzard/Battle.net, `Program Files`, or `C:\Windows`. Deleting individual files in these trees will corrupt games or system runtimes.
  - 🟠 **MODERATE RISK**: Large archives and downloads located in user profiles or common tools.
  - 🟢 **SAFE USER FILE**: Standalone media (video recordings, disk images, ISOs) outside executable program trees.
- **Game Corruption Interceptor**: Deleting a High-Risk game file prompts an explicit warning dialog advising the user to uninstall the game properly via its launcher (Steam, Epic, Xbox app) rather than manually deleting chunk files.
- **Warning Notice Banner**: Added a high-visibility warning banner at the top of the Space Analyzer workspace detailing the risks of manual file deletion in application directories.

### 🌐 Multi-Drive Target Scope
- **Full System Scan Support**: Added `🌐 All Drives (Full System Scan)` option in the target scope dropdown, allowing one-click indexing across all connected local and external drives simultaneously, alongside individual drive targeting (`C:\`, `D:\`, etc.).

### 🎨 Fully Themed Win32 & Custom Controls
- **Zero White-on-White Dropdown Glitches**: Completely rewrote the `ComboBox`, `ComboBoxItem`, and `ToggleButton` control templates in XAML. Native OS Aero white popups are replaced with dynamic theme brushes (`SurfaceDarkBrush`, `PanelDarkBrush`, `AccentBlueBrush`).
- **Sleek Custom ScrollBars**: Replaced standard OS gray sliders with modern, arrowless Win11-style scrollbars that smoothly adapt to the active theme palette. Includes hover and active dragging feedback.
- **Classic Light Theme Added**: Introduced a 5th built-in theme: **Classic Light** (clean white palette `#F4F5F7` background, `#FFFFFF` surface, `#0066CC` accent, `#212529` dark typography) for users who prefer a bright, high-readability daytime aesthetic.

### ⚡ Stationary Zero-Jitter Telemetry & UI Fixes
- **Stationary Metric Telemetry**: Fixed status bar layout by isolating the live action indicator, reclaimed byte counter, deleted file count, and active file path into dedicated, fixed-width columns. Text no longer bounces or shifts when variable-length file paths are processed.
- **Header Collision Resolved**: Converted the volume capacity summary header into a 3-column responsive grid, preventing the `STORAGE CAPACITY & DRIVE HEALTH` title from overlapping `Total Storage` and `Free Space` metrics on narrow windows.

---

## [1.1.0] - 2026-10-18

### ✨ Added
- **Multi-Theme Engine**: Introduced 4 built-in high-contrast themes: Charcoal Dark (Default), OLED Pure Black, Midnight Navy, and Matrix Green.
- **Personalization Settings Tab**: Added dedicated settings workspace with theme picker radio cards, configurable minimum large file threshold (25 MB – 1 GB), and confirmation toggles.
- **Safe Mode Time-Cushion (24-Hour Guard)**: Only delete cache and temporary files created more than 24 hours ago, preventing conflicts with running installers and active sessions.
- **Recycle Bin Fallback Mode**: Option to move cleaned files to the Windows Recycle Bin instead of permanent deletion.

### ⚡ Performance & Polish
- **Throttled Parallel Deletion**: Improved deletion responsiveness by batching UI dispatches, eliminating micro-stutters during high-file-count sweeps.
- **DWM Immersive Dark Titlebar**: Integrated `DwmSetWindowAttribute` API to apply native dark titlebars on Windows 10/11.

---

## [1.0.0] - 2026-10-18

### 🚀 Initial Public Release
- **Drive Capacity Monitor**: Real-time inspection of attached volumes with color-coded health meters (Healthy, Warning, Critical).
- **Multi-Target Cleanup Engine**: Deep clean routines covering `%TEMP%`, Windows System Temp, Prefetch, Windows Update cache, WER crash dumps, GPU shader caches (DirectX, NVIDIA, AMD), browser caches (Chrome, Edge, Firefox, Brave, Opera), and developer caches (Pip, NPM, Cargo).
- **Top 100 Space Hogs Analyzer**: Catalog large files over 50 MB with File Explorer and Recycle Bin context actions.
- **Native UAC Elevation**: Embedded `requireAdministrator` manifest ensures full access to protected system directories.
- **Portable Architecture**: Zero installer required; runs directly from portable folder.
