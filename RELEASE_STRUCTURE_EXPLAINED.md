# Release Structure Comparison

## ❌ Current Release Structure (Won't Auto-Update)

Your current v1.0.1 release on GitHub:

```
v1.0.1/
└── TFTSleepTracker.exe          ← Single executable
```

**Problem**: Your app's `UpdateService.cs` expects Squirrel packages, not a standalone .exe

**Result**: Users on 1.0.0 will NOT be notified of updates

---

## ✅ Required Release Structure (Will Auto-Update)

What your v1.0.1 release SHOULD contain:

```
v1.0.1/
├── Setup.exe                            ← Installer for new users
├── RELEASES                             ← CRITICAL: Auto-update manifest
├── TFTSleepTracker-1.0.1-full.nupkg    ← Full installation package
└── TFTSleepTracker-1.0.1-delta.nupkg   ← Delta update from 1.0.0
```

**Result**: Users on 1.0.0 will automatically receive 1.0.1 within 7 days

---

## 📊 Visual Comparison

### Current Setup (Broken Auto-Updates)

```
GitHub Release v1.0.1
    │
    └── TFTSleepTracker.exe
    
User on v1.0.0
    │
    ├─ UpdateService checks GitHub
    ├─ Looks for RELEASES file  ❌ NOT FOUND
    ├─ Cannot determine updates
    └─ User stays on 1.0.0      ❌ STUCK
```

### Correct Setup (Working Auto-Updates)

```
GitHub Release v1.0.1
    ├── Setup.exe
    ├── RELEASES                 ✅ FOUND
    ├── *.full.nupkg
    └── *.delta.nupkg
    
User on v1.0.0
    │
    ├─ UpdateService checks GitHub
    ├─ Finds RELEASES file       ✅ SUCCESS
    ├─ Sees v1.0.1 available
    ├─ Downloads delta package (background)
    ├─ User restarts app
    └─ Auto-updates to 1.0.1     ✅ SUCCESS
```

---

## 📁 RELEASES File Explained

The `RELEASES` file is a **manifest** that tells your app what versions are available:

```
7B2A5D3F TFTSleepTracker-1.0.1-delta.nupkg 245678
8C3B6E4G TFTSleepTracker-1.0.1-full.nupkg 5234567
```

Each line contains:
- **SHA1 hash** of the package
- **Filename** of the package
- **Size** in bytes

Your app reads this file to determine:
- "Is there a newer version?"
- "Which package should I download?"
- "What's the file size?"

**Without this file, auto-updates are IMPOSSIBLE.**

---

## 🔄 Update Flow with Squirrel

### Step-by-Step Process

```
1. App Startup
   └─> UpdateService initialized
   
2. Every 7 Days (Automatic)
   └─> Check https://github.com/albert9f/TFT-sleep-tracker/releases
   
3. Download RELEASES File
   └─> Parse available versions
   
4. Compare Versions
   Current: 1.0.0
   Available: 1.0.1
   └─> Update available! ✅
   
5. Download Package (Background)
   ├─> Check for delta: TFTSleepTracker-1.0.1-delta.nupkg
   │   └─> Found! Download ~500 KB (just changes)
   │
   └─> Fallback to full if no delta available
   
6. Stage Update
   └─> Extract to temporary location
   
7. User Restarts App
   └─> Update applies
   └─> Old version backed up (for safety)
   
8. Success!
   └─> App now running 1.0.1
```

---

## 📦 Package Types Explained

### Full Package (`-full.nupkg`)
- **Contains**: Entire application (~5-10 MB)
- **Used for**: First install, or if delta fails
- **Always created** by Squirrel

### Delta Package (`-delta.nupkg`)
- **Contains**: Only changed files (~50-500 KB)
- **Used for**: Updating existing installations
- **Only created** if previous version exists
- **Much faster** than full package

---

## 🎯 File Size Comparison

| Scenario | Without Squirrel | With Squirrel |
|----------|------------------|---------------|
| **Fresh Install** | 8 MB (.exe) | 8 MB (full.nupkg) |
| **Update 1.0.0→1.0.1** | 8 MB (new .exe) | 200 KB (delta.nupkg) |
| **Update 1.0.1→1.0.2** | 8 MB (new .exe) | 150 KB (delta.nupkg) |

**Bandwidth saved per user: ~7.8 MB per update!**

With 100 users updating: **780 MB saved!**

---

## ⚙️ How Your Code Uses This

### UpdateService.cs (Already Implemented)

```csharp
using (var mgr = new GithubUpdateManager("https://github.com/albert9f/TFT-sleep-tracker"))
{
    var updateInfo = await mgr.CheckForUpdate();
    
    if (updateInfo.ReleasesToApply.Any())
    {
        // Download update in background
        await mgr.DownloadReleases(updateInfo.ReleasesToApply);
        
        // Stage update for next restart
        await mgr.ApplyReleases(updateInfo);
    }
}
```

**What it does:**
1. Connects to your GitHub repo
2. Reads `RELEASES` file
3. Compares versions
4. Downloads delta/full package
5. Stages update
6. Applies on next restart

**All automatic!** No user interaction needed.

---

## 🛠️ How to Fix Your Current Release

### Option 1: Replace v1.0.1 Release

```powershell
# 1. Delete current v1.0.1 release on GitHub
#    (or just add files to existing release)

# 2. Build proper Squirrel packages
.\scripts\release-1.0.1.ps1 -Upload

# 3. Done! Release now has proper structure
```

### Option 2: Create New Release (v1.0.2)

```powershell
# 1. Update version to 1.0.2 in .csproj

# 2. Build with Squirrel
.\scripts\pack-squirrel.ps1 -Version "1.0.2" -Upload

# 3. Users on 1.0.0 and 1.0.1 can now update
```

---

## ✅ Verification Checklist

After uploading, verify your release has:

- [ ] `Setup.exe` - For new installations
- [ ] `RELEASES` - **MUST HAVE** for auto-updates
- [ ] `*-full.nupkg` - Full package
- [ ] `*-delta.nupkg` - Delta package (if upgrading)

**If RELEASES file is missing, auto-updates WILL NOT WORK.**

---

## 🎓 Key Takeaways

1. **Standalone .exe files** do NOT support auto-updates
2. **Squirrel packages** (.nupkg) are required for auto-updates
3. **RELEASES file** is the manifest that enables update checks
4. **Delta packages** save bandwidth and time
5. **Your code is correct** - you just need proper packages
6. **One-time fix** - future releases will use same structure

---

## 📞 Next Steps

1. Read: `RELEASE_1.0.1_QUICK_START.md`
2. Follow: `RELEASE_1.0.1_CHECKLIST.md`
3. Run: `.\scripts\release-1.0.1.ps1 -Upload`
4. Verify: Check GitHub release has all files
5. Test: Install `Setup.exe` on clean machine
6. Wait: Users auto-update within 7 days!

---

**Bottom Line**: You need the full Squirrel package structure, not just a single .exe file, for auto-updates to work.

Run `.\scripts\release-1.0.1.ps1 -Upload` on Windows and you're all set! 🚀
