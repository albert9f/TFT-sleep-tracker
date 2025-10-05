# Manual Squirrel Release Guide

Since you're in a Linux environment but need to create a Windows Squirrel release, here's how to do it manually:

## Prerequisites

You need access to a **Windows machine** with:
- .NET 8 SDK installed
- Squirrel CLI installed: `dotnet tool install --global Squirrel`

## Step-by-Step Instructions

### 1. Clone/Pull Latest Code on Windows

```powershell
git clone https://github.com/albert9f/TFT-sleep-tracker.git
cd TFT-sleep-tracker
```

Or if already cloned:
```powershell
cd TFT-sleep-tracker
git pull origin main
```

### 2. Verify Version Number

Open `TFTSleepTracker.App/TFTSleepTracker.App.csproj` and confirm:
```xml
<Version>1.0.1</Version>
```

### 3. Run the Packaging Script

```powershell
.\scripts\pack-squirrel.ps1 -Version "1.0.1"
```

This will:
- Build the solution
- Run tests
- Create the NuGet package
- Generate the Squirrel installer

### 4. Review Output

Check the `dist/` folder for:
```
dist/
├── Setup.exe                           ← Main installer
├── RELEASES                            ← Update manifest
├── TFTSleepTracker-1.0.1-full.nupkg   ← Full package
└── TFTSleepTracker-1.0.1-delta.nupkg  ← Delta update (if from 1.0.0)
```

### 5. Upload to GitHub

#### Option A: Automatic Upload (Recommended)

```powershell
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -Upload
```

This automatically creates a GitHub release and uploads all files.

#### Option B: Manual Upload via Web

1. Go to https://github.com/albert9f/TFT-sleep-tracker/releases
2. Click "Draft a new release"
3. Set tag: `v1.0.1`
4. Set title: `TFT Sleep Tracker v1.0.1`
5. Add release notes
6. Upload ALL files from `dist/` folder:
   - Setup.exe
   - RELEASES
   - TFTSleepTracker-1.0.1-full.nupkg
   - TFTSleepTracker-1.0.1-delta.nupkg (if exists)
7. Publish release

#### Option C: Upload via GitHub CLI

```powershell
# Create release
gh release create v1.0.1 `
  --title "TFT Sleep Tracker v1.0.1" `
  --notes "Auto-update enabled release for version 1.0.1" `
  dist/*
```

### 6. Verify Auto-Update Will Work

After uploading, verify the release contains:
- ✅ `Setup.exe` - For new users
- ✅ `RELEASES` - **Critical for auto-updates**
- ✅ `*.nupkg` files - The actual update packages

Your app's `UpdateService.cs` points to:
```
https://github.com/albert9f/TFT-sleep-tracker
```

With the RELEASES file uploaded, users on 1.0.0 will:
1. Check for updates (every 7 days automatically)
2. Find version 1.0.1 in the RELEASES file
3. Download the delta or full package
4. Apply update on next restart

## Important Notes

### ⚠️ DO NOT Mix Release Types

- **Never mix** single .exe files with Squirrel packages in the same release
- Either use `.exe` files (no auto-update) OR Squirrel packages (with auto-update)
- Your app expects Squirrel packages, so **only upload Squirrel packages**

### ⚠️ RELEASES File is Critical

- The `RELEASES` file is a **manifest** that tells your app what updates are available
- Without it, auto-updates **will not work**
- Always upload it with your packages

### ⚠️ Keep All Versions

- Don't delete old releases (e.g., keep 1.0.0)
- Squirrel needs old versions to calculate deltas
- Users might be on any version and need to update

## Testing the Release

### Test on a Clean Machine

1. **Uninstall** any existing version of TFT Sleep Tracker
2. **Download** Setup.exe from the v1.0.1 release
3. **Run** Setup.exe - it should install to `%LocalAppData%\TFTSleepTracker`
4. **Launch** the app - verify version is 1.0.1 in title bar or About dialog

### Test Auto-Update (If Users on 1.0.0)

1. On a machine with 1.0.0 installed
2. Wait for the 7-day check, OR
3. Manually trigger update check in the app (if you have a button)
4. App should download 1.0.1 in background
5. Restart app - should update to 1.0.1

## Troubleshooting

### "Squirrel not found"

```powershell
dotnet tool install --global Squirrel
```

Add to PATH: `%USERPROFILE%\.dotnet\tools`

### "WPF not supported"

You must run this on Windows. WPF apps cannot be built on Linux/Mac.

### Build Errors

```powershell
# Clean and rebuild
dotnet clean
dotnet restore
.\scripts\pack-squirrel.ps1 -Version "1.0.1"
```

### Tests Fail

```powershell
# Skip tests
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -SkipTests
```

## Quick Commands Cheat Sheet

```powershell
# Full build with tests
.\scripts\pack-squirrel.ps1 -Version "1.0.1"

# Build without tests
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -SkipTests

# Build and auto-upload to GitHub
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -Upload

# Build, skip tests, and upload
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -SkipTests -Upload
```

## What Happens After Upload?

Once v1.0.1 is properly released with Squirrel packages:

1. **New Users**: Download Setup.exe and install
2. **Existing Users (1.0.0)**: 
   - App checks for updates every 7 days
   - Finds 1.0.1 in RELEASES file
   - Downloads delta package (~few KB, just the changes)
   - Updates on next restart
   - **No manual action needed!** 🎉

## Next Release (1.0.2)

When you want to release 1.0.2:

```powershell
# Update version in .csproj
# <Version>1.0.2</Version>

# Package
.\scripts\pack-squirrel.ps1 -Version "1.0.2" -Upload
```

Squirrel automatically:
- Creates delta from 1.0.1 → 1.0.2
- Creates delta from 1.0.0 → 1.0.2
- Updates RELEASES file
- Users on any version can update!

---

**Need Help?** Open an issue on GitHub or check the main PACKAGING.md documentation.
