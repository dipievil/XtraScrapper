@echo off
REM Build script for XtraScrapper Suite

echo 🔨 Building XtraScrapper Suite...

REM Create build directory
if not exist "build" mkdir build

REM Build XtraScrapper
echo 📦 Building XtraScrapper...
cd src\XtraScrapper
dotnet publish -c Release -o ..\..\build\XtraScrapper
cd ..\..

REM Build XtraImageScrapper
echo 🖼️ Building XtraImageScrapper...
cd src\XtraImageScrapper
dotnet publish -c Release -o ..\..\build\XtraImageScrapper
cd ..\..

REM Build XtraRCleaner
echo 🧹 Building XtraRCleaner...
cd src\XtraRCleaner
dotnet publish -c Release -o ..\..\build\XtraRCleaner
cd ..\..

echo ✅ Build completed! Check the build/ folder.
echo.
echo 📁 Build output:
echo   - build\XtraScrapper\XtraScrapper.exe
echo   - build\XtraImageScrapper\XtraImageScrapper.exe
echo   - build\XtraRCleaner\XtraRCleaner.exe

pause