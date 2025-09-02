#!/bin/bash
# Build script for XtraScrapper Suite on Linux/Mac
# Builds all projects and publishes them as single-file executables

echo "Building XtraScrapper Suite..."
echo

# Create build directory
mkdir -p build

# Build XtraScrapper (ROM Organizer)
echo "Building XtraScrapper (ROM Organizer)..."
cd src/XtraScrapper
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true -o ../../build
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build XtraScrapper"
    exit 1
fi
cd ../..

# Build XtraImageScrapper (Image Downloader)
echo
echo "Building XtraImageScrapper (Image Downloader)..."
cd src/XtraImageScrapper
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true -o ../../build
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build XtraImageScrapper"
    exit 1
fi
cd ../..

# Build XtraRCleaner (ROM Cleaner)
echo
echo "Building XtraRCleaner (ROM Cleaner)..."
cd src/XtraRCleaner
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true -o ../../build
if [ $? -ne 0 ]; then
    echo "ERROR: Failed to build XtraRCleaner"
    exit 1
fi
cd ../..

echo
echo "✅ Build completed successfully!"
echo
echo "Built files in build/ directory:"
ls -la build/*.exe 2>/dev/null || echo "No .exe files found (expected on non-Windows)"

echo
echo "🎉 XtraScrapper Suite is ready!"