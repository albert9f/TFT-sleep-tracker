# Squirrel Packaging - Quick Reference

## One-Line Command

```powershell
cd scripts && .\pack-squirrel.ps1 -Version "1.0.0"
```

## What You Get

- `dist/Setup.exe` ← **Give this to users**
- `dist/RELEASES` ← Required for auto-updates
- `dist/*.nupkg` ← Update packages

## First Time Setup

```powershell
# 1. Install Squirrel CLI (one-time)
dotnet tool install --global Squirrel

# 2. Create a release
cd scripts
.\pack-squirrel.ps1 -Version "1.0.0"

# 3. Distribute Setup.exe
```

## Update Workflow

```powershell
# 1. Increment version
.\pack-squirrel.ps1 -Version "1.0.1"

# 2. Upload everything in dist/ folder to your update server
#    (including RELEASES file)

# 3. Users get automatic delta updates
```

## Script Options

```powershell
# Full build with tests
.\pack-squirrel.ps1 -Version "1.0.0"

# Skip tests (faster)
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests

# Auto-upload to GitHub
.\pack-squirrel.ps1 -Version "1.0.0" -Upload

# Minimal script (just the essentials)
.\pack-simple.ps1 -Version "1.0.0"
```

## Manual Commands (if needed)

```powershell
# Step 1: Publish
dotnet publish TFTSleepTracker.App/TFTSleepTracker.App.csproj `
    -c Release -r win-x64 `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=true `
    /p:SelfContained=true

# Step 2: Pack
squirrel pack `
    --framework net8.0-windows `
    --packId TFTSleepTracker `
    --packVersion 1.0.0 `
    --packDirectory TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish `
    --output nupkgs

# Step 3: Releasify
squirrel releasify `
    --package nupkgs/TFTSleepTracker.1.0.0.nupkg `
    --releaseDir dist
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| "squirrel: command not found" | Run: `dotnet tool install --global Squirrel` |
| Can't find .NET | Install .NET 8 SDK from https://dotnet.microsoft.com/download |
| Build fails | Run on Windows machine (WPF requires Windows) |
| Path too long | Move repo closer to root (e.g., `C:\code\TFT`) |

## Version Format

- `1.0.0` - Production release
- `1.0.1` - Bug fix
- `1.1.0` - New feature
- `2.0.0` - Breaking change
- `1.0.0-beta` - Pre-release

## Where Files Go

After installation, the app lives in:
```
%LocalAppData%\TFTSleepTracker\
```

## Auto-Updates

To enable auto-updates in your app:

```csharp
// Check for updates periodically
using Squirrel;

var updateUrl = "https://your-server.com/releases/";
using var mgr = new UpdateManager(updateUrl);
var updates = await mgr.CheckForUpdate();

if (updates.ReleasesToApply.Any())
{
    await mgr.UpdateApp();
    // Restart to apply
}
```

Upload your `dist/` folder contents to that URL.

---

**See PACKAGING.md for detailed documentation**
