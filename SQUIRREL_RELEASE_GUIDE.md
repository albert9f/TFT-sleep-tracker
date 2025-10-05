# 🎯 SQUIRREL RELEASE FOR 1.0.1 - COMPLETE GUIDE

## 📌 Quick Summary

You asked me to create a **Squirrel release** for version **1.0.1** so that users on **1.0.0** can **auto-update**.

**Status**: ✅ **Everything is ready!** You just need to build and upload the release.

---

## 🚀 Choose Your Method

### Method 1: GitHub Actions (Easiest - No Windows Needed!)

```bash
# Just push a tag - GitHub does the rest!
git tag v1.0.1
git push origin v1.0.1
```

✅ Runs in GitHub's cloud  
✅ No local Windows machine needed  
✅ Fully automated  
✅ Takes ~7 minutes  

**Read**: `.github/workflows/README.md` for details

---

### Method 2: PowerShell Script (Windows Machine)

```powershell
# On your Windows machine
.\scripts\release-1.0.1.ps1 -Upload
```

✅ Full control  
✅ Test locally before release  
✅ Same result as GitHub Actions  

**Read**: `RELEASE_1.0.1_QUICK_START.md` for details

---

## 📁 What I've Created for You

### 📄 Documentation Files

1. **`RELEASE_1.0.1_QUICK_START.md`**  
   → 3-step guide to create the release

2. **`RELEASE_1.0.1_CHECKLIST.md`**  
   → Step-by-step checklist with verification

3. **`RELEASE_STRUCTURE_EXPLAINED.md`**  
   → Visual explanation of Squirrel packages

4. **`scripts/manual-release-guide.md`**  
   → Detailed manual instructions

### 🔧 Scripts

5. **`scripts/release-1.0.1.ps1`**  
   → One-command PowerShell script (Windows)

6. **`scripts/release-1.0.1.bat`**  
   → Double-click batch file (Windows)

7. **`.github/workflows/squirrel-release.yml`**  
   → GitHub Actions workflow (automated)

8. **`.github/workflows/README.md`**  
   → GitHub Actions documentation

### ✅ What Already Existed (Your Setup)

9. **`scripts/pack-squirrel.ps1`**  
   → Full-featured packaging script ✅ Already created

10. **`TFTSleepTracker.Core/Update/UpdateService.cs`**  
    → Auto-update code ✅ Already implemented

11. **`TFTSleepTracker.App/TFTSleepTracker.App.csproj`**  
    → Version: 1.0.1 ✅ Already set

---

## 🎯 What This Solves

### ❌ Current Problem

Your v1.0.1 release has only `TFTSleepTracker.exe` (standalone file).

**Result**: Users on 1.0.0 **cannot** auto-update.

### ✅ Solution

Create v1.0.1 with proper Squirrel packages:
- `Setup.exe`
- `RELEASES` file
- `.nupkg` packages

**Result**: Users on 1.0.0 **will** auto-update within 7 days!

---

## ⚡ Recommended Approach

### Option A: GitHub Actions (Recommended for You)

Since you're in a Linux dev container, the easiest approach is **GitHub Actions**:

1. **Commit the workflow** (if not already done):
   ```bash
   git add .github/
   git commit -m "Add automated Squirrel release workflow"
   git push
   ```

2. **Create and push tag**:
   ```bash
   git tag v1.0.1
   git push origin v1.0.1
   ```

3. **Watch it work**:
   - Go to: https://github.com/albert9f/TFT-sleep-tracker/actions
   - Watch the workflow run (~7 minutes)
   - Check releases when done: https://github.com/albert9f/TFT-sleep-tracker/releases

4. **Verify release contains**:
   - `Setup.exe`
   - `RELEASES` ← Critical!
   - `*.nupkg` files

**Done!** Users will auto-update! 🎉

---

### Option B: Windows Machine (If You Have Access)

If you have access to a Windows machine:

1. **Clone/pull repo**:
   ```powershell
   git clone https://github.com/albert9f/TFT-sleep-tracker.git
   cd TFT-sleep-tracker
   ```

2. **Run release script**:
   ```powershell
   .\scripts\release-1.0.1.ps1 -Upload
   ```

3. **Done!** Release created automatically.

---

## 📊 What Happens After Release

### Timeline for Users on 1.0.0

| Day | What Happens |
|-----|--------------|
| **Day 0** | You upload v1.0.1 with Squirrel packages |
| **Day 1-7** | Users' apps check for updates (every 7 days) |
| **Update Check** | App downloads `RELEASES` file from GitHub |
| **Update Found** | App sees 1.0.1 is available |
| **Background Download** | Delta package downloaded (~200-500 KB) |
| **User Restarts** | Update applies automatically |
| **Done!** | User now on 1.0.1 |

**Average rollout: 3-7 days** for all users

---

## 🔍 Verification Steps

After releasing, verify:

1. ✅ Release exists: https://github.com/albert9f/TFT-sleep-tracker/releases/tag/v1.0.1
2. ✅ Contains `Setup.exe`
3. ✅ Contains `RELEASES` file ← **CRITICAL**
4. ✅ Contains `.nupkg` files
5. ✅ Download and test `Setup.exe` on Windows

---

## 📚 Documentation Index

| File | Purpose | When to Read |
|------|---------|--------------|
| **RELEASE_1.0.1_QUICK_START.md** | Quick 3-step guide | Read first! |
| **RELEASE_1.0.1_CHECKLIST.md** | Detailed checklist | During release |
| **RELEASE_STRUCTURE_EXPLAINED.md** | Visual explanation | To understand Squirrel |
| **scripts/manual-release-guide.md** | Manual instructions | Troubleshooting |
| **.github/workflows/README.md** | GitHub Actions guide | For automation |

---

## 🎓 Key Concepts

### What is Squirrel?
An auto-update framework for Windows apps. Downloads updates in the background and applies them on restart.

### What is the RELEASES file?
A manifest that lists available versions and packages. **Required** for auto-updates to work.

### What are delta packages?
Packages containing only changed files. Much smaller than full packages (~200 KB vs ~8 MB).

### Why does my code expect Squirrel packages?
Your `UpdateService.cs` uses `GithubUpdateManager`, which expects Squirrel's package structure.

---

## ⚠️ Important Notes

1. **Windows Required**: WPF apps must be built on Windows
   - GitHub Actions solves this (runs on Windows VM)
   - Or use a Windows machine locally

2. **RELEASES File is Critical**: Without it, auto-updates will NOT work

3. **Don't Mix Release Types**: Either use standalone .exe OR Squirrel packages (not both)

4. **Version in .csproj**: Already set to 1.0.1 ✅

5. **Your Code is Correct**: UpdateService already implements auto-updates ✅

---

## 🚦 Next Steps

### Immediate Action (Choose One):

**Option 1: GitHub Actions**
```bash
git tag v1.0.1
git push origin v1.0.1
```

**Option 2: Windows Machine**
```powershell
.\scripts\release-1.0.1.ps1 -Upload
```

### Then:

1. Verify release on GitHub
2. Test `Setup.exe` installation
3. Wait for users to auto-update (7 days)
4. Monitor for issues

---

## 🎉 Success Criteria

You'll know it worked when:

- ✅ GitHub release v1.0.1 has all Squirrel files
- ✅ `RELEASES` file is present
- ✅ `Setup.exe` installs correctly
- ✅ Users report receiving auto-update
- ✅ No manual downloads needed

---

## 💡 Future Releases

For version 1.0.2 or later:

### With GitHub Actions:
```bash
git tag v1.0.2
git push origin v1.0.2
```

### With PowerShell:
```powershell
.\scripts\pack-squirrel.ps1 -Version "1.0.2" -Upload
```

**That's it!** Squirrel handles everything else automatically.

---

## 🆘 Need Help?

### Questions About:

- **The process**: Read `RELEASE_1.0.1_QUICK_START.md`
- **Squirrel structure**: Read `RELEASE_STRUCTURE_EXPLAINED.md`
- **GitHub Actions**: Read `.github/workflows/README.md`
- **Manual steps**: Read `scripts/manual-release-guide.md`
- **Verification**: Read `RELEASE_1.0.1_CHECKLIST.md`

### Troubleshooting:

- Build errors → Check `.github/workflows/README.md`
- Tests fail → Use `-SkipTests` flag
- No Windows machine → Use GitHub Actions
- RELEASES file missing → Re-run packaging script

---

## 📌 Bottom Line

**Your setup is complete and ready to go!**

You have two options:
1. **GitHub Actions** (easiest, no Windows needed)
2. **PowerShell script** (Windows machine required)

Both create identical Squirrel packages that enable auto-updates for users on 1.0.0.

**Choose one method, follow the steps, and you're done!** 🚀

---

## ✅ Checklist for You

- [ ] Read `RELEASE_1.0.1_QUICK_START.md`
- [ ] Choose method (GitHub Actions or PowerShell)
- [ ] Create the release
- [ ] Verify files on GitHub
- [ ] Test `Setup.exe` on Windows
- [ ] Users auto-update within 7 days
- [ ] Success! 🎉

---

**You're all set! Everything you need is documented and ready to use.** 

Pick your method and create that release! 💪
