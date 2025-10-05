# Test Button - Quick Reference

## 🚀 One-Liner

Click **"🧪 Send Test Data to Discord"** to verify your Discord bot integration is working.

## ⚡ Quick Start

```bash
1. Open TFTSleepTracker.exe
2. Scroll to bottom
3. Click test button
4. Check result message
```

## 📤 What Gets Sent

```json
{
  "deviceId": "your-device-id",
  "date": "2001-09-[random 1-30]",
  "sleepMinutes": 120,
  "computedAt": "2025-10-04T21:52:00Z"
}
```

## ✅ Success

```
✅ Success! Sent 2 hours of sleep for September 15, 2001
```
→ Everything works! Check Discord.

## ❌ Common Errors

| Error | Fix |
|-------|-----|
| Discord bot URL not configured | Edit `C:\ProgramData\TFTSleepTracker\settings.json` |
| Upload failed | Check bot URL and token |
| Network error | Check internet connection |

## 🎲 Random Dates

Each click generates a **random day** in September 2001:
- 2001-09-01 to 2001-09-30
- Always 2 hours (120 minutes)
- Safe fake data for testing

## 📁 Files

| File | Location |
|------|----------|
| Executable | `release/TFTSleepTracker.exe` |
| Settings | `C:\ProgramData\TFTSleepTracker\settings.json` |
| Icon | `TFTSleepTracker.App/app.ico` |
| Guide | `TEST_BUTTON_UPDATE.md` |

## 🔧 Configuration Required

Before testing, configure:

```json
{
  "botHost": "https://your-discord-bot.com",
  "token": "your-secret-token"
}
```

Location: `C:\ProgramData\TFTSleepTracker\settings.json`

## 🎯 Use Cases

- ✅ Verify Discord bot is reachable
- ✅ Test authentication token
- ✅ Check network connectivity
- ✅ Validate payload format
- ✅ Debug integration issues

## 🛡️ Safety

- ✅ Uses fake historical date (2001)
- ✅ Does not interfere with real tracking
- ✅ Can be used unlimited times
- ✅ No real data sent

## 📊 Status Display

Test button shows:
- 🕐 "Sending test data..." (while sending)
- ✅ Green text = success
- ❌ Red text = error
- 💡 Helpful hints for fixing issues

## 🎨 Icon

Custom icon applied:
- ✅ Title bar
- ✅ Taskbar
- ✅ Alt+Tab menu
- ✅ Desktop shortcut (if created)

## 📥 Download

**Location:** `/workspaces/TFT-sleep-tracker/release/TFTSleepTracker.exe`

**How:**
1. Right-click → Download (VS Code)
2. Or commit and download from GitHub

**Size:** 162 MB (self-contained)

## 🎉 Ready?

```bash
✓ Downloaded .exe
✓ Configured settings.json
✓ Opened app
✓ Clicked test button
✓ Saw success message
→ You're done! ✅
```

---

**For detailed info, see:** `TEST_BUTTON_UPDATE.md`
