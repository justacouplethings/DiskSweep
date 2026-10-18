<p align="center">
  <img src="Assets/app_icon.png" width="160" alt="DiskSweep Logo" />
</p>

<h1 align="center">DiskSweep</h1>

<p align="center">
  <strong>Fast, utilitarian storage capacity monitor and zero-harm system cleanup utility for Windows.</strong>
</p>

<p align="center">
  <a href="https://github.com/justacouplethings/DiskSweep/releases"><img src="https://img.shields.io/badge/release-v1.1.1-blue.svg" alt="Release"></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-007acc.svg" alt="Platform">
  <img src="https://img.shields.io/badge/framework-.NET%2010%20WPF-512bd4.svg" alt="Framework">
  <img src="https://img.shields.io/badge/privileges-Administrator%20UAC-red.svg" alt="Privileges">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-green.svg" alt="License"></a>
</p>

---

## 🚀 Overview

**DiskSweep** is built for power users who demand clear visibility over their drives and swift, non-destructive junk removal. Inspired by classic Windows engineering utilities like **WinDirStat**, **RAMMap**, and **Process Hacker**, DiskSweep brings that utilitarian, high-density productivity into a modern, sleek multi-theme UI.

- **Storage Capacity & Health**: Instant inspection of all active local, external, and removable drives with responsive non-colliding capacity meters.
- **Deep Clean Engine**: Scans safe-to-delete temporary files, crash dumps, shader caches, browser caches, Windows Update leftovers, and package manager artifacts.
- **Stationary Telemetry**: Zero text jitter during active sweeps; fixed-column layout for cleaned bytes, deleted file count, and operation status.
- **Space Analyzer 2.0**: Drive target selection (individual drive or all drives system scan), major folder aggregation, and game corruption safety protection.
- **5 Sleek Themes**: Charcoal Dark, OLED Pure Black, Midnight Navy, Matrix Green, and Classic Light (White Theme).
- **Fully Themed Win32 Controls**: Sleek modern scrollbars and themed dropdowns with zero white-on-white rendering bugs.
- **Always Elevated**: Native UAC manifest (`requireAdministrator`) ensures full access to system-level temp and diagnostic directories.

---

## ⚡ Quick Start (Portable)

No installation required. 

1. Download [`DiskSweep-v1.1.1-Portable.zip`](release/DiskSweep-v1.1.1-Portable.zip) from the repository.
2. Extract the archive.
3. Run `DiskSweep.exe` (or launch via `run.bat`).

*(Requires Windows 10/11 and .NET 10 Desktop Runtime)*

---

## 🔍 Features in Detail

### 1. Drive Capacity & Health Meters
- Real-time enumeration of all attached drives.
- Dynamic color-coded capacity meters:
  - 🟢 **Healthy**: > 20% free space remaining
  - 🟠 **Warning**: 10% – 20% free space remaining
  - 🔴 **Critical**: < 10% free space remaining
- Reports filesystem (NTFS, FAT32, exFAT, ReFS), volume labels, exact used space, and free capacity percentage.
- One-click context menu to explore drive in File Explorer or launch Windows Disk Management / Storage Sense.

### 2. Multi-Target Safe Cleanup Engine
Cleans junk without breaking Windows or user applications:
- **System Temporary Files**: `%TEMP%`, Windows System Temp (`C:\Windows\Temp`), and Prefetch traces.
- **Windows Updates & Diagnostics**: Downloaded Windows Update cache (`SoftwareDistribution\Download`), Windows Error Reporting (WER) logs, user-mode crash dumps, and BSOD kernel minidumps.
- **Graphics & Shader Caches**: DirectX 11/12 GPU caches (`D3DSCache`), NVIDIA DXCache/GLCache, and AMD Radeon shader pipelines.
- **Web Browser Caches**: Google Chrome, Microsoft Edge, Brave Browser, Mozilla Firefox, Opera, and Opera GX HTTP and GPU caches (passwords, logins, and history are never touched).
- **Application & Gaming Caches**: Discord media and code caches, Steam CEF browser cache, Spotify stream cache.
- **Developer Package Caches**: Python Pip cache, Node.js NPM cache, and Rust Cargo registry cache.
- **Windows Recycle Bin**: Integrated shell API to query and empty discarded items across all volumes.

### 3. Space Analyzer (Large Files)
- Scans any selected drive for the top 100 largest files (>50 MB).
- Sorts by size and displays file name, extension, last modified date, and path.
- Context menu allows moving large forgotten files directly to the Recycle Bin or jumping to their folder in Explorer.

### 4. Interactive Execution & Audit Log
- High-contrast monospace log detailing every path scanned, bytes reclaimed, and active files skipped.
- Real-time progress bar and counter tracking total items selected and disk space to be recovered.

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Description |
|---|---|
| `F5` | Start System Scan |
| `Ctrl + R` | Refresh Drives and Storage Statistics |
| `Ctrl + A` | Select All Cleanable Targets |
| `Esc` | Cancel Active Scan or Deletion |
| `F1` | About DiskSweep |

---

## 🛠️ Building from Source

```cmd
git clone https://github.com/justacouplethings/DiskSweep.git
cd DiskSweep
dotnet build -c Release
```

The output binary will be located at:
`bin\Release\net10.0-windows\DiskSweep.exe`

---

## 📄 License

Distributed under the [MIT License](LICENSE).
