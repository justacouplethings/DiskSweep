@echo off
echo ============================================================
echo   Pushing DiskSweep to GitHub (origin/main)
echo ============================================================
git push -u origin main
if %ERRORLEVEL% EQU 0 (
    echo.
    echo Successfully pushed DiskSweep to GitHub!
) else (
    echo.
    echo Push failed. If prompted, please authenticate with your GitHub credentials or Personal Access Token.
)
pause
