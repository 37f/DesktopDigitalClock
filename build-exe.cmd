@echo off
setlocal
pushd "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: .NET SDK was not found.
    echo Install .NET 6 SDK or a newer SDK, then run this file again.
    pause
    exit /b 1
)

dotnet publish "DesktopDigitalClock.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o "publish\win-x64"
if errorlevel 1 (
    echo.
    echo Build failed.
    pause
    exit /b 1
)

echo.
echo Build complete:
echo %~dp0publish\win-x64\DesktopDigitalClock.exe
pause
