# Squirrel Packaging Setup - Complete

## ✅ What Has Been Done

I've set up a complete Squirrel.Windows packaging solution for your TFT Sleep Tracker app. Here's what's been created:

### 📁 Files Created

1. **`/scripts/pack-squirrel.ps1`** - Full-featured packaging script with:
   - Automatic prerequisite checking
   - Build, test, publish, pack, and releasify steps
   - Pretty terminal output with progress indicators
   - Optional GitHub release upload
   - Error handling and validation

2. **`/scripts/pack-simple.ps1`** - Minimal script showing just the essential Squirrel commands

3. **`/scripts/build-installer.bat`** - Windows batch file wrapper for users who prefer double-clicking

4. **`/PACKAGING.md`** - Complete packaging documentation with:
   - Quick start guide
   - Prerequisites
   - Step-by-step instructions
   - Manual packaging steps
   - Distribution guide
   - Update workflow
   - Troubleshooting section

5. **`/PACKAGING_QUICKREF.md`** - Quick reference card with:
   - One-line commands
   - Common options
   - Manual commands
   - Troubleshooting table

### 🔧 Files Updated

1. **`/TFTSleepTracker.App/TFTSleepTracker.App.csproj`** - Added metadata:
   - Version information
   - Authors, Company, Product
   - Description and Copyright
   - Application icon reference

2. **`/.gitignore`** - Added patterns to ignore:
   - `/dist/` - Squirrel output directory
   - `/nupkgs/` - NuGet package directory
   - `*.nupkg`, `Setup.exe`, `RELEASES`
   - `**/publish/` directories

3. **`/README.md`** - Updated packaging section with links to new docs

## 🚀 How to Use (On Your Windows Machine)

### Method 1: PowerShell Script (Recommended)

```powershell
# Navigate to the scripts folder
cd scripts

# Create installer
.\pack-squirrel.ps1 -Version "1.0.0"

# The installer will be in: ../dist/Setup.exe
```

### Method 2: Batch File (Double-Click)

1. Open `scripts` folder in Windows Explorer
2. Double-click `build-installer.bat`
3. Enter version when prompted
4. Find installer in `dist\Setup.exe`

### Method 3: Simple Script

```powershell
cd scripts
.\pack-simple.ps1 -Version "1.0.0"
```

## 📋 Prerequisites (One-Time Setup)

Before first use, install:

```powershell
# Install Squirrel CLI
dotnet tool install --global Squirrel

# Verify installation
squirrel --version
```

That's it! The .NET 8 SDK you already have is sufficient.

## 📦 What You Get

After running the script, the `dist/` folder contains:

```
dist/
├── Setup.exe                              ← DISTRIBUTE THIS
├── RELEASES                               ← Required for updates
├── TFTSleepTracker-1.0.0-full.nupkg     ← Full package
└── (delta packages on subsequent builds)
```

**Give `Setup.exe` to your users!**

## 🔄 Update Workflow

When you want to release an update:

```powershell
# 1. Build new version
.\pack-squirrel.ps1 -Version "1.0.1"

# 2. Upload entire dist/ folder to your web server
#    (users' apps will auto-update from there)
```

Squirrel creates delta packages automatically - users only download the changes!

## 🎯 Script Features

### Full Script (`pack-squirrel.ps1`)

- ✅ Prerequisite checking (dotnet, squirrel)
- ✅ Clean build directories
- ✅ Restore NuGet packages
- ✅ Build solution in Release mode
- ✅ Run tests (optional: `-SkipTests`)
- ✅ Publish as self-contained exe
- ✅ Create NuGet package
- ✅ Generate installer with Squirrel
- ✅ Upload to GitHub (optional: `-Upload`)
- ✅ Beautiful progress output
- ✅ Error handling

### Options

```powershell
# Basic build
.\pack-squirrel.ps1 -Version "1.0.0"

# Skip tests (faster)
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests

# Auto-upload to GitHub
.\pack-squirrel.ps1 -Version "1.0.0" -Upload

# Both
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests -Upload
```

## 📖 Documentation

- **Full guide**: See `PACKAGING.md`
- **Quick reference**: See `PACKAGING_QUICKREF.md`
- **Script help**: `Get-Help .\pack-squirrel.ps1 -Detailed`

## ⚠️ Important Notes

1. **Windows Required**: Must run on Windows (WPF apps require Windows)
2. **Version Format**: Use semantic versioning (e.g., `1.0.0`, `1.2.3-beta`)
3. **First Install**: Squirrel installs to `%LocalAppData%\TFTSleepTracker`
4. **Auto-Updates**: Upload `dist/` contents to a web server for auto-updates

## 🐛 Troubleshooting

### "squirrel: command not found"

```powershell
dotnet tool install --global Squirrel
# Add to PATH: %USERPROFILE%\.dotnet\tools
```

### "dotnet: command not found"

Install .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

### Build Errors

- Make sure you're on Windows (WPF requirement)
- Check if all NuGet packages restored: `dotnet restore`
- Try running with administrator privileges

### Tests Fail

Use `-SkipTests` flag to build without running tests:

```powershell
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
```

## 🎓 Manual Commands (For Reference)

If you want to run steps manually:

```powershell
# 1. Publish
dotnet publish TFTSleepTracker.App/TFTSleepTracker.App.csproj `
    -c Release -r win-x64 `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=true `
    /p:SelfContained=true `
    /p:Version=1.0.0

# 2. Pack
squirrel pack `
    --framework net8.0-windows `
    --packId TFTSleepTracker `
    --packVersion 1.0.0 `
    --packDirectory TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish `
    --output nupkgs

# 3. Releasify
squirrel releasify `
    --package nupkgs/TFTSleepTracker.1.0.0.nupkg `
    --releaseDir dist
```

## 🆚 Comparison with Clowd.Squirrel

Your existing `pack.ps1` uses **Clowd.Squirrel** (a fork).  
The new `pack-squirrel.ps1` uses **official Squirrel CLI**.

Both work fine! Choose based on preference:

| Feature | Squirrel (official) | Clowd.Squirrel |
|---------|---------------------|----------------|
| Maturity | Original, stable | Fork, actively maintained |
| Command | `squirrel` | `squirrel` (different impl) |
| Install | `dotnet tool install --global Squirrel` | `dotnet tool install --global Clowd.Squirrel` |
| Docs | https://github.com/Squirrel/Squirrel.Windows | https://github.com/clowd/Clowd.Squirrel |

You can use either! Both scripts are included.

## ✨ Next Steps

1. **Test locally**: Run `.\pack-squirrel.ps1 -Version "0.9.0"`
2. **Test installer**: Run `dist\Setup.exe` on a clean Windows machine
3. **Set up updates**: Deploy `dist/` folder to a web server
4. **Configure app**: Point UpdateService to your update URL
5. **Go live**: Build version `1.0.0` and distribute!

## 🆘 Need Help?

- Check `PACKAGING.md` for detailed instructions
- Check `PACKAGING_QUICKREF.md` for quick commands
- See Squirrel docs: https://github.com/Squirrel/Squirrel.Windows
- Open an issue on GitHub

---

**You're all set!** 🎉

Run `.\scripts\pack-squirrel.ps1 -Version "1.0.0"` on your Windows machine and you'll have a distributable installer.
