@echo off
cd /d "D:\Endstar Plot Flag\PLOT FLAG SYSTEM\EndstarPropInjector"
echo Starting build... > build_log.txt
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" EndstarPropInjector.csproj -p:Configuration=Release >> build_log.txt 2>&1
echo Exit code: %ERRORLEVEL% >> build_log.txt
echo. >> build_log.txt
echo DLL info: >> build_log.txt
dir bin\Release\*.dll >> build_log.txt 2>&1
