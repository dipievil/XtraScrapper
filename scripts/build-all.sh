#!/bin/bash
# Build script for XtraScrapper Suite

echo "🔨 Building XtraScrapper Suite..."

# Create build directory
mkdir -p ../../build

# Build XtraScrapper
echo "📦 Building XtraScrapper..."
cd src/XtraScrapper
dotnet publish -c Release -o ../../build/XtraScrapper
cd ../..

# Build XtraImageScrapper  
echo "🖼️ Building XtraImageScrapper..."
cd src/XtraImageScrapper
dotnet publish -c Release -o ../../build/XtraImageScrapper
cd ../..

# Build XtraRCleaner
echo "🧹 Building XtraRCleaner..."
cd src/XtraRCleaner
dotnet publish -c Release -o ../../build/XtraRCleaner
cd ../..

echo "✅ Build completed! Check the build/ folder."
echo ""
echo "📁 Build output:"
echo "  - build/XtraScrapper/XtraScrapper.exe"
echo "  - build/XtraImageScrapper/XtraImageScrapper.exe" 
echo "  - build/XtraRCleaner/XtraRCleaner.exe"