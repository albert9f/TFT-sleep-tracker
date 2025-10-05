# First Release Checklist

Use this checklist when creating your first release of TFT Sleep Tracker.

## Pre-Release Checklist

### ✅ Development Complete
- [ ] All features implemented
- [ ] Code reviewed and cleaned up
- [ ] Comments and documentation updated
- [ ] Known bugs fixed or documented
- [ ] Version number decided (suggest: `1.0.0`)

### ✅ Testing
- [ ] All unit tests pass: `dotnet test`
- [ ] Manual testing on Windows 10
- [ ] Manual testing on Windows 11
- [ ] Tested autostart functionality
- [ ] Tested system tray icon and menu
- [ ] Tested sleep tracking accuracy
- [ ] Tested upload queue functionality
- [ ] Tested with Discord bot (if applicable)
- [ ] Tested hibernate/resume
- [ ] Tested RDP disconnect/reconnect
- [ ] Tested DST transition (if near date)

### ✅ Configuration
- [ ] Default settings are appropriate
- [ ] Bot host URL configured (if applicable)
- [ ] Token mechanism tested
- [ ] Update URL configured (if using auto-update)

### ✅ Build Environment
- [ ] .NET 8 SDK installed
- [ ] Windows machine available (or VM)
- [ ] Squirrel CLI installed: `dotnet tool install --global Squirrel`
- [ ] GitHub CLI installed (if using `-Upload` flag): https://cli.github.com/
- [ ] Authenticated to GitHub: `gh auth login`

### ✅ Documentation
- [ ] README.md is up to date
- [ ] END_USER_README.md exists and is clear
- [ ] Installation instructions tested
- [ ] Configuration examples provided
- [ ] Known issues documented

## Build & Package

### ✅ Pre-Build
- [ ] All changes committed to git
- [ ] Working directory is clean: `git status`
- [ ] On correct branch (e.g., `main`)
- [ ] Pulled latest changes: `git pull`

### ✅ Version Update
- [ ] Updated version in `TFTSleepTracker.App/TFTSleepTracker.App.csproj`
  ```xml
  <Version>1.0.0</Version>
  ```
- [ ] Updated CHANGELOG.md (if exists)
- [ ] Committed version bump

### ✅ Build Package
- [ ] Navigated to scripts folder: `cd scripts`
- [ ] Ran packaging script:
  ```powershell
  .\pack-squirrel.ps1 -Version "1.0.0"
  ```
- [ ] Build completed successfully
- [ ] All tests passed
- [ ] Installer created in `dist/Setup.exe`

### ✅ Verify Output
- [ ] `dist/Setup.exe` exists
- [ ] `dist/RELEASES` exists
- [ ] `dist/*.nupkg` files exist
- [ ] File sizes look reasonable (~85-105 MB)

## Testing the Installer

### ✅ Local Testing
- [ ] Copied `Setup.exe` to a test folder
- [ ] Ran `Setup.exe` as a regular user (not admin)
- [ ] Installer completed without errors
- [ ] App launched successfully
- [ ] Desktop shortcut created
- [ ] Start Menu entry created
- [ ] System tray icon appears
- [ ] App starts on Windows login
- [ ] Can access settings
- [ ] Can view today's data
- [ ] Can close app (stays in tray)
- [ ] Can exit app completely

### ✅ Clean Install Testing
- [ ] Tested on a fresh Windows machine (or VM)
- [ ] No .NET runtime pre-installed required
- [ ] Installer works without admin rights
- [ ] App functions correctly after install

### ✅ Uninstall Testing
- [ ] Located uninstaller:
  ```
  %LocalAppData%\TFTSleepTracker\Update.exe --uninstall
  ```
- [ ] Ran uninstaller
- [ ] All files removed
- [ ] Shortcuts removed
- [ ] Autostart registry entry removed

### ✅ Update Testing (for v1.0.1+)
- [ ] Build new version with higher number
- [ ] Upload to update server
- [ ] App detects update
- [ ] Delta update downloads
- [ ] Update applies successfully
- [ ] App restarts with new version

## Distribution

### ✅ GitHub Release
- [ ] Created git tag: `git tag v1.0.0`
- [ ] Pushed tag: `git push origin v1.0.0`
- [ ] Created GitHub Release (manual or with `-Upload` flag)
- [ ] Uploaded `Setup.exe` to release
- [ ] Uploaded `RELEASES` file to release
- [ ] Uploaded `*.nupkg` files to release
- [ ] Added release notes
- [ ] Marked as "Latest" release

### ✅ Update Server Setup (Optional)
- [ ] Web server configured
- [ ] Uploaded entire `dist/` folder contents
- [ ] Accessible at update URL
- [ ] Tested accessing RELEASES file via URL
- [ ] App configured with correct update URL

### ✅ End User Communication
- [ ] Installation instructions provided
- [ ] System requirements listed (Windows 10/11, 64-bit)
- [ ] Configuration guide available
- [ ] Support contact provided
- [ ] Known limitations documented

## Post-Release

### ✅ Monitoring
- [ ] Track installation feedback
- [ ] Monitor error reports
- [ ] Check Discord bot for uploads (if applicable)
- [ ] Note any compatibility issues
- [ ] Document user feedback

### ✅ Hotfix Preparation
- [ ] Know how to quickly build v1.0.1
- [ ] Update server credentials available
- [ ] GitHub access verified
- [ ] Emergency contact list ready

## Troubleshooting Checklist

If build fails:

- [ ] Check .NET SDK version: `dotnet --version` (need 8.0+)
- [ ] Check Squirrel installed: `squirrel --version`
- [ ] Clean solution: Delete `bin/`, `obj/`, `dist/`, `nupkgs/` folders
- [ ] Restore packages: `dotnet restore`
- [ ] Check for build errors in output
- [ ] Try simple script: `.\pack-simple.ps1 -Version "1.0.0"`

If installer fails:

- [ ] Run Setup.exe from Command Prompt to see errors
- [ ] Check Windows Event Viewer for .NET errors
- [ ] Verify Windows version (10/11, 64-bit)
- [ ] Check disk space (need ~200 MB)
- [ ] Try installing as administrator
- [ ] Check antivirus not blocking

If app doesn't start:

- [ ] Check installed location: `%LocalAppData%\TFTSleepTracker`
- [ ] Look for error logs
- [ ] Check Windows Event Viewer
- [ ] Verify .NET runtime included (self-contained build)
- [ ] Try running `TFTSleepTracker.exe` directly

## Version History Template

Keep track in CHANGELOG.md:

```markdown
# Changelog

## [1.0.0] - 2025-10-04

### Added
- Initial release
- Sleep tracking during 11 PM - 8 AM window
- System tray integration
- Autostart functionality
- Discord bot upload integration
- Squirrel auto-update support

### Known Issues
- None

## [Unreleased]

### Added
- (future features)

### Fixed
- (future fixes)
```

## Success Criteria

Your release is successful when:

- ✅ Users can download and install without assistance
- ✅ App runs reliably 24/7
- ✅ Sleep tracking data is accurate
- ✅ Updates install automatically
- ✅ No critical bugs reported in first week
- ✅ Users provide positive feedback

---

## Quick Reference

```powershell
# Build release
cd scripts
.\pack-squirrel.ps1 -Version "1.0.0"

# Test install
.\dist\Setup.exe

# Uninstall (for testing)
%LocalAppData%\TFTSleepTracker\Update.exe --uninstall

# Upload to GitHub
.\pack-squirrel.ps1 -Version "1.0.0" -Upload
```

---

**Good luck with your release! 🚀**

Remember: It's better to delay and test thoroughly than to rush and have issues in production.
