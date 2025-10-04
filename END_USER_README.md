# 🌙 TFT Sleep Tracker - Get Your Rest, Hextech Style!

Welcome to the **TFT Sleep Tracker**, your personal sleep companion that helps you monitor your rest during those crucial nighttime hours! Whether you're climbing ranked or just want to optimize your sleep schedule, this friendly app has your back.

## 🌟 What Does This App Do?

TFT Sleep Tracker automatically monitors when you're inactive on your computer between **11:00 PM and 8:00 AM** (your local time). It uses this information to estimate how much sleep you're getting each night. 

**Here's the magic:**
- 🔒 **Privacy First**: We only track *when* you're inactive - never *what* you're doing. No keystrokes, no screen content, nothing personal.
- ⏱️ **Smart Detection**: The app waits for 60 continuous minutes of inactivity before counting it as "sleep" to avoid false positives.
- 📊 **Daily Summaries**: Each morning at 8:05 AM, you get a tally of your sleep time from the previous night.
- ☁️ **Optional Sync**: Share your stats with friends via our Discord bot integration (totally optional!).

## 🛡️ Privacy & Data

**What we track:**
- Time when you're inactive on your keyboard and mouse
- Duration of inactivity periods during nighttime hours (11 PM - 8 AM)
- Daily total sleep minutes

**What we DON'T track:**
- ❌ Keystrokes or what you type
- ❌ Screenshots or what's on your screen
- ❌ Application names or what you're running
- ❌ Mouse movements or click patterns
- ❌ Web browsing history
- ❌ Any personal information beyond a device ID

**Where is my data stored?**
- **Locally on your PC**: All detailed activity logs are stored in CSV files at `%ProgramData%\TFTSleepTracker\data`
- **Discord Bot (Optional)**: Only your daily total (e.g., "480 minutes of sleep") is sent to the bot, along with a date and anonymous device ID
- **Never in the cloud**: We don't store your data on external servers (except via the optional Discord bot)

## 💾 Installation

Getting started is super easy!

### Step 1: Download
1. Head to our [GitHub Releases](https://github.com/albert9f/TFT-sleep-tracker/releases) page
2. Download the latest installer (it'll be named something like `TFTSleepTrackerSetup-1.0.0.exe`)

### Step 2: Install
1. Double-click the installer
2. Follow the on-screen prompts (it's just a couple of clicks!)
3. The app will install to your user directory automatically

### Step 3: Done!
That's it! The app will:
- ✅ Start automatically
- ✅ Add itself to Windows startup (so it tracks every night)
- ✅ Appear in your system tray as a small icon

## 🚀 First Run

When you first launch TFT Sleep Tracker:

1. **System Tray Icon**: Look for the app icon in your system tray (bottom-right of your screen, near the clock)
2. **No Setup Needed**: The app starts tracking immediately with sensible defaults
3. **Auto-Start Enabled**: By default, the app will start with Windows so you never miss a night

## 🎮 How to Use

### The 60-Minute Rule

TFT Sleep Tracker uses a smart rule to avoid counting short breaks as sleep:

**If you're inactive for 60+ continuous minutes during the nighttime window [11 PM, 8 AM), it counts as sleep.**

Here's how it works:
- 😴 **Idle 11:00 PM to 7:00 AM** → Counts as 8 hours (after subtracting the first 60 minutes)
- ⚡ **Idle 11:00 PM to 1:00 AM, active 5 min, idle 1:05 AM to 8:00 AM** → Both spans count (after subtracting 60 min from each)
- 🎮 **Idle for only 45 minutes** → Doesn't count (under the threshold)

The 60-minute threshold is applied *inside* the nighttime window to each continuous span of inactivity. This ensures we're only counting real sleep, not just AFK moments!

### System Tray Menu

Right-click the tray icon to access these options:

#### 📂 Open
Opens the main window where you can see your current stats and settings.

#### 📤 Send Now
Manually trigger a summary upload to the Discord bot. This calculates yesterday's sleep and sends it right away (useful if you want to check your stats before 8:05 AM).

#### 🔄 Check for Updates
Checks GitHub for new versions. If an update is available, it downloads and installs automatically.

#### 🚪 Quit Background
Stops the app completely. You'll see a confirmation dialog to make sure you really want to exit. (Note: Tracking stops until you restart the app!)

### Auto-Start with Windows

By default, the app starts automatically when you log into Windows. You can toggle this:

1. Open the main window (right-click tray icon → Open)
2. Check or uncheck **"Start with Windows"**
3. Done!

The app uses the Windows registry to manage auto-start, so it's 100% compatible with your system.

## 🔧 Configuration

### Discord Bot Integration (Optional)

If you want to share your sleep stats with friends or track them on Discord, you'll need to configure the bot connection:

1. **Find the settings file**: 
   - Press `Win + R`
   - Type: `%ProgramData%\TFTSleepTracker`
   - Open `settings.json` in Notepad

2. **Edit the settings**:
   ```json
   {
     "botHost": "https://your-discord-bot.example.com",
     "token": "your-static-authentication-token",
     "deviceId": "device-a1b2c3d4e5f6",
     "lastUpdateCheck": "2024-01-15T08:05:00Z"
   }
   ```

3. **What to configure**:
   - `botHost`: The URL of your Discord bot server
   - `token`: Your unique authentication token (ask your bot admin!)
   - `deviceId`: Auto-generated, leave it as-is
   - `lastUpdateCheck`: Auto-managed, leave it as-is

4. **Save and restart** the app

Once configured, your daily summaries will automatically upload to the bot at 8:05 AM each morning!

### Verify Your Data on Discord

After configuring the bot:
1. Open Discord
2. Type `/sleep` in your server
3. See your tracked sleep stats!

## 📊 Data Files

All your data is stored locally on your PC:

- **Activity Logs**: `%ProgramData%\TFTSleepTracker\data\YYYY-MM-DD.csv`
  - Detailed logs of every check (every 30 seconds)
  - Columns: Timestamp, IsActive, InactivityMinutes, SleepMinutesIncrement

- **Daily Summaries**: `%APPDATA%\TFTSleepTracker\summary.json`
  - Historical daily totals
  - JSON format for easy parsing

- **Upload Queue**: `%ProgramData%\TFTSleepTracker\queue\*.json`
  - Pending uploads to Discord bot
  - Automatically retried if upload fails

- **Settings**: `%ProgramData%\TFTSleepTracker\settings.json`
  - Your bot configuration
  - Device ID and update timestamps

## 🔄 Updates

TFT Sleep Tracker checks for updates **once a week** and applies them silently in the background. You don't need to do anything!

**How it works:**
1. Every 7 days, the app checks GitHub for new releases
2. If an update is found, it downloads automatically
3. The update installs the next time you restart your PC (or manually restart the app)
4. Your settings and data are preserved

**Want to check for updates now?**
- Right-click the tray icon → Check for Updates

## ❓ Troubleshooting

### App Isn't Tracking
- **Check the tray icon**: Is it running? If not, launch it from the Start Menu
- **Auto-start disabled**: Open main window and enable "Start with Windows"
- **CSV files empty**: Wait 30 seconds for the first check to occur

### Data Not Uploading to Discord Bot
- **Check settings.json**: Make sure `botHost` and `token` are correct
- **Network issues**: Check your internet connection
- **Bot server down**: Ask your bot admin to check the server status
- **Check queue directory**: Pending uploads will be in `%ProgramData%\TFTSleepTracker\queue\`

### App Won't Start
- **Install .NET 8.0**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Check Windows version**: Requires Windows 10 (1809+) or Windows 11
- **Check Windows Event Log**: Application → TFTSleepTracker for error details

### Inaccurate Sleep Tracking
- **Are you using RDP?**: The app tracks input on the machine where it's running
- **Multiple users?**: Each Windows user account is tracked separately
- **Laptop sleep/hibernate?**: The app pauses tracking during system sleep (by design)
- **Time zone changes**: Sleep is calculated in local time, so DST transitions are handled automatically

### Privacy Concerns
- **Review the logs**: Open any CSV file to see exactly what's recorded (just timestamps and idle durations)
- **Disable Discord upload**: Delete or rename `settings.json` to stop all uploads
- **Uninstall completely**: Use Windows "Add or Remove Programs" and delete data folders manually if desired

## 🌌 About TFT Sleep Tracker

This app is inspired by the strategic depth and hextech magic of Teamfight Tactics! We believe that good sleep is just as important as good positioning in your TFT games. 

**Made with ❤️ by the community, for the community.**

### Contributing
Found a bug? Have a feature idea? We'd love to hear from you!
- [Open an issue on GitHub](https://github.com/albert9f/TFT-sleep-tracker/issues)
- [Submit a pull request](https://github.com/albert9f/TFT-sleep-tracker/pulls)

### License
This project is open source under the MIT License. Feel free to use, modify, and share!

### Support
Need help? Have questions?
- 📖 Check the [Developer README](README.md) for technical details
- 🐛 [Report issues on GitHub](https://github.com/albert9f/TFT-sleep-tracker/issues)
- 📋 Review the [QA Checklist](docs/QA_CHECKLIST.md) for manual testing scenarios

---

**Happy tracking, and sweet dreams!** 🌙✨

*Disclaimer: This app is not affiliated with Riot Games. TFT and Teamfight Tactics are trademarks of Riot Games.*
