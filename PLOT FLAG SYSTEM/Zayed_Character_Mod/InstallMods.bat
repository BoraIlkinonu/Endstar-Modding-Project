@echo off
echo ========================================
echo  Endstar Mod Installer
echo ========================================
echo.

REM Use exe if available, otherwise fall back to Python
if exist "%~dp0EndstarModInstaller.exe" (
    "%~dp0EndstarModInstaller.exe"
) else (
    python "%~dp0EndstarModInstaller.py"
)
pause
