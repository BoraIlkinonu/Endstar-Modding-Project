@echo off
echo ========================================
echo  Endstar Mod Patcher - Uninstall
echo ========================================
echo.

python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Python not found!
    pause
    exit /b 1
)

echo Restoring original game files...
python "%~dp0EndstarModPatcher.py" uninstall

echo.
echo Original files restored!
pause
