# TFT Sleep Tracker - Release Notes

## v1.0.0 - Production Release (October 5, 2025)

### 🎉 Features
- **Single-file executable** - No installation required, just run the .exe
- **Self-contained** - Includes entire .NET 8.0 runtime (71MB)
- **Hardcoded server configuration** - Connects to http://35.212.220.200:8080
- **Automatic hourly sync** - Uploads sleep data every hour at :00
- **Test button** - Send fake September 2001 sleep data to verify connection
- **Custom TFT icon** - Branded application icon
- **Auto-start at login** - Optional Windows startup integration
- **Offline queue** - Stores and retries uploads when disconnected

### 🔧 Technical Changes
- **Snake_case JSON serialization** - Compatible with Python FastAPI backend
- **Detailed error messages** - Shows actual HTTP errors and connection issues
- **Force-override settings** - Always uses hardcoded server values
- **Windows Event Log integration** - Logs important events for debugging

### 📦 What's Included
- `TFTSleepTracker.exe` (71MB) - Single self-contained executable
- No additional files or dependencies required
- Works on any Windows 10/11 x64 PC

### 🚀 Deployment
1. Download `TFTSleepTracker.exe`
2. Run it - that's it!
3. Click "Test" button to verify connection
4. Look for green "✅ Success!" message

### 🔒 Configuration (Hardcoded)
- Server: `http://35.212.220.200:8080`
- Token: `weatheryETHAN`
- Sync interval: Every hour on the :00 mark

### 🐛 Bug Fixes
- Fixed localhost:5000 connection issue (now correctly connects to VM)
- Fixed camelCase/snake_case JSON mismatch with Python API
- Fixed misleading error messages (now shows actual errors)
- Fixed settings override to always use hardcoded values

### 📝 Known Issues
- None currently reported

### 🔮 Future Enhancements
- System tray right-click menu
- Manual sync button in system tray
- Update notification system
- Configurable sync interval (optional)
