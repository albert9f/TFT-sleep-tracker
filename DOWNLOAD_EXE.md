# 📥 How to Get the Executable

## Option 1: Download via VS Code (Easiest)

1. In VS Code, navigate to the file explorer
2. Go to: `release/TFTSleepTracker.exe`
3. Right-click the file
4. Select "Download..."
5. Save it to your local machine

## Option 2: Command Line Download

If you're using GitHub Codespaces or a remote dev container:

```bash
# From the VS Code terminal, you can copy it to your workspace
# Then download from VS Code's file explorer
ls -lh /workspaces/TFT-sleep-tracker/release/TFTSleepTracker.exe
```

## Option 3: Commit and Push (For Version Control)

If you want to track this in git:

```bash
cd /workspaces/TFT-sleep-tracker

# Add the release folder to git (it might be in .gitignore)
git add -f release/TFTSleepTracker.exe

# Commit it
git commit -m "Build: Add single executable v1.0.0 with hourly Discord sync"

# Push to your repo
git push
```

Then download from GitHub directly.

## Option 4: GitHub Release (Professional)

Create a GitHub release:

```bash
# Make sure you have gh CLI installed and authenticated
gh release create v1.0.0 \
  release/TFTSleepTracker.exe \
  --title "TFT Sleep Tracker v1.0.0" \
  --notes "First release with automatic hourly Discord sync"
```

Then download from: https://github.com/albert9f/TFT-sleep-tracker/releases

## File Details

```
Filename: TFTSleepTracker.exe
Size:     162 MB
Platform: Windows x64
Type:     Self-contained executable
Runtime:  .NET 8.0 included
```

## Verification

After downloading, verify the file:

### Windows:
1. Right-click → Properties
2. Check file size (should be ~162 MB)
3. Check "Digital Signatures" tab (if you signed it)

### Linux/Mac:
```bash
ls -lh TFTSleepTracker.exe
file TFTSleepTracker.exe
```

Should show: `PE32+ executable (GUI) x86-64, for MS Windows`

## Next Steps After Download

1. ✅ Transfer to a Windows machine
2. ✅ Test it yourself first
3. ✅ Send to client with `CLIENT_QUICK_START.md`
4. ✅ Provide Discord bot credentials

## Security Note

Windows Defender SmartScreen will likely warn about this file because:
- It's a new executable
- It's not code-signed
- It hasn't been downloaded by many users yet

**Solution for client**: Click "More info" → "Run anyway"

**Optional**: Get a code signing certificate to avoid this warning
- Cost: ~$100-400/year
- Benefit: No SmartScreen warnings
- See: https://docs.microsoft.com/en-us/windows/win32/seccrypto/using-signtool-to-sign-a-file

---

**Your executable is ready at**: `/workspaces/TFT-sleep-tracker/release/TFTSleepTracker.exe`

Just download it and send it to your client! 🚀
