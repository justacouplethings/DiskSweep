# Changelog

All notable changes to the **DiskSweep** project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
