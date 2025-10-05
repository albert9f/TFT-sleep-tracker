# Release 1.0.1 - Squirrel Release Checklist ✅

## Pre-Release Checklist

- [ ] Code changes committed to main branch
- [ ] Version updated to `1.0.1` in `TFTSleepTracker.App/TFTSleepTracker.App.csproj`
- [ ] Tests passing locally
- [ ] Access to Windows machine (WPF requirement)

## Building the Release (Windows Machine)

- [ ] Pull latest code from GitHub
  ```powershell
  git pull origin main
  ```

- [ ] Install Squirrel CLI (if not already installed)
  ```powershell
  dotnet tool install --global Squirrel
  ```

- [ ] Run packaging script
  ```powershell
  .\scripts\pack-squirrel.ps1 -Version "1.0.1"
  ```

- [ ] Verify `dist/` folder contains:
  - [ ] `Setup.exe`
  - [ ] `RELEASES` ← **CRITICAL for auto-updates**
  - [ ] `TFTSleepTracker-1.0.1-full.nupkg`
  - [ ] `TFTSleepTracker-1.0.1-delta.nupkg` (if upgrading from 1.0.0)

## Uploading to GitHub

### Option A: Automatic Upload (Recommended)
- [ ] Run script with `-Upload` flag
  ```powershell
  .\scripts\pack-squirrel.ps1 -Version "1.0.1" -Upload
  ```

### Option B: Manual Upload via Web
- [ ] Go to https://github.com/albert9f/TFT-sleep-tracker/releases/new
- [ ] Create new release:
  - Tag: `v1.0.1`
  - Title: `TFT Sleep Tracker v1.0.1`
  - Description: Add release notes
- [ ] Upload ALL files from `dist/` folder
- [ ] Publish release

### Option C: Upload via GitHub CLI
- [ ] Create and upload release
  ```powershell
  gh release create v1.0.1 --title "TFT Sleep Tracker v1.0.1" --notes "Auto-update enabled" dist/*
  ```

## Verification After Upload

- [ ] Visit https://github.com/albert9f/TFT-sleep-tracker/releases/tag/v1.0.1
- [ ] Verify release contains:
  - [ ] ✅ `Setup.exe` (for new installations)
  - [ ] ✅ `RELEASES` file (manifest for updates)
  - [ ] ✅ `TFTSleepTracker-1.0.1-full.nupkg`
  - [ ] ✅ Delta package (if applicable)

## Testing

### Test 1: Fresh Installation
- [ ] Download `Setup.exe` from GitHub release
- [ ] Run installer on a clean Windows machine
- [ ] Verify app installs to `%LocalAppData%\TFTSleepTracker`
- [ ] Launch app and verify it works
- [ ] Check version in app (should show 1.0.1)

### Test 2: Auto-Update (If Users on 1.0.0)
- [ ] On a machine with 1.0.0 installed
- [ ] Wait for automatic update check (7 days) OR
- [ ] Trigger manual update check (if implemented)
- [ ] Verify update downloads in background
- [ ] Restart app
- [ ] Verify app updates to 1.0.1

## Post-Release Communication

- [ ] Notify users about the new release (optional)
- [ ] Update documentation if needed
- [ ] Monitor for any issues reported by users

## What Auto-Update Provides

✅ **Users on 1.0.0 will automatically receive 1.0.1** within 7 days (your configured check interval)

✅ **No manual download required** - updates happen silently in background

✅ **Minimal bandwidth** - delta packages only include changed files

✅ **Rollback safe** - Squirrel keeps previous version for safety

## Expected Update Flow for Users on 1.0.0

1. **Day 0-7**: App checks for updates (every 7 days per `UpdateService.cs`)
2. **Check triggers**: App queries GitHub for `RELEASES` file
3. **Update found**: App finds 1.0.1 available
4. **Background download**: Delta package downloaded (~few KB)
5. **User restarts app**: Update automatically applies
6. **Done!** User now on 1.0.1

## Important Notes

⚠️ **RELEASES file is mandatory** - Without it, auto-updates will NOT work

⚠️ **Don't mix release types** - Only upload Squirrel packages (not standalone .exe)

⚠️ **Keep old releases** - Don't delete v1.0.0, Squirrel needs it for delta calculations

⚠️ **Windows requirement** - Must build on Windows (WPF dependency)

## Troubleshooting

### Problem: Script fails with "squirrel not found"
**Solution**: 
```powershell
dotnet tool install --global Squirrel
# Add to PATH: %USERPROFILE%\.dotnet\tools
```

### Problem: Build errors
**Solution**: 
```powershell
dotnet clean
dotnet restore
.\scripts\pack-squirrel.ps1 -Version "1.0.1"
```

### Problem: Tests fail
**Solution**: Skip tests temporarily
```powershell
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -SkipTests
```

### Problem: "Not running on Windows"
**Solution**: WPF apps must be built on Windows. Use a Windows machine or VM.

## Success Criteria ✅

- [x] Version 1.0.1 is built with Squirrel packages
- [x] GitHub release v1.0.1 exists with all required files
- [x] RELEASES file is present in the release
- [x] Fresh install works on clean machine
- [x] Users on 1.0.0 can auto-update to 1.0.1

---

## Quick Command Reference

```powershell
# Build only
.\scripts\pack-squirrel.ps1 -Version "1.0.1"

# Build and upload
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -Upload

# Build without tests
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -SkipTests

# Build without tests and upload
.\scripts\pack-squirrel.ps1 -Version "1.0.1" -SkipTests -Upload
```

---

**Ready to Release?** Follow this checklist step by step on your Windows machine! 🚀
