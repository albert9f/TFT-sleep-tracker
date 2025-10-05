# Squirrel Packaging - Troubleshooting Guide

Common issues and solutions when building Windows installers with Squirrel.

## Table of Contents

- [Installation Issues](#installation-issues)
- [Build Issues](#build-issues)
- [Installer Issues](#installer-issues)
- [Runtime Issues](#runtime-issues)
- [Update Issues](#update-issues)

---

## Installation Issues

### "squirrel: command not found" or "The term 'squirrel' is not recognized"

**Cause**: Squirrel CLI is not installed or not in PATH.

**Solutions**:

1. Install Squirrel CLI:
   ```powershell
   dotnet tool install --global Squirrel
   ```

2. If already installed, update it:
   ```powershell
   dotnet tool update --global Squirrel
   ```

3. Add .NET tools to PATH:
   - Windows: Add `%USERPROFILE%\.dotnet\tools` to your PATH environment variable
   - Restart PowerShell after changing PATH

4. Verify installation:
   ```powershell
   squirrel --version
   ```

---

### "dotnet: command not found"

**Cause**: .NET SDK is not installed.

**Solution**: Install .NET 8 SDK from https://dotnet.microsoft.com/download/dotnet/8.0

After installation, verify:
```powershell
dotnet --version
```

Should show `8.0.x` or higher.

---

### "pwsh: command not found" (when using .bat file)

**Cause**: PowerShell Core not installed (optional, Windows PowerShell will be used).

**Solution**: This is fine - the batch file will fall back to Windows PowerShell. Or install PowerShell Core from https://github.com/PowerShell/PowerShell/releases

---

## Build Issues

### "Cannot find path... because it does not exist"

**Cause**: Wrong directory or repository not cloned properly.

**Solution**:

1. Ensure you're in the correct directory:
   ```powershell
   cd /path/to/TFT-sleep-tracker/scripts
   ```

2. Verify files exist:
   ```powershell
   ls
   ```
   You should see `pack-squirrel.ps1`

---

### "Tests failed"

**Cause**: Unit tests are failing.

**Solutions**:

1. **Skip tests temporarily**:
   ```powershell
   .\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
   ```

2. **Run tests manually to see details**:
   ```powershell
   cd ..
   dotnet test --verbosity detailed
   ```

3. **Fix failing tests** based on the output.

---

### "dotnet restore failed"

**Cause**: Network issues or corrupted NuGet cache.

**Solutions**:

1. Clear NuGet cache:
   ```powershell
   dotnet nuget locals all --clear
   ```

2. Retry restore:
   ```powershell
   dotnet restore
   ```

3. Check internet connection and proxy settings.

---

### "dotnet build failed"

**Cause**: Compilation errors in code.

**Solutions**:

1. Run build manually with verbose output:
   ```powershell
   dotnet build --configuration Release --verbosity detailed
   ```

2. Check error messages for syntax errors or missing dependencies.

3. Ensure all project references are correct.

4. Clean and rebuild:
   ```powershell
   dotnet clean
   dotnet build
   ```

---

### "dotnet publish failed"

**Cause**: Various reasons - missing runtime, platform mismatch, etc.

**Solutions**:

1. Verify you're on Windows (WPF apps require Windows).

2. Check disk space (need ~500 MB free).

3. Try publishing manually:
   ```powershell
   dotnet publish TFTSleepTracker.App/TFTSleepTracker.App.csproj `
       -c Release -r win-x64 --self-contained
   ```

4. Check for error messages about missing SDKs or workloads.

---

### "squirrel pack failed"

**Cause**: Issues with published output or Squirrel CLI version.

**Solutions**:

1. Verify published output exists:
   ```powershell
   ls TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish
   ```
   Should contain `.exe` files.

2. Try manual pack command:
   ```powershell
   squirrel pack --help
   ```
   Read help to ensure correct syntax.

3. Update Squirrel CLI:
   ```powershell
   dotnet tool update --global Squirrel
   ```

---

### "squirrel releasify failed"

**Cause**: NuGet package is malformed or missing.

**Solutions**:

1. Verify .nupkg file exists:
   ```powershell
   ls nupkgs/*.nupkg
   ```

2. Check file size (should be ~85-105 MB for self-contained app).

3. Try extracting .nupkg (it's a ZIP file) to inspect contents:
   ```powershell
   Expand-Archive nupkgs/TFTSleepTracker.1.0.0.nupkg -DestinationPath temp
   ls temp
   ```

4. Rebuild from scratch:
   ```powershell
   Remove-Item dist, nupkgs -Recurse -Force
   .\pack-squirrel.ps1 -Version "1.0.0"
   ```

---

### "PathTooLongException"

**Cause**: Windows path length limit (260 characters).

**Solutions**:

1. **Move repository closer to root**:
   ```powershell
   # Instead of:
   C:\Users\YourName\Documents\Projects\MyCompany\TFT-sleep-tracker\
   
   # Use:
   C:\code\TFT\
   ```

2. **Enable long paths in Windows** (Windows 10 1607+):
   - Run as admin:
   ```powershell
   New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" `
       -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
   ```
   - Restart computer

---

## Installer Issues

### Setup.exe won't run / "Windows protected your PC"

**Cause**: SmartScreen warning for unsigned executables.

**Solutions**:

1. Click "More info" → "Run anyway"

2. **Code sign your executable** (production):
   - Purchase code signing certificate
   - Sign Setup.exe: https://github.com/Squirrel/Squirrel.Windows/blob/master/docs/using/code-signing.md

3. For testing, disable SmartScreen temporarily (not recommended for production).

---

### "The application failed to initialize properly"

**Cause**: Missing .NET runtime (if not self-contained) or corrupted install.

**Solutions**:

1. Verify self-contained build:
   ```powershell
   # In pack-squirrel.ps1, check publish command has:
   --self-contained true
   ```

2. Install .NET Desktop Runtime:
   https://dotnet.microsoft.com/download/dotnet/8.0/runtime

3. Rebuild installer:
   ```powershell
   Remove-Item dist -Recurse -Force
   .\pack-squirrel.ps1 -Version "1.0.0"
   ```

---

### Installer runs but app doesn't start

**Cause**: Installation succeeded but executable has issues.

**Solutions**:

1. Check installation location:
   ```
   %LocalAppData%\TFTSleepTracker\
   ```

2. Run executable directly to see error:
   ```powershell
   %LocalAppData%\TFTSleepTracker\app-1.0.0\TFTSleepTracker.exe
   ```

3. Check Windows Event Viewer:
   - Open Event Viewer
   - Windows Logs → Application
   - Look for .NET Runtime errors

4. Verify dependencies included in publish folder.

---

### "Access denied" during installation

**Cause**: Insufficient permissions (rare - Squirrel uses user profile).

**Solutions**:

1. Check if antivirus is blocking.

2. Verify `%LocalAppData%` is writable:
   ```powershell
   New-Item -ItemType File -Path "$env:LOCALAPPDATA\test.txt"
   Remove-Item "$env:LOCALAPPDATA\test.txt"
   ```

3. Run as administrator (usually not necessary).

---

## Runtime Issues

### App crashes on startup

**Cause**: Various - missing dependencies, configuration issues, etc.

**Solutions**:

1. Check app logs (if implemented).

2. Run from command line to see errors:
   ```powershell
   %LocalAppData%\TFTSleepTracker\app-1.0.0\TFTSleepTracker.exe
   ```

3. Check Event Viewer for .NET exceptions.

4. Verify all dependencies are copied during publish:
   ```powershell
   ls TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish
   ```

---

### System tray icon doesn't appear

**Cause**: WPF system tray issues or Windows Forms not included.

**Solution**: Verify project has:
```xml
<UseWindowsForms>true</UseWindowsForms>
```

---

### Autostart doesn't work

**Cause**: Registry entry not created or Windows startup disabled.

**Solutions**:

1. Check registry manually:
   ```
   HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
   ```
   Should have entry for TFTSleepTracker.

2. Check Task Manager → Startup tab.

3. Verify `AutostartHelper.cs` is being called on first run.

---

## Update Issues

### Updates not detected

**Cause**: Update URL not configured or unreachable.

**Solutions**:

1. Verify update URL in code.

2. Test URL manually:
   ```powershell
   curl https://your-server.com/releases/RELEASES
   ```
   Should return Squirrel RELEASES file content.

3. Check app has internet access (firewall).

---

### Update downloads but doesn't apply

**Cause**: Update manager issues or permissions.

**Solutions**:

1. Check update logs (if implemented).

2. Verify `Update.exe` exists in app folder.

3. Ensure app restarts after update download.

4. Check Windows Event Viewer for errors.

---

### Delta updates failing

**Cause**: Corrupted delta package or mismatch.

**Solutions**:

1. Ensure RELEASES file is updated on server.

2. Upload both full and delta packages.

3. Clear local package cache:
   ```
   %LocalAppData%\TFTSleepTracker\packages\
   ```

4. Force full update instead of delta.

---

## General Troubleshooting Steps

### Nuclear Option: Complete Clean Build

If nothing works, try a complete clean build:

```powershell
# 1. Clean everything
Remove-Item bin, obj, dist, nupkgs, releases -Recurse -Force -ErrorAction SilentlyContinue
dotnet clean

# 2. Clear NuGet cache
dotnet nuget locals all --clear

# 3. Restore from scratch
dotnet restore

# 4. Build fresh
dotnet build -c Release

# 5. Run tests
dotnet test

# 6. Package
cd scripts
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
```

---

## Getting Help

### Still stuck?

1. **Read the documentation**:
   - `PACKAGING.md` - Full guide
   - `PACKAGING_QUICKREF.md` - Quick commands
   - `PACKAGING_WORKFLOW.md` - Visual workflow

2. **Check Squirrel documentation**:
   - https://github.com/Squirrel/Squirrel.Windows

3. **Search for errors**:
   - Copy exact error message
   - Search GitHub issues: https://github.com/Squirrel/Squirrel.Windows/issues
   - Search Stack Overflow

4. **Enable verbose logging**:
   - Add `--verbosity detailed` to dotnet commands
   - Check Squirrel CLI output carefully

5. **Create a minimal reproduction**:
   - Create a new simple WPF project
   - Try packaging that
   - If it works, compare with your project

---

## Prevention Tips

### Avoid common issues:

✅ **Use version control** - Commit before packaging  
✅ **Test locally first** - Don't distribute untested installers  
✅ **Keep paths short** - Avoid deeply nested directories  
✅ **Run on clean VM** - Test installation on fresh Windows  
✅ **Sign your code** - Get code signing certificate for production  
✅ **Document versions** - Keep CHANGELOG.md updated  
✅ **Automate builds** - Use CI/CD to catch issues early  
✅ **Test updates** - Always test update path, not just fresh install  

---

## Quick Diagnostics

Run this diagnostic script if you're having issues:

```powershell
Write-Host "=== Diagnostic Info ===" -ForegroundColor Cyan

Write-Host "`n1. .NET SDK:"
dotnet --version

Write-Host "`n2. Squirrel CLI:"
squirrel --version 2>&1

Write-Host "`n3. PowerShell:"
$PSVersionTable.PSVersion

Write-Host "`n4. Current Directory:"
Get-Location

Write-Host "`n5. Disk Space:"
Get-PSDrive C | Select-Object Used,Free

Write-Host "`n6. Path Length:"
(Get-Location).Path.Length

Write-Host "`n7. Files Present:"
Test-Path .\pack-squirrel.ps1
Test-Path ..\TFTSleepTracker.App\TFTSleepTracker.App.csproj

Write-Host "`n8. .NET Global Tools Path:"
$env:PATH -split ';' | Where-Object { $_ -like '*\.dotnet\tools*' }

Write-Host "`n=== End Diagnostics ===" -ForegroundColor Cyan
```

Save as `diagnose.ps1` and run from `scripts/` folder.

---

**Still having issues? Open an issue on GitHub with:**
- Full error message
- Output from diagnostic script above
- Steps to reproduce
- Your environment (Windows version, .NET version)
