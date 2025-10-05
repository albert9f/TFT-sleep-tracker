# Quick Start Guide for Your Client

## Installation & Setup (5 Minutes)

### Step 1: Download and Run
1. Download `TFTSleepTracker.exe`
2. Double-click to run it
3. If Windows Defender blocks it:
   - Click "More info"
   - Click "Run anyway"

### Step 2: Configure Discord Connection
1. Press `Win + R` to open Run dialog
2. Type: `%ProgramData%\TFTSleepTracker`
3. Press Enter
4. Open `settings.json` with Notepad
5. Edit these fields:
   ```json
   {
     "botHost": "YOUR_DISCORD_BOT_URL_HERE",
     "token": "YOUR_TOKEN_HERE",
     "deviceId": "keep-this-value"
   }
   ```
6. Save and close

### Step 3: Restart the App
1. Right-click the tray icon (system tray, bottom-right)
2. Click "Quit Background"
3. Run `TFTSleepTracker.exe` again

### Step 4: Verify It's Working
- Look for the app icon in your system tray (bottom-right corner)
- Open Discord and check your bot channel
- Within the next hour, your data will appear automatically

## What Happens Now?

### Automatic Hourly Sync
- Every hour at :00 (1:00, 2:00, 3:00, etc.), the app sends your sleep data to Discord
- You don't need to do anything - it's completely automatic
- Only complete days are sent (not today)

### Manual Upload
If you want to send data right now:
1. Right-click the tray icon
2. Click "Send Now"
3. Check Discord - your data should appear within seconds

## Troubleshooting

### "App isn't in my system tray"
- Check if it's running: Press `Ctrl + Shift + Esc` to open Task Manager
- Look for "TFTSleepTracker" in the list
- If not running, double-click `TFTSleepTracker.exe` again

### "No data appearing in Discord"
1. Check your settings file:
   - Press `Win + R`, type `%ProgramData%\TFTSleepTracker`, press Enter
   - Open `settings.json` - verify botHost and token are correct
2. Check for queued uploads:
   - In the same folder, open the `queue` subfolder
   - If files are piling up here, there's a network issue
3. Restart the app:
   - Right-click tray icon → Quit Background
   - Run TFTSleepTracker.exe again

### "App won't start"
- Install .NET 8.0 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
- Make sure you have Windows 10 (version 1809 or newer) or Windows 11

## Uninstalling

1. Right-click tray icon → Quit Background
2. Delete `TFTSleepTracker.exe`
3. Delete these folders if you want to remove all data:
   - `%AppData%\TFTSleepTracker` (your sleep logs)
   - `%ProgramData%\TFTSleepTracker` (settings and queue)

## Privacy & Data

### What's Stored Locally:
- **Activity logs**: `%AppData%\TFTSleepTracker\activity_*.csv`
  - Timestamps of when you were active/inactive
  - Never sent to Discord
- **Daily summaries**: `%AppData%\TFTSleepTracker\summaries_*.json`
  - Daily total sleep minutes
  - Used to generate Discord uploads

### What's Sent to Discord:
- Date (e.g., "2025-10-03")
- Total sleep minutes (e.g., "480")
- Anonymous device ID (e.g., "a1b2c3d4-...")
- **That's it!** No keystrokes, no app names, no personal data

## Advanced Settings

### Disable Auto-Start
1. Right-click tray icon → Open
2. Uncheck "Start with Windows"
3. App will only run when you manually start it

### Change Nightly Window
Currently hardcoded to 11 PM - 8 AM. Contact the developer if you need this customized.

## Support

If you run into issues:
1. Check Windows Event Viewer:
   - Press `Win + R`, type `eventvwr`, press Enter
   - Go to Windows Logs → Application
   - Look for "TFTSleepTracker" events
2. Contact your administrator with the error details

---

**That's it!** The app is now tracking your sleep and syncing to Discord every hour automatically. 🎉
