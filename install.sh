#!/usr/bin/env bash
# DiskSweep Cross-Platform Installer for macOS & Linux
set -euo pipefail

REPO="justacouplethings/DiskSweep"
VERSION="v1.2.0"

echo "============================================================"
echo "          DiskSweep Universal CLI Installer ($VERSION)"
echo "============================================================"

# 1. Detect Operating System
OS="$(uname -s)"
case "$OS" in
    Darwin)
        OS_TYPE="macos"
        ;;
    Linux)
        OS_TYPE="linux"
        ;;
    *)
        echo "[!] Unsupported Operating System: $OS"
        exit 1
        ;;
esac

# 2. Detect Architecture
ARCH="$(uname -m)"
case "$ARCH" in
    x86_64|amd64)
        ARCH_TYPE="x64"
        ;;
    arm64|aarch64)
        ARCH_TYPE="arm64"
        ;;
    *)
        echo "[!] Unsupported architecture: $ARCH"
        exit 1
        ;;
esac

# Validate platform combination
TARGET="${OS_TYPE}-${ARCH_TYPE}"
if [ "$TARGET" = "linux-arm64" ]; then
    echo "[!] linux-arm64 is not currently in the pre-built matrix. Please build from source using 'dotnet publish'."
    exit 1
fi

TARBALL="DiskSweep-${VERSION}-${OS_TYPE}-${ARCH_TYPE}.tar.gz"
DOWNLOAD_URL="https://github.com/${REPO}/releases/download/${VERSION}/${TARBALL}"

echo "[*] Detected platform: $OS_TYPE ($ARCH_TYPE)"

# Determine installation directory
if [ -w "/usr/local/bin" ]; then
    INSTALL_DIR="/usr/local/bin"
    SUDO_CMD=""
elif command -v sudo >/dev/null 2>&1; then
    INSTALL_DIR="/usr/local/bin"
    SUDO_CMD="sudo"
else
    INSTALL_DIR="${HOME}/.local/bin"
    mkdir -p "$INSTALL_DIR"
    SUDO_CMD=""
fi

TMP_DIR="$(mktemp -d)"
cleanup() {
    rm -rf "$TMP_DIR"
}
trap cleanup EXIT

# 3. Check for local build first (if cloned from repo)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" 2>/dev/null && pwd || echo "")"
LOCAL_BIN="${SCRIPT_DIR}/dist/${OS_TYPE}-${ARCH_TYPE}/disksweep"

if [ -f "$LOCAL_BIN" ]; then
    echo "[*] Found local build artifact at $LOCAL_BIN"
    CP_SRC="$LOCAL_BIN"
else
    echo "[*] Downloading $TARBALL..."
    if command -v curl >/dev/null 2>&1; then
        curl -fsSL "$DOWNLOAD_URL" -o "${TMP_DIR}/${TARBALL}"
    elif command -v wget >/dev/null 2>&1; then
        wget -qO "${TMP_DIR}/${TARBALL}" "$DOWNLOAD_URL"
    else
        echo "[!] Neither curl nor wget found. Please install one of them."
        exit 1
    fi

    echo "[*] Extracting..."
    tar -xzf "${TMP_DIR}/${TARBALL}" -C "$TMP_DIR"
    CP_SRC="${TMP_DIR}/disksweep"
fi

# 4. Install binary
echo "[*] Installing disksweep to ${INSTALL_DIR}/disksweep..."
$SUDO_CMD install -m 755 "$CP_SRC" "${INSTALL_DIR}/disksweep"

# 5. Verify PATH
if ! echo "$PATH" | tr ':' '\n' | grep -qx "$INSTALL_DIR"; then
    echo ""
    echo "[!] Warning: $INSTALL_DIR is not in your \$PATH."
    echo "    Add it by running:"
    echo "      export PATH=\"${INSTALL_DIR}:\$PATH\""
    echo ""
fi

# 6. Verify execution
echo ""
echo "[✓] Installation complete!"
"${INSTALL_DIR}/disksweep" --version || true
echo ""
echo "Usage:"
echo "  disksweep           # Interactive terminal dashboard"
echo "  disksweep drives    # List all drives & capacity meters"
echo "  disksweep clean     # Run cleanup checklist"
echo "  disksweep analyze   # Inspect large files and directories"
echo ""
