@echo off
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "%~dp0EndstarPropInjector.csproj" -p:Configuration=Release
echo Build completed with exit code: %ERRORLEVEL%
dir "%~dp0bin\Release\*.dll"
