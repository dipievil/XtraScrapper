@echo off
REM Build script for XtraScrapper Suite
REM Builds all projects and publishes them as single-file executables

echo Building XtraScrapper Suite...
echo.

REM Create build directory
if not exist "build" mkdir build

REM Build XtraScrapper (ROM Organizer)
echo Building XtraScrapper (ROM Organizer)...
cd src\XtraScrapper
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true -o ..\..\build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build XtraScrapper
    pause
    exit /b 1
)
cd ..\..

REM Build XtraImageScrapper (Image Downloader)
echo.
echo Building XtraImageScrapper (Image Downloader)...
cd src\XtraImageScrapper
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true -o ..\..\build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build XtraImageScrapper
    pause
    exit /b 1
)
cd ..\..

REM Build XtraRCleaner (ROM Cleaner)
echo.
echo Building XtraRCleaner (ROM Cleaner)...
cd src\XtraRCleaner
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true -o ..\..\build
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build XtraRCleaner
    pause
    exit /b 1
)
cd ..\..

echo.
echo ✅ Build completed successfully!
echo.
echo Built files in build\ directory:
dir build\*.exe /b

echo.
echo 🎉 XtraScrapper Suite is ready!
pause