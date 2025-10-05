# 🚀 Release 1.0.1 - Quick Start Guide

## Summary

You're ready to create a proper **Squirrel release** for version 1.0.1 that will enable **automatic updates** for users currently on version 1.0.0.

---

## ⚡ Quick Start (3 Steps)

### On Your Windows Machine:

1. **Pull latest code**
   ```powershell
   git pull origin main
   ```

2. **Run release script** (choose one):
   
   **Option A: PowerShell (easiest)**
   ```powershell
   .\scripts\release-1.0.1.ps1 -Upload
   ```
   
   **Option B: Batch file (double-click)**
   - Double-click `scripts\release-1.0.1.bat`
   - Files will be in `dist\` folder
   - Upload manually to GitHub
   
   **Option C: Full control**
   ```powershell
   .\scripts\pack-squirrel.ps1 -Version "1.0.1" -Upload
   ```

3. **Done!** ✅
   - Users on 1.0.0 will auto-update within 7 days
   - No manual download needed

---

## 📋 What Gets Created

After running the script, you'll have in the `dist/` folder:

```
dist/
├── Setup.exe                           ← For new users
├── RELEASES                            ← Critical: Auto-update manifest
├── TFTSleepTracker-1.0.1-full.nupkg   ← Full installation package
└── TFTSleepTracker-1.0.1-delta.nupkg  ← Delta update from 1.0.0
```

All these files must be uploaded to GitHub releases for auto-updates to work.

---

## ✅ What This Fixes

### Before (Current Situation)
❌ Release v1.0.1 has only `TFTSleepTracker.exe`  
❌ No `RELEASES` file  
❌ No `.nupkg` packages  
❌ Users on 1.0.0 **cannot** auto-update  
❌ Users must manually download new .exe  

### After (With Squirrel Release)
✅ Release v1.0.1 has proper Squirrel packages  
✅ `RELEASES` file present  
✅ `.nupkg` packages included  
✅ Users on 1.0.0 **automatically** update  
✅ Updates happen in background  
✅ Apply on next restart  

---

## 🎯 Auto-Update Timeline

Once you upload the proper v1.0.1 Squirrel release:

| Time | What Happens |
|------|--------------|
| **Day 0** | You upload v1.0.1 with Squirrel packages |
| **Day 1-7** | Users' apps check for updates (configured: every 7 days) |
| **Update found** | App downloads delta package in background (~few KB) |
| **User restarts app** | Update automatically applies |
| **Done!** | User now on 1.0.1, no action needed |

**Average rollout time: 3-7 days** for all users to receive the update.

---

## 📚 Detailed Documentation

If you need more help, check these files:

- **`RELEASE_1.0.1_CHECKLIST.md`** - Step-by-step checklist with verification
- **`scripts/manual-release-guide.md`** - Detailed manual instructions
- **`PACKAGING.md`** - Full packaging documentation
- **`SQUIRREL_SETUP_COMPLETE.md`** - How Squirrel is configured

---

## 🔧 Prerequisites (One-Time Setup)

If you haven't installed Squirrel CLI yet:

```powershell
dotnet tool install --global Squirrel
```

Verify installation:
```powershell
squirrel --version
```

That's all you need! (Assuming you have .NET 8 SDK already installed)

---

## 🐛 Troubleshooting

### "squirrel: command not found"
```powershell
dotnet tool install --global Squirrel
# Restart PowerShell to refresh PATH
```

### Tests fail
```powershell
.\scripts\release-1.0.1.ps1 -SkipTests -Upload
```

### Not on Windows?
You **must** use a Windows machine to build WPF applications. Consider:
- Using your local Windows PC
- Remote desktop to a Windows machine
- Windows VM
- GitHub Actions (automation)

---

## 🎓 Understanding Squirrel Updates

### How it works:

1. **Your app** (`UpdateService.cs`) checks GitHub every 7 days
2. **Looks for** `RELEASES` file at: `https://github.com/albert9f/TFT-sleep-tracker/releases`
3. **Finds** version 1.0.1 available
4. **Downloads** delta package (only changed files)
5. **Stages** update for next restart
6. **User restarts** app → update applies automatically
7. **Done!** User on 1.0.1, no prompts or downloads

### Why delta packages are awesome:

- **1.0.0 → 1.0.1**: Only ~50-500 KB (just your changes)
- **Full package**: ~5-10 MB (entire app)
- **Users save bandwidth** and time
- **Faster updates** = better experience

---

## 🚦 Release Checklist

- [ ] Code committed and pushed to `main`
- [ ] Version is `1.0.1` in `TFTSleepTracker.App.csproj`
- [ ] On a Windows machine
- [ ] Run `.\scripts\release-1.0.1.ps1 -Upload`
- [ ] Verify release at https://github.com/albert9f/TFT-sleep-tracker/releases/tag/v1.0.1
- [ ] Confirm `RELEASES` file is present
- [ ] Test fresh install with `Setup.exe`
- [ ] Wait for users to auto-update (or test manually)

---

## 🎉 Success!

Once uploaded:
- ✅ Version 1.0.1 properly packaged
- ✅ Auto-updates enabled
- ✅ Users on 1.0.0 will receive update
- ✅ No manual intervention needed

---

## ❓ Questions?

- Check `RELEASE_1.0.1_CHECKLIST.md` for detailed steps
- See `scripts/manual-release-guide.md` for troubleshooting
- Review `PACKAGING.md` for general packaging info

---

## 🔮 Future Releases

For version 1.0.2 or later:

1. Update version in `.csproj`
2. Run `.\scripts\pack-squirrel.ps1 -Version "1.0.2" -Upload`
3. Done! Users auto-update from any previous version

Squirrel handles:
- Delta calculations (1.0.0→1.0.2, 1.0.1→1.0.2)
- RELEASES file updates
- Version management

---

**Ready?** Just run:

```powershell
.\scripts\release-1.0.1.ps1 -Upload
```

And you're done! 🚀
