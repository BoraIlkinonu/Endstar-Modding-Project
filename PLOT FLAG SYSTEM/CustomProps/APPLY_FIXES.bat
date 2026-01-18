@echo off
echo ========================================
echo  CustomProps Plugin - Apply Fixes
echo ========================================
echo.

set "DIR=%~dp0"
cd /d "%DIR%"

echo Current directory: %CD%
echo.

:: Check if fixed files exist
if not exist "PropIntegration_FIXED.cs" (
    echo ERROR: PropIntegration_FIXED.cs not found!
    pause
    exit /b 1
)
if not exist "CustomPropsPlugin_FIXED.cs" (
    echo ERROR: CustomPropsPlugin_FIXED.cs not found!
    pause
    exit /b 1
)
if not exist "ProperPropInjector_FIXED.cs" (
    echo ERROR: ProperPropInjector_FIXED.cs not found!
    pause
    exit /b 1
)

echo Fixed files found. Proceeding...
echo.

:: Create backup folder with timestamp
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set "BACKUP_DIR=BACKUP_%datetime:~0,8%_%datetime:~8,6%"
mkdir "%BACKUP_DIR%" 2>nul

echo Creating backups in: %BACKUP_DIR%
echo.

:: Backup original files (if they exist)
if exist "PropIntegration.cs" (
    echo Backing up PropIntegration.cs...
    copy /Y "PropIntegration.cs" "%BACKUP_DIR%\PropIntegration.cs"
)

if exist "CustomPropsPlugin.cs" (
    echo Backing up CustomPropsPlugin.cs...
    copy /Y "CustomPropsPlugin.cs" "%BACKUP_DIR%\CustomPropsPlugin.cs"
)

if exist "ProperPropInjector.cs" (
    echo Backing up ProperPropInjector.cs...
    copy /Y "ProperPropInjector.cs" "%BACKUP_DIR%\ProperPropInjector.cs"
)

echo.
echo Backups complete.
echo.

:: Apply fixes
echo Applying fixed versions...
echo.

echo Replacing PropIntegration.cs...
copy /Y "PropIntegration_FIXED.cs" "PropIntegration.cs"
if errorlevel 1 (
    echo ERROR: Failed to replace PropIntegration.cs
    pause
    exit /b 1
)

echo Replacing CustomPropsPlugin.cs...
copy /Y "CustomPropsPlugin_FIXED.cs" "CustomPropsPlugin.cs"
if errorlevel 1 (
    echo ERROR: Failed to replace CustomPropsPlugin.cs
    pause
    exit /b 1
)

echo Replacing ProperPropInjector.cs...
copy /Y "ProperPropInjector_FIXED.cs" "ProperPropInjector.cs"
if errorlevel 1 (
    echo ERROR: Failed to replace ProperPropInjector.cs
    pause
    exit /b 1
)

echo.
echo ========================================
echo  SUCCESS! All fixes applied.
echo ========================================
echo.
echo Backups saved to: %BACKUP_DIR%
echo.
echo Next steps:
echo   1. Rebuild the plugin (dotnet build or Visual Studio)
echo   2. Copy the new DLL to BepInEx/plugins
echo   3. Launch Endstar and test the prop tool
echo.
pause
