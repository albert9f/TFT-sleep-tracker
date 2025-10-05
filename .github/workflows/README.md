# Automated Squirrel Releases with GitHub Actions

## Overview

This workflow automates the entire Squirrel release process using GitHub Actions. It runs on GitHub's cloud infrastructure (Windows VM), so you don't need a local Windows machine.

## ✅ What It Does

1. ✅ Builds your WPF application on Windows
2. ✅ Runs all tests
3. ✅ Creates NuGet package with Squirrel
4. ✅ Generates installer (Setup.exe)
5. ✅ Creates delta packages
6. ✅ Generates RELEASES manifest
7. ✅ Uploads everything to GitHub Releases
8. ✅ **All automatically!**

## 🚀 How to Use

### Option 1: Push a Version Tag (Recommended)

```bash
# Tag your commit with version number
git tag v1.0.1
git push origin v1.0.1
```

**That's it!** GitHub Actions will:
- Detect the tag
- Extract version (1.0.1)
- Build and package
- Create release automatically

### Option 2: Manual Trigger from GitHub UI

1. Go to: https://github.com/albert9f/TFT-sleep-tracker/actions
2. Select "Build and Release with Squirrel" workflow
3. Click "Run workflow"
4. Enter version number (e.g., `1.0.1`)
5. Click "Run workflow"

GitHub will build and release automatically!

## 📋 Prerequisites (One-Time Setup)

### 1. Enable GitHub Actions

File is already created at `.github/workflows/squirrel-release.yml`

Just commit and push it:

```bash
git add .github/workflows/squirrel-release.yml
git commit -m "Add automated Squirrel release workflow"
git push
```

### 2. Verify Permissions

Go to: Settings → Actions → General

Ensure:
- ✅ "Allow GitHub Actions to create and approve pull requests" is enabled
- ✅ Workflow permissions: "Read and write permissions"

That's all! No secrets needed (uses built-in `GITHUB_TOKEN`).

## 🎯 Release Process (Automated)

### Before GitHub Actions:
```
Developer's Windows Machine:
1. Pull code
2. Build solution
3. Run tests
4. Publish app
5. Create NuGet package
6. Run Squirrel releasify
7. Upload to GitHub
```

**Time: ~10-15 minutes**  
**Requires: Windows machine, manual steps**

### With GitHub Actions:
```
Developer:
1. git tag v1.0.1
2. git push origin v1.0.1
```

**Time: ~5-10 minutes (automated)**  
**Requires: Nothing! Runs in the cloud**

## 📊 Workflow Stages

The workflow consists of these stages:

### Stage 1: Setup
- Checkout code
- Install .NET SDK
- Install Squirrel CLI
- Determine version from tag

### Stage 2: Build
- Restore NuGet packages
- Build in Release mode
- Run all tests
- Publish as self-contained executable

### Stage 3: Package
- Create NuGet package with Squirrel
- Generate installer files
- Create delta packages

### Stage 4: Release
- Create GitHub Release
- Upload Setup.exe
- Upload RELEASES file
- Upload .nupkg packages

### Stage 5: Notify
- Report success/failure
- Upload artifacts for debugging

## 🎓 Example Usage

### Release Version 1.0.1

```bash
# 1. Make your changes
git add .
git commit -m "Fix bug in sleep calculation"

# 2. Create version tag
git tag -a v1.0.1 -m "Release version 1.0.1"

# 3. Push tag
git push origin v1.0.1

# 4. Watch the magic happen!
# Go to: https://github.com/albert9f/TFT-sleep-tracker/actions
```

GitHub Actions will:
1. Start automatically (within seconds)
2. Build on Windows VM (~3 minutes)
3. Run tests (~1 minute)
4. Create packages (~2 minutes)
5. Create release (~1 minute)
6. Done! (~7 minutes total)

### Release Version 1.0.2

```bash
git tag v1.0.2
git push origin v1.0.2
```

That's it! Same process, fully automated.

## 🔍 Monitoring Progress

### View Workflow Run

1. Go to: https://github.com/albert9f/TFT-sleep-tracker/actions
2. Click on the running workflow
3. Watch live progress
4. See logs for each step

### Check Release

Once complete, go to:
https://github.com/albert9f/TFT-sleep-tracker/releases

You'll see your new release with all files!

## ✅ Verification

After workflow completes, verify the release has:

- [ ] `Setup.exe` - Installer
- [ ] `RELEASES` - Manifest
- [ ] `TFTSleepTracker-{version}-full.nupkg` - Full package
- [ ] `TFTSleepTracker-{version}-delta.nupkg` - Delta (if applicable)

## 🐛 Troubleshooting

### Workflow Fails on Tests

The workflow runs all tests. If tests fail, the release is aborted.

**Solution**: Fix tests locally, then re-tag:

```bash
# Fix tests
git add .
git commit -m "Fix failing tests"

# Delete old tag
git tag -d v1.0.1
git push --delete origin v1.0.1

# Create new tag
git tag v1.0.1
git push origin v1.0.1
```

### Workflow Fails on Build

Check the build logs in GitHub Actions to see the error.

Common issues:
- Missing dependencies
- .csproj version mismatch
- Code compilation errors

### Manual Trigger Doesn't Show

Ensure you've pushed the workflow file:

```bash
git add .github/workflows/squirrel-release.yml
git commit -m "Add workflow"
git push
```

Then refresh: https://github.com/albert9f/TFT-sleep-tracker/actions

## ⚙️ Configuration

### Change .NET Version

Edit `.github/workflows/squirrel-release.yml`:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'  # Change to '9.0.x' if needed
```

### Skip Tests

If you want to skip tests (not recommended):

```yaml
# Comment out or remove this step
# - name: Run tests
#   run: dotnet test --configuration Release --no-build --verbosity normal
```

### Custom Release Notes

Edit the `body:` section in the workflow:

```yaml
body: |
  ## Custom Release Notes
  
  Your custom message here!
```

## 📦 Artifacts

The workflow saves build artifacts for 30 days, even if the release fails.

To download:
1. Go to workflow run
2. Scroll to "Artifacts" section
3. Download `release-artifacts-{version}.zip`

Contains:
- Setup.exe
- RELEASES
- All .nupkg files

## 🆚 Comparison: Manual vs Automated

| Aspect | Manual (PowerShell Script) | Automated (GitHub Actions) |
|--------|----------------------------|----------------------------|
| **Requires Windows** | ✅ Yes | ❌ No (runs in cloud) |
| **Manual steps** | Multiple | One (push tag) |
| **Time** | ~10-15 min | ~7 min |
| **Reliability** | Depends on local setup | Consistent environment |
| **Logs** | Local terminal | Saved in GitHub |
| **Rollback** | Manual | Easy (delete tag/release) |
| **Cost** | Free (your machine) | Free (GitHub Actions) |

## 🎯 Recommendation

### Use GitHub Actions when:
- ✅ You don't have Windows locally
- ✅ You want consistent builds
- ✅ You want automated releases
- ✅ You want to save time

### Use PowerShell script when:
- ✅ You want local testing before release
- ✅ You want full control over each step
- ✅ GitHub Actions is unavailable
- ✅ You prefer manual process

**Best of both worlds**: Use PowerShell for testing, GitHub Actions for releases!

## 🚀 Quick Start for 1.0.1

If you want to use GitHub Actions for 1.0.1:

```bash
# 1. Commit the workflow file
git add .github/workflows/squirrel-release.yml
git commit -m "Add automated release workflow"
git push

# 2. Create and push tag
git tag v1.0.1
git push origin v1.0.1

# 3. Watch it work!
# Visit: https://github.com/albert9f/TFT-sleep-tracker/actions
```

In ~7 minutes, you'll have a complete release with all Squirrel packages!

## ⚡ Future Releases

Every future release is now just:

```bash
git tag v1.0.2
git push origin v1.0.2
```

That's it! Fully automated. 🎉

## 📚 Additional Resources

- GitHub Actions Docs: https://docs.github.com/en/actions
- Squirrel.Windows: https://github.com/Squirrel/Squirrel.Windows
- .NET SDK Action: https://github.com/actions/setup-dotnet

---

**Bottom Line**: GitHub Actions automates everything, saving you time and ensuring consistent releases.

For 1.0.1, you can either:
1. Use GitHub Actions (push `v1.0.1` tag)
2. Use PowerShell script locally

Both create the same Squirrel packages! 🚀
