# Scripts Directory

This folder contains all the packaging and build scripts for TFT Sleep Tracker.

## 📦 Packaging Scripts

### pack-squirrel.ps1 (⭐ Recommended)

**Full-featured packaging script using official Squirrel CLI**

```powershell
.\pack-squirrel.ps1 -Version "1.0.0"
```

**Features:**
- ✅ Prerequisite checking
- ✅ Clean build process
- ✅ Automated testing
- ✅ Self-contained publishing
- ✅ NuGet package creation
- ✅ Installer generation
- ✅ Optional GitHub upload
- ✅ Beautiful progress output
- ✅ Comprehensive error handling

**Options:**
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

**Output:** `../dist/Setup.exe` and update packages

---

### pack-simple.ps1

**Minimal script showing essential Squirrel commands**

```powershell
.\pack-simple.ps1 -Version "1.0.0"
```

**Use when:**
- Learning Squirrel commands
- Debugging build issues
- Need quick reference
- Troubleshooting full script

**What it does:**
1. Clean directories
2. Publish app
3. Create NuGet package
4. Generate installer

**No extras:** No tests, no checks, no GitHub upload - just the core Squirrel workflow.

---

### build-installer.bat

**Windows batch file wrapper for double-clicking**

```cmd
build-installer.bat 1.0.0
```

**Use when:**
- Prefer GUI over command line
- Don't want to open PowerShell
- Want simple double-click build

**How it works:**
1. Double-click `build-installer.bat`
2. Script prompts for version (if not provided)
3. Calls `pack-squirrel.ps1` internally
4. Shows results and pauses

---

### pack.ps1

**Original packaging script using Clowd.Squirrel (fork)**

```powershell
.\pack.ps1 -Version "1.0.0"
.\pack.ps1 -Version "1.0.0" -Upload
```

**Difference from pack-squirrel.ps1:**
- Uses `Clowd.Squirrel` instead of official `Squirrel`
- Both work fine - choose based on preference
- See [comparison table](../SQUIRREL_SETUP_COMPLETE.md#-comparison-with-clowdsquirrel)

---

## 🔍 Diagnostic Script

### diagnose.ps1

**Environment checker and troubleshooting tool**

```powershell
.\diagnose.ps1
```

**Checks:**
- ✓ .NET SDK installation and version
- ✓ Squirrel CLI installation
- ✓ PowerShell version
- ✓ Operating system
- ✓ Current directory and path length
- ✓ Repository structure
- ✓ Disk space
- ✓ .NET tools PATH configuration
- ✓ GitHub CLI (optional)

**When to use:**
- Before first build
- Troubleshooting build issues
- After installing prerequisites
- Setting up new machine

---

## 📖 Quick Reference

| Script | Purpose | When to Use |
|--------|---------|-------------|
| `pack-squirrel.ps1` | Full automated build | Production releases |
| `pack-simple.ps1` | Minimal build | Learning, debugging |
| `build-installer.bat` | GUI wrapper | Non-PowerShell users |
| `pack.ps1` | Clowd.Squirrel build | Alternative packaging |
| `diagnose.ps1` | Environment check | Setup, troubleshooting |

---

## 🚀 Getting Started

### First Time Setup

1. **Install prerequisites:**
   ```powershell
   # Install Squirrel CLI
   dotnet tool install --global Squirrel
   ```

2. **Verify environment:**
   ```powershell
   .\diagnose.ps1
   ```

3. **Build your first installer:**
   ```powershell
   .\pack-squirrel.ps1 -Version "1.0.0"
   ```

4. **Find your installer:**
   ```
   ..\dist\Setup.exe
   ```

### Subsequent Builds

```powershell
# Increment version and rebuild
.\pack-squirrel.ps1 -Version "1.0.1"
```

---

## 📂 Output Directories

After running a packaging script:

```
TFT-sleep-tracker/
├── dist/                           ← Main output (distribute this!)
│   ├── Setup.exe                   ← Give to users
│   ├── RELEASES                    ← Update manifest
│   └── *.nupkg                     ← Update packages
│
├── nupkgs/                         ← Intermediate packages
│   └── TFTSleepTracker.*.nupkg    ← Source NuGet package
│
└── TFTSleepTracker.App/bin/Release/...
    └── publish/                    ← Published app files
```

---

## ⚙️ Script Internals

### What pack-squirrel.ps1 Does

```
[0/7] Check prerequisites
  ├─ Verify .NET SDK
  └─ Install Squirrel CLI if needed

[1/7] Clean build directories
  ├─ Remove dist/
  └─ Remove nupkgs/

[2/7] Restore NuGet packages
  └─ dotnet restore

[3/7] Build solution
  └─ dotnet build -c Release

[4/7] Run tests
  └─ dotnet test (skip with -SkipTests)

[5/7] Publish app
  ├─ Self-contained
  ├─ Single file
  ├─ Trimmed
  └─ win-x64 runtime

[6/7] Create NuGet package
  └─ squirrel pack

[7/7] Generate installer
  └─ squirrel releasify

[8/7] Optional: Upload to GitHub
  └─ gh release create (with -Upload)
```

---

## 🐛 Troubleshooting

### Common Issues

**"squirrel: command not found"**
```powershell
dotnet tool install --global Squirrel
# Restart PowerShell
```

**"Tests failed"**
```powershell
# Skip tests
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
```

**"Build errors"**
```powershell
# Run diagnostics
.\diagnose.ps1

# Clean and retry
dotnet clean
.\pack-squirrel.ps1 -Version "1.0.0"
```

**"Path too long"**
```powershell
# Move repository to shorter path
# e.g., C:\code\TFT\
```

📖 **See [TROUBLESHOOTING.md](../TROUBLESHOOTING.md) for comprehensive guide**

---

## 📚 Documentation

- **[PACKAGING.md](../PACKAGING.md)** - Full packaging guide
- **[PACKAGING_QUICKREF.md](../PACKAGING_QUICKREF.md)** - Quick commands
- **[PACKAGING_WORKFLOW.md](../PACKAGING_WORKFLOW.md)** - Visual workflow
- **[TROUBLESHOOTING.md](../TROUBLESHOOTING.md)** - Problem solving
- **[RELEASE_CHECKLIST.md](../RELEASE_CHECKLIST.md)** - First release guide

---

## 💡 Tips

### Speed Up Builds

```powershell
# Skip tests
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests

# Use simple script (no tests, no checks)
.\pack-simple.ps1 -Version "1.0.0"
```

### Automate Releases

```powershell
# Build and upload in one command
.\pack-squirrel.ps1 -Version "1.0.0" -Upload
```

### Debug Build Issues

```powershell
# 1. Run diagnostics
.\diagnose.ps1

# 2. Try simple script
.\pack-simple.ps1 -Version "1.0.0"

# 3. Run steps manually
dotnet restore
dotnet build -c Release
dotnet publish ...
```

---

## 🆘 Need Help?

1. Run diagnostics: `.\diagnose.ps1`
2. Check troubleshooting guide: `TROUBLESHOOTING.md`
3. Read packaging docs: `PACKAGING.md`
4. Open an issue on GitHub

---

**Ready to build? Start here:**

```powershell
# Check environment
.\diagnose.ps1

# Build installer
.\pack-squirrel.ps1 -Version "1.0.0"

# Test installer
..\dist\Setup.exe
```

🎉 **You've got this!**
