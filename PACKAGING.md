# TFT Sleep Tracker - Windows Installer Packaging Guide

This guide explains how to create a Windows installer for TFT Sleep Tracker using Squirrel.

## Quick Start

### Prerequisites

1. **Install .NET 8 SDK** (if not already installed)
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **Install Squirrel CLI**
   ```powershell
   dotnet tool install --global Squirrel
   ```

3. **(Optional) Install GitHub CLI** for automated releases
   - Download from: https://cli.github.com/
   - Authenticate: `gh auth login`

### Create a Release Package

Run the packaging script from the `scripts` directory:

```powershell
cd scripts
.\pack-squirrel.ps1 -Version "1.0.0"
```

The script will:
1. ✓ Clean previous builds
2. ✓ Restore NuGet packages
3. ✓ Build the solution in Release mode
4. ✓ Run tests
5. ✓ Publish as a self-contained executable
6. ✓ Create a NuGet package
7. ✓ Generate the installer with Squirrel

### Output

After successful packaging, you'll find these files in the `dist/` folder:

- **`Setup.exe`** - The installer to distribute to users
- **`RELEASES`** - Manifest file for auto-updates (required)
- **`TFTSleepTracker-{version}-full.nupkg`** - Full update package
- **`TFTSleepTracker-{version}-delta.nupkg`** - Delta update package (for subsequent releases)

## Advanced Options

### Skip Tests

If you need to package quickly without running tests:

```powershell
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
```

### Upload to GitHub Releases

Automatically create a GitHub release and upload the installer:

```powershell
.\pack-squirrel.ps1 -Version "1.0.0" -Upload
```

This requires the GitHub CLI (`gh`) to be installed and authenticated.

## Manual Packaging Steps

If you prefer to run the steps manually:

### 1. Publish the Application

```powershell
dotnet publish TFTSleepTracker.App/TFTSleepTracker.App.csproj `
    -c Release `
    -r win-x64 `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=true `
    /p:SelfContained=true `
    /p:Version=1.0.0
```

### 2. Create NuGet Package

```powershell
squirrel pack `
    --framework net8.0-windows `
    --packId TFTSleepTracker `
    --packVersion 1.0.0 `
    --packDirectory TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish `
    --output nupkgs
```

### 3. Create Installer

```powershell
squirrel releasify `
    --package nupkgs/TFTSleepTracker.1.0.0.nupkg `
    --releaseDir dist
```

## Distribution

### First-Time Installation

1. Share `Setup.exe` with your users (via email, download link, etc.)
2. Users run `Setup.exe` to install the application
3. Squirrel installs to the user's AppData folder: `%LocalAppData%\TFTSleepTracker`
4. Desktop and Start Menu shortcuts are created automatically

### Updates

Squirrel provides automatic update functionality:

1. Build a new version with an incremented version number
2. Package it using the same process
3. Upload the contents of `dist/` folder (including `RELEASES` file) to a web server
4. Configure your app to check for updates at that URL
5. Squirrel will download and apply delta updates automatically

The app can check for updates using code like:

```csharp
using Squirrel;

var updateUrl = "https://your-server.com/releases/";
using var updateManager = new UpdateManager(updateUrl);
var updateInfo = await updateManager.CheckForUpdate();

if (updateInfo.ReleasesToApply.Any())
{
    await updateManager.UpdateApp();
    // Restart the app to apply updates
}
```

## Troubleshooting

### "Squirrel command not found"

Make sure the .NET global tools directory is in your PATH:
- Windows: `%USERPROFILE%\.dotnet\tools`

Reinstall Squirrel CLI:
```powershell
dotnet tool uninstall --global Squirrel
dotnet tool install --global Squirrel
```

### "dotnet: command not found"

Install the .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

### Build Errors

Ensure you're running from a Windows machine (or a Windows VM) as this is a WPF application.

### Large Installer Size

The self-contained publish includes the entire .NET runtime (~80-100 MB). To reduce size:
- Remove `--self-contained true` (requires users to install .NET 8)
- Or use framework-dependent deployment

## Version Numbering

Follow semantic versioning (MAJOR.MINOR.PATCH):
- **MAJOR**: Breaking changes
- **MINOR**: New features, backwards compatible
- **PATCH**: Bug fixes

Examples:
- `1.0.0` - Initial release
- `1.1.0` - New feature added
- `1.1.1` - Bug fix
- `2.0.0` - Major update with breaking changes

Pre-release versions:
- `1.0.0-beta`
- `1.0.0-rc1`

## Resources

- **Squirrel Documentation**: https://github.com/Squirrel/Squirrel.Windows
- **Squirrel CLI**: https://github.com/clowd/Clowd.Squirrel
- **.NET Publishing**: https://learn.microsoft.com/en-us/dotnet/core/deploying/

## Support

For issues with packaging, check:
1. Build logs in the console output
2. Test results if tests are failing
3. Squirrel documentation for specific errors

For application issues, refer to the main README.md.
