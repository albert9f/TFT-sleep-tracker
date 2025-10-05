# Test Button Update - October 4, 2025

## ✅ What Was Added

A **Test Button** has been added to the TFT Sleep Tracker application to verify Discord integration is working correctly.

## 🎯 What It Does

When you click the **"🧪 Send Test Data to Discord"** button:

1. Generates a **random date** in September 2001 (between day 1-30)
2. Creates a **fake sleep session** of exactly **2 hours (120 minutes)**
3. Sends the data to your configured Discord bot
4. Shows success or error message

## 📍 Where to Find It

The test button is located at the bottom of the main window, in a new "Testing" section below the Settings card.

## 🔧 How to Use It

### Step 1: Configure Discord Bot Settings

Before testing, make sure your Discord bot is configured:

1. Navigate to: `C:\ProgramData\TFTSleepTracker\settings.json`
2. Set your bot URL and token:
   ```json
   {
     "botHost": "https://your-discord-bot.com",
     "token": "your-secret-token",
     "deviceId": "auto-generated-id"
   }
   ```
3. Save the file

### Step 2: Test the Connection

1. Open **TFT Sleep Tracker** application
2. Scroll down to the **Testing** section
3. Click the **"🧪 Send Test Data to Discord"** button
4. Wait for the result message

### Step 3: Interpret Results

**✅ Success Message:**
```
✅ Success! Sent 2 hours of sleep for September [XX], 2001
```
This means:
- Discord bot URL is correct
- Token is valid
- Network connection is working
- Bot received the data

**❌ Error Messages:**

```
❌ Error: Discord bot URL not configured. Check settings.json
```
→ You need to configure `botHost` in settings.json

```
❌ Upload failed. Check bot URL and token in settings.json
```
→ Bot URL is incorrect or token is invalid

```
❌ Error: [specific error message]
```
→ Network or other technical issue

## 📊 Test Data Format

The test button sends this exact payload:

```json
{
  "deviceId": "your-device-id",
  "date": "2001-09-XX",
  "sleepMinutes": 120,
  "computedAt": "2025-10-04T21:52:00Z"
}
```

Where:
- `date`: Random day in September 2001 (e.g., 2001-09-15)
- `sleepMinutes`: Always 120 (2 hours)
- `computedAt`: Current UTC timestamp

## 🎨 Icon Update

The application now uses the **custom icon** you provided:

- **File**: `app.ico` (converted from `the icon.png`)
- **Location**: Application executable and window title bar
- **Sizes**: Multiple resolutions for Windows (16x16 to 256x256)

## 📦 Updated Files

### New Files:
- `TFTSleepTracker.App/app.ico` - Application icon
- `TFTSleepTracker.App/app.png` - Original icon (backup)

### Modified Files:
- `TFTSleepTracker.App/MainWindow.xaml` - Added test button UI
- `TFTSleepTracker.App/MainWindow.xaml.cs` - Added test button logic
- `TFTSleepTracker.App/TFTSleepTracker.App.csproj` - Added icon reference

### Rebuilt:
- `release/TFTSleepTracker.exe` - Fresh build with test button and icon

## 🧪 Testing Checklist

Use this checklist when testing:

- [ ] Application opens successfully
- [ ] Custom icon appears in title bar
- [ ] Custom icon appears in taskbar
- [ ] Test button is visible at bottom of window
- [ ] Click test button shows "Sending test data..."
- [ ] Success message appears (if bot configured)
- [ ] Check Discord bot received the data
- [ ] Date is random each time you click
- [ ] Sleep minutes is always 120 (2 hours)

## 🔒 Privacy Note

The test data:
- ✅ Uses fake date (September 2001)
- ✅ Uses fixed sleep time (2 hours)
- ✅ Does not send real data
- ✅ Does not interfere with real tracking
- ✅ Safe to use unlimited times

## 🚀 Ready to Ship

The new executable is ready at:
```
/workspaces/TFT-sleep-tracker/release/TFTSleepTracker.exe
```

**Size:** 162 MB (self-contained, includes .NET 8.0 runtime)  
**Icon:** ✓ Included  
**Test Button:** ✓ Included  
**Platform:** Windows x64  
**Dependencies:** None

## 📥 How to Download

### Option 1: VS Code
1. Right-click `release/TFTSleepTracker.exe`
2. Select "Download"
3. Save to your computer

### Option 2: Git Commit
```bash
git add .
git commit -m "Add test button and custom icon"
git push
```
Then download from GitHub.

## 🎯 Next Steps

1. **Download** the new executable
2. **Test** on a Windows machine
3. **Configure** Discord bot settings
4. **Click** the test button
5. **Verify** data appears in Discord
6. **Send** to client with updated documentation

## ❓ FAQ

**Q: Will the test button interfere with real tracking?**  
A: No, it only sends a fake historical date from 2001.

**Q: How many times can I click the test button?**  
A: Unlimited. Each click sends a new random date.

**Q: What if the test fails?**  
A: Check your `settings.json` file for correct bot URL and token.

**Q: Does the test button work offline?**  
A: No, it requires internet connection to reach the Discord bot.

**Q: Can I remove the test button after testing?**  
A: Yes, we can create a version without it for production.

## 🎉 Summary

✅ Test button added  
✅ Custom icon applied  
✅ New executable built  
✅ Ready for testing  
✅ Ready for client delivery  

**You're all set!** 🚀
