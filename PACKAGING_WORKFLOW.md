# Squirrel Packaging Workflow

```
┌─────────────────────────────────────────────────────────────────────┐
│                     SQUIRREL PACKAGING WORKFLOW                      │
└─────────────────────────────────────────────────────────────────────┘

                            ┌──────────────┐
                            │  Run Script  │
                            │              │
                            │  Version?    │
                            └──────┬───────┘
                                   │
                                   ▼
                    ┌──────────────────────────┐
                    │  [1/7] Check Prerequisites│
                    │  • .NET SDK installed?   │
                    │  • Squirrel CLI present? │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [2/7] Clean Build Dirs  │
                    │  • Remove dist/          │
                    │  • Remove nupkgs/        │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [3/7] Restore Packages  │
                    │  dotnet restore          │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [4/7] Build Solution    │
                    │  dotnet build -c Release │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [5/7] Run Tests         │
                    │  dotnet test             │
                    │  (skip with -SkipTests)  │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [6/7] Publish App       │
                    │  • Self-contained        │
                    │  • Single file           │
                    │  • Trimmed               │
                    │  • win-x64 runtime       │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [7/7] Create NuGet Pkg  │
                    │  squirrel pack           │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  [8/7] Releasify         │
                    │  squirrel releasify      │
                    │  • Setup.exe             │
                    │  • RELEASES              │
                    │  • *.nupkg packages      │
                    └──────────┬───────────────┘
                               │
                               ▼
                    ┌──────────────────────────┐
                    │  OPTIONAL: Upload        │
                    │  gh release create       │
                    │  (with -Upload flag)     │
                    └──────────┬───────────────┘
                               │
                               ▼
                         ┌───────────┐
                         │   DONE!   │
                         └───────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                           OUTPUT STRUCTURE                           │
└─────────────────────────────────────────────────────────────────────┘

  dist/                                      ← Main output folder
  ├── Setup.exe                              ← 🎯 INSTALLER (distribute)
  ├── RELEASES                               ← 📋 Update manifest
  ├── TFTSleepTracker-1.0.0-full.nupkg     ← 📦 Full package
  └── TFTSleepTracker-1.0.1-delta.nupkg    ← ⚡ Delta (next update)

  nupkgs/                                    ← Intermediate packages
  └── TFTSleepTracker.1.0.0.nupkg           ← 📦 Source NuGet package


┌─────────────────────────────────────────────────────────────────────┐
│                         DISTRIBUTION FLOW                            │
└─────────────────────────────────────────────────────────────────────┘

  Developer                    End User              Update Server
  ─────────                    ────────              ─────────────

     │                                                     │
     │ 1. Run pack-squirrel.ps1                           │
     ├──────────────────┐                                 │
     │   dist/Setup.exe │                                 │
     │                  │                                 │
     │ 2. Upload Setup.exe                                │
     ├────────────────────────────────────────────────────┤
     │                         │                          │
     │                         │ 3. Download Setup.exe    │
     │                         ├──────────────────┐       │
     │                         │  Run installer   │       │
     │                         │  (First install) │       │
     │                         └──────────────────┘       │
     │                         │                          │
     │                         │ App installed at:        │
     │                         │ %LocalAppData%\          │
     │                         │   TFTSleepTracker\       │
     │                         │                          │
     │ 4. Build v1.0.1                                    │
     ├──────────────────┐                                 │
     │   dist/          │                                 │
     │   ├─ Setup.exe   │                                 │
     │   ├─ RELEASES    │                                 │
     │   └─ *.nupkg     │                                 │
     │                  │                                 │
     │ 5. Upload dist/ to server                          │
     ├────────────────────────────────────────────────────┤
     │                         │                          │
     │                         │ 6. App checks for update │
     │                         ├──────────────────────────┤
     │                         │         ◄────────────────┤
     │                         │    RELEASES file         │
     │                         │    delta.nupkg           │
     │                         │                          │
     │                         │ 7. Auto-update applied   │
     │                         │    (seamless!)           │
     │                         │                          │


┌─────────────────────────────────────────────────────────────────────┐
│                         VERSION TIMELINE                             │
└─────────────────────────────────────────────────────────────────────┘

  v1.0.0 (Initial Release)
    │
    │   dist/Setup.exe ──────────────► Distributed to users
    │   dist/RELEASES
    │   dist/*-full.nupkg
    │
    ▼
  User installs ───────────► App running v1.0.0
    │
    │
    ▼
  v1.0.1 (Bug Fix)
    │
    │   dist/Setup.exe ──────────────► New users get v1.0.1
    │   dist/RELEASES
    │   dist/*-1.0.0-full.nupkg
    │   dist/*-1.0.1-delta.nupkg ──► Existing users get delta
    │
    ▼
  App auto-updates ─────────► Now running v1.0.1
    │                           (only downloads delta!)
    │
    ▼
  v1.1.0 (New Feature)
    │
    │   dist/Setup.exe
    │   dist/RELEASES
    │   dist/*-1.0.1-full.nupkg
    │   dist/*-1.1.0-delta.nupkg
    │
    ▼
  App auto-updates ─────────► Now running v1.1.0


┌─────────────────────────────────────────────────────────────────────┐
│                     SCRIPT OPTIONS COMPARISON                        │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────┬──────────────┬──────────────┬──────────────┐
│ Feature              │ pack-        │ pack-        │ build-       │
│                      │ squirrel.ps1 │ simple.ps1   │ installer.bat│
├──────────────────────┼──────────────┼──────────────┼──────────────┤
│ Prerequisites check  │      ✓       │      ✗       │      ✗       │
│ Clean directories    │      ✓       │      ✓       │      ✓       │
│ Restore packages     │      ✓       │      ✗       │      ✓       │
│ Build solution       │      ✓       │      ✗       │      ✓       │
│ Run tests            │      ✓       │      ✗       │      ✓       │
│ Publish app          │      ✓       │      ✓       │      ✓       │
│ Create NuGet package │      ✓       │      ✓       │      ✓       │
│ Releasify            │      ✓       │      ✓       │      ✓       │
│ GitHub upload        │   optional   │      ✗       │      ✗       │
│ Pretty output        │      ✓       │      ~       │      ~       │
│ Skip tests option    │      ✓       │     N/A      │      ✗       │
│ Error handling       │   robust     │    basic     │    basic     │
│ Double-click run     │      ✗       │      ✗       │      ✓       │
├──────────────────────┼──────────────┼──────────────┼──────────────┤
│ Use case             │ Production   │ Learn/Debug  │ Convenience  │
│                      │ releases     │ commands     │ for non-PS   │
└──────────────────────┴──────────────┴──────────────┴──────────────┘


┌─────────────────────────────────────────────────────────────────────┐
│                         FILE SIZE GUIDE                              │
└─────────────────────────────────────────────────────────────────────┘

  Component                           Size (approx)
  ───────────────────────────────────────────────────────
  Your app code                       ~2-5 MB
  .NET runtime (self-contained)       ~80-100 MB
  ───────────────────────────────────────────────────────
  Total published app                 ~85-105 MB
  
  Setup.exe (installer)               ~85-105 MB
  Full .nupkg                         ~85-105 MB
  Delta .nupkg (updates)              ~2-10 MB (only changes!)
  
  💡 Users only download deltas for updates!


┌─────────────────────────────────────────────────────────────────────┐
│                       COMMON COMMANDS CHEAT SHEET                    │
└─────────────────────────────────────────────────────────────────────┘

  # First-time setup
  dotnet tool install --global Squirrel
  
  # Create release
  .\pack-squirrel.ps1 -Version "1.0.0"
  
  # Quick build (skip tests)
  .\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
  
  # Upload to GitHub
  .\pack-squirrel.ps1 -Version "1.0.0" -Upload
  
  # Simple script (minimal)
  .\pack-simple.ps1 -Version "1.0.0"
  
  # Batch file (double-click)
  build-installer.bat 1.0.0
  
  # Check Squirrel version
  squirrel --version
  
  # Check .NET version
  dotnet --version
  
  # Test installer locally
  .\dist\Setup.exe
  
  # Uninstall (for testing)
  %LocalAppData%\TFTSleepTracker\Update.exe --uninstall


┌─────────────────────────────────────────────────────────────────────┐
│                          INSTALLATION PATHS                          │
└─────────────────────────────────────────────────────────────────────┘

  Squirrel installs to:
    %LocalAppData%\TFTSleepTracker\
    
  Typical path:
    C:\Users\<username>\AppData\Local\TFTSleepTracker\
    
  Structure:
    TFTSleepTracker\
    ├── app-1.0.0\              ← Current version
    │   └── TFTSleepTracker.exe
    ├── app-1.0.1\              ← After update
    │   └── TFTSleepTracker.exe
    ├── Update.exe              ← Squirrel updater
    └── packages\               ← Cached updates
    
  Shortcuts created at:
    Desktop:     %USERPROFILE%\Desktop\TFT Sleep Tracker.lnk
    Start Menu:  %APPDATA%\Microsoft\Windows\Start Menu\Programs\TFT Sleep Tracker.lnk


┌─────────────────────────────────────────────────────────────────────┐
│                              SUCCESS!                                │
└─────────────────────────────────────────────────────────────────────┘

  You're ready to package your app! 🎉
  
  Next steps:
  1. cd scripts
  2. .\pack-squirrel.ps1 -Version "1.0.0"
  3. Distribute dist\Setup.exe
  
  Questions? See PACKAGING.md or PACKAGING_QUICKREF.md
```
