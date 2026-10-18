<p align="center">
  <img src="Assets/app_icon.png" width="160" alt="DiskSweep Logo" />
</p>

<h1 align="center">DiskSweep</h1>

<p align="center">
  <strong>Fast, utilitarian storage capacity monitor and zero-harm system cleanup utility for Windows, macOS, and Linux.</strong>
</p>

<p align="center">
  <a href="https://github.com/justacouplethings/DiskSweep/releases/tag/v1.2.0"><img src="https://img.shields.io/badge/release-v1.2.0-blue.svg" alt="Release"></a>
  <img src="https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-007acc.svg" alt="Platforms">
  <img src="https://img.shields.io/badge/framework-.NET%2010%20%7C%20Spectre.Console-512bd4.svg" alt="Framework">
  <img src="https://img.shields.io/badge/privileges-Administrator%20UAC%20%7C%20root-red.svg" alt="Privileges">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green.svg" alt="License"></a>
</p>

---

## 🚀 Overview

**DiskSweep** is built for power users and sysadmins who demand instant visibility over their storage and swift, zero-harm junk removal. Inspired by classic engineering utilities like **WinDirStat**, **RAMMap**, and **Process Hacker**, DiskSweep combines high-density productivity with cross-platform performance:

- **Universal Multi-Platform Support**: Native Windows WPF desktop application with hardware-accelerated rendering, alongside standalone native CLI binaries for **macOS** (Apple Silicon & Intel) and **Linux** (x64).
- **Storage Capacity & Health Meters**: Real-time inspection of active drives (NTFS, APFS, ext4, btrfs, FAT32, exFAT) with color-coded health indicators and responsive volume headers.
- **Deep Clean Engine**: Swiftly purges temporary files, crash dumps, GPU shader caches, browser caches, OS updates, and package manager leftovers (APT, Pacman, DNF, Pip, NPM, Cargo, Homebrew).
- **Space Analyzer with Direct In-App Deletion**: Scan drives for large space hogs and major directory clusters. Directly delete individual files or clean entire unneeded folders with one click (`🗑️ Delete Selected`, `🗑️ Delete`, `🗑️ Clean Folder`).
- **Cross-Platform Game Corruption Guard**: Automatically identifies and protects game chunk files (`.pak`, `.ucas`, `.rpf`, `.vpk`) across Steam, Epic Games, Riot, Battle.net, macOS `/Applications`, Linux Flatpak/Snap, and Wine/Proton prefixes.
- **Stationary Zero-Jitter Telemetry**: Fixed-width telemetry columns separate live reclaimed byte counters from changing file paths, eliminating layout jitter during high-speed sweeps.
- **5 Built-In Visual Themes**: Charcoal Dark, OLED Pure Black, Midnight Navy, Matrix Green, and Classic Light (White Theme) with custom Win32 scrollbars and dropdowns.
- **Interactive Terminal Suite (`disksweep`)**: Interactive TUI dashboard powered by `Spectre.Console` for SSH sessions, headless servers, and terminal workflows.

---

## ⚡ Quick Start

### 🪟 Windows (Portable GUI App)
No installation required.
1. Download [`DiskSweep-v1.2.0-Portable.zip`](https://github.com/justacouplethings/DiskSweep/releases/download/v1.2.0/DiskSweep-v1.2.0-Portable.zip).
2. Extract the archive anywhere.
3. Run `DiskSweep.exe` (or launch via `run.bat`). Automatically prompts for Administrator privileges to ensure complete system access.

*(Requires Windows 10/11 and .NET 10 Desktop Runtime)*

### 🍎 macOS & 🐧 Linux (One-Line Installer)
Install the standalone, zero-dependency `disksweep` binary with one command:

```bash
curl -fsSL https://raw.githubusercontent.com/justacouplethings/DiskSweep/main/install.sh | bash
```

Or download standalone tarballs directly from [Releases](https://github.com/justacouplethings/DiskSweep/releases/tag/v1.2.0):
- **macOS Apple Silicon (M1/M2/M3/M4)**: [`DiskSweep-v1.2.0-macOS-arm64.tar.gz`](https://github.com/justacouplethings/DiskSweep/releases/download/v1.2.0/DiskSweep-v1.2.0-macOS-arm64.tar.gz)
- **macOS Intel (x64)**: [`DiskSweep-v1.2.0-macOS-x64.tar.gz`](https://github.com/justacouplethings/DiskSweep/releases/download/v1.2.0/DiskSweep-v1.2.0-macOS-x64.tar.gz)
- **Linux (x64)**: [`DiskSweep-v1.2.0-Linux-x64.tar.gz`](https://github.com/justacouplethings/DiskSweep/releases/download/v1.2.0/DiskSweep-v1.2.0-Linux-x64.tar.gz)
- **Windows CLI**: [`DiskSweep-v1.2.0-Windows-CLI.zip`](https://github.com/justacouplethings/DiskSweep/releases/download/v1.2.0/DiskSweep-v1.2.0-Windows-CLI.zip)

---

## 💻 Universal CLI Usage (`disksweep`)

The `disksweep` command-line utility provides both an interactive terminal dashboard and non-interactive scriptable subcommands:

```bash
# Launch interactive terminal dashboard (TUI)
disksweep

# Display all mounted drives and storage capacity meters
disksweep drives

# Deep clean junk targets with preview/dry-run
disksweep clean --dry-run

# Run full deep clean across all targets
disksweep clean --all

# Clean and send files to Trash / Recycle Bin instead of permanent deletion
disksweep clean --all --trash

# Analyze large files on current drive (or specify path)
disksweep analyze
disksweep analyze /var --min-size=100
```

---

## 🎨 Built-In GUI Themes

DiskSweep includes 5 custom-crafted themes switchable live from the **Settings** tab or top menu bar:

1. **Charcoal Dark** (Default): Classic Visual Studio dark palette (`#181818` / `#007ACC`).
2. **OLED Pure Black**: True deep black (`#000000`) with vibrant neon mint accents (`#00FF9D`).
3. **Midnight Navy**: Nord-inspired cyber night navy (`#0B0F17` / `#3B82F6`).
4. **Matrix Green**: High-contrast terminal emerald dark (`#060A06` / `#10B981`).
5. **Classic Light**: Clean white aesthetic (`#F4F5F7` / `#0066CC`) with dark typography for bright environments.

---

## 🔍 Features in Detail

### 1. Drive Capacity & Health Meters
- Real-time enumeration of all attached drives across Windows drive letters, macOS `/Volumes`, and Linux mount points.
- Filters out virtual pseudo-filesystems (`tmpfs`, `devtmpfs`, `sysfs`, `proc`, `squashfs`, `overlay`).
- Dynamic color-coded capacity meters:
  - 🟢 **Healthy**: > 20% free space remaining
  - 🟠 **Warning**: 10% – 20% free space remaining
  - 🔴 **Critical**: < 10% free space remaining
- Non-colliding responsive layout displaying total storage pool, free capacity, filesystem, and volume labels.

### 2. Multi-Target Safe Cleanup Engine
Cleans junk without breaking applications or user configurations:
- **System Temporary Files**: Windows `%TEMP%`, `C:\Windows\Temp`, Prefetch, macOS `/tmp`, Linux `/tmp`.
- **System Logs & Diagnostics**: Windows Error Reporting (WER) logs, crash dumps, kernel minidumps, macOS `DiagnosticReports`, Linux `/var/log` and `/var/crash`.
- **Graphics & Shader Caches**: DirectX 11/12 GPU caches (`D3DSCache`), NVIDIA DXCache/GLCache, and AMD Radeon shader pipelines.
- **Web Browser Caches**: Google Chrome, Apple Safari, Microsoft Edge, Brave Browser, Mozilla Firefox, Opera, and Opera GX HTTP and GPU caches (passwords, logins, and history are never touched).
- **Application & Gaming Caches**: Discord media/code caches, Steam CEF web cache, Spotify stream cache, Xcode `DerivedData` & `iOS DeviceSupport`.
- **Package Manager Caches**: APT archives (`/var/cache/apt/archives`), Pacman pkg (`/var/cache/pacman/pkg`), DNF cache (`/var/cache/dnf`), Python Pip, Node.js NPM, Rust Cargo, and Homebrew cache.
- **Trash & Recycle Bin**: Native shell integration for Windows Recycle Bin, macOS `~/.Trash`, and Linux FreeDesktop XDG Trash.

### 3. Space Analyzer with Direct Deletion & Game Guard
- **Drive Target Scope**: Scan a specific local drive or select `🌐 All Drives (Full System Scan)`.
- **Direct File & Folder Deletion**: Delete space hogs directly inside the UI via inline `🗑️ Delete` row buttons or `🗑️ Delete Selected` toolbar button.
- **Major Folder Clustering**: Switch between individual files and `📁 Major Folders` with `🗑️ Clean Folder` actions.
- **Game Corruption Safeguard**: Analyzes directory context and tags game assets (Steam, Epic, Riot, Blizzard, Flatpak, Wine/Proton) with 🔴 **HIGH RISK** badges to warn users against corrupting installed games.
- **Dynamic Parent Metric Sync**: Deleting individual files dynamically updates parent directory sizes and file counters in real time.

---

## ⌨️ Keyboard Shortcuts (Windows GUI)

| Shortcut | Description |
|---|---|
| `F5` | Start System Scan |
| `Ctrl + R` | Refresh Drives and Storage Statistics |
| `Ctrl + A` | Select All Cleanable Targets |
| `Ctrl + ,` | Open Settings & Personalization |
| `Del` | Delete Selected Large File |
| `Esc` | Cancel Active Scan or Deletion |
| `F1` | About DiskSweep |

---

## 🛠️ Building from Source

### Windows WPF Application:
```cmd
git clone https://github.com/justacouplethings/DiskSweep.git
cd DiskSweep
dotnet build -c Release
```
Output executable: `bin\Release\net10.0-windows\DiskSweep.exe`

### Cross-Platform CLI Tool (`disksweep`):
```bash
# Build standalone native binary for your current OS:
dotnet publish DiskSweep.Cli -c Release --self-contained -p:PublishSingleFile=true -o publish/
```

---

## 📜 Changelog

See [CHANGELOG.md](CHANGELOG.md) for the full release history, cross-platform notes, and safety improvements.

---

## 📄 License

Distributed under the [MIT License](LICENSE).
