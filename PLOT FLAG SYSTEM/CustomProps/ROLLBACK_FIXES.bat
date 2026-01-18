@echo off
echo ========================================
echo  CustomProps Plugin - Rollback Fixes
echo ========================================
echo.

set "DIR=%~dp0"
cd /d "%DIR%"

echo Current directory: %CD%
echo.

:: Find the most recent backup folder
set "LATEST_BACKUP="
for /f "delims=" %%D in ('dir /b /ad /o-n "BACKUP_*" 2^>nul') do (
    if not defined LATEST_BACKUP set "LATEST_BACKUP=%%D"
)

if not defined LATEST_BACKUP (
    echo ERROR: No backup folder found!
    echo Run APPLY_FIXES.bat first to create backups.
    pause
    exit /b 1
)

echo Found backup folder: %LATEST_BACKUP%
echo.

:: Check if backup files exist
if not exist "%LATEST_BACKUP%\PropIntegration.cs" (
    echo WARNING: PropIntegration.cs not in backup
)
if not exist "%LATEST_BACKUP%\CustomPropsPlugin.cs" (
    echo WARNING: CustomPropsPlugin.cs not in backup
)
if not exist "%LATEST_BACKUP%\ProperPropInjector.cs" (
    echo WARNING: ProperPropInjector.cs not in backup
)

echo.
echo This will restore the original files from: %LATEST_BACKUP%
echo.
set /p CONFIRM="Are you sure you want to rollback? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo Rollback cancelled.
    pause
    exit /b 0
)

echo.
echo Restoring original files...
echo.

:: Restore files
if exist "%LATEST_BACKUP%\PropIntegration.cs" (
    echo Restoring PropIntegration.cs...
    copy /Y "%LATEST_BACKUP%\PropIntegration.cs" "PropIntegration.cs"
)

if exist "%LATEST_BACKUP%\CustomPropsPlugin.cs" (
    echo Restoring CustomPropsPlugin.cs...
    copy /Y "%LATEST_BACKUP%\CustomPropsPlugin.cs" "CustomPropsPlugin.cs"
)

if exist "%LATEST_BACKUP%\ProperPropInjector.cs" (
    echo Restoring ProperPropInjector.cs...
    copy /Y "%LATEST_BACKUP%\ProperPropInjector.cs" "ProperPropInjector.cs"
)

echo.
echo ========================================
echo  Rollback complete!
echo ========================================
echo.
echo Original files restored from: %LATEST_BACKUP%
echo Don't forget to rebuild the plugin.
echo.
pause
