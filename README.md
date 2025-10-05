# TFT Sleep Tracker

A Windows desktop application that mo### Discord Sync Configuration

The app automatically syncs sleep data to Discord every hour. To configure:

1. Edit `%ProgramData%\TFTSleepTracker\settings.json`
2. Set `botHost` to your Discord bot's URL
3. Set `token` to your authentication token
4. The app will automatically upload summaries every hour at :00 (e.g., 1:00 PM, 2:00 PM)ser activity to track sleep patterns during nighttime hours (11 PM - 8 AM).

## Features

- **Automatic Activity Tracking**: Monitors keyboard and mouse activity to detect inactivity periods
- **Daily Sleep Summaries**: Calculates total sleep minutes based on inactivity during nightly windows
- **System Tray Integration**: Runs quietly in the background with a system tray icon
- **Autostart**: Automatically starts with Windows login
- **Activity Tracking**: Monitors keyboard and mouse input every 30 seconds to determine active/inactive periods
- **Smart Sleep Detection**: Uses a configurable nightly window (default 11 PM – 8 AM) and 5-minute inactivity threshold
- **Automatic Hourly Sync**: Sends complete daily summaries to Discord every hour on the hour
- **CSV Logging**: Stores all activity data locally in `%AppData%\TFTSleepTracker\activity_*.csv`
- **Upload Queue**: Queues sleep summaries for upload to a Discord bot endpoint with retry logic
- **Auto-Update**: Checks for app updates daily using Squirrel (Windows only)
- **System Tray**: Runs in the background with a tray icon for easy access
- **Auto-Update**: Checks for updates weekly via Squirrel.Windows and applies them silently
- **Robustness**: Handles hibernation, RDP sessions, DST transitions, and file IO errors gracefully

## Installation

### From Release Package

1. Download the latest installer from [GitHub Releases](https://github.com/albert9f/TFT-sleep-tracker/releases)
2. Run the installer
3. The app will start automatically and add itself to Windows startup

### From Source

```bash
# Clone the repository
git clone https://github.com/albert9f/TFT-sleep-tracker.git
cd TFT-sleep-tracker

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run the app
dotnet run --project TFTSleepTracker.App
```

## Configuration

Settings are stored in `%ProgramData%\TFTSleepTracker\settings.json`:

```json
{
  "botHost": "https://your-bot-host.com",
  "token": "your-static-token",
  "deviceId": "auto-generated-device-id",
  "lastUpdateCheck": "2024-01-15T12:00:00Z"
}
```

### Upload Configuration

To enable sleep summary uploads:

1. Open `%ProgramData%\TFTSleepTracker\settings.json`
2. Set `botHost` to your Discord bot's URL
3. Set `token` to your static authentication token
4. The app will automatically upload summaries at 8:05 AM daily

## Data Storage

- **Activity Data**: `%APPDATA%\TFTSleepTracker\YYYY-MM-DD.csv`
- **Daily Summaries**: `%APPDATA%\TFTSleepTracker\summary.json`
- **Upload Queue**: `%ProgramData%\TFTSleepTracker\queue\*.json`
- **Settings**: `%ProgramData%\TFTSleepTracker\settings.json`

## Development

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11 (required for Windows-specific APIs)
- Visual Studio 2022 or later (optional)

### Build & Test

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test

# Run the app
dotnet run --project TFTSleepTracker.App
```

### Project Structure

```
TFTSleepTracker/
├── TFTSleepTracker.App/         # WPF application
│   ├── App.xaml.cs               # Application lifecycle
│   ├── MainWindow.xaml           # UI
│   ├── AutostartHelper.cs        # Registry autostart management
│   └── DailySummaryScheduler.cs  # Daily 8:05 AM scheduler
├── TFTSleepTracker.Core/         # Core logic library
│   ├── Storage/                  # Data persistence
│   │   ├── CsvLogger.cs          # Activity CSV logging
│   │   ├── SummaryStore.cs       # Daily summary persistence
│   │   ├── AppSettings.cs        # Configuration management
│   │   ├── UploadQueue.cs        # Upload queue management
│   │   └── FileRetryHelper.cs    # File IO retry logic
│   ├── Logic/                    # Business logic
│   │   ├── SleepCalculator.cs    # Sleep computation
│   │   └── SystemEventsHandler.cs # Power/session event handling
│   ├── Net/                      # Network operations
│   │   ├── UploadService.cs      # HTTP upload client
│   │   └── UploadQueueProcessor.cs # Background queue processor
│   └── Update/                   # Auto-update
│       └── UpdateService.cs      # Squirrel.Windows integration
└── TFTSleepTracker.Tests/        # Unit tests
```

## Packaging & Distribution

### Creating a Windows Installer

Use the Squirrel packaging script to create a Windows installer:

```powershell
# Quick start - create installer
cd scripts
.\pack-squirrel.ps1 -Version "1.0.0"

# Skip tests for faster build
.\pack-squirrel.ps1 -Version "1.0.0" -SkipTests

# Auto-upload to GitHub Releases
.\pack-squirrel.ps1 -Version "1.0.0" -Upload
```

**Output**: Find `Setup.exe` in the `dist/` folder - distribute this to users!

**Prerequisites**:
- .NET 8 SDK
- Squirrel CLI: `dotnet tool install --global Squirrel`
- (Optional) GitHub CLI for auto-upload: https://cli.github.com/

📖 **See [PACKAGING.md](PACKAGING.md) for detailed instructions**  
⚡ **See [PACKAGING_QUICKREF.md](PACKAGING_QUICKREF.md) for quick reference**

### Alternative: Clowd.Squirrel

The repository also includes `pack.ps1` which uses Clowd.Squirrel (a Squirrel fork):

```powershell
.\scripts\pack.ps1 -Version "1.0.0"
```

## CI/CD

The repository includes a GitHub Actions workflow that:
- Builds the solution on `windows-latest` runners
- Runs all tests
- Uploads build artifacts

See `.github/workflows/build.yml` for details.

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Support

For issues, questions, or feature requests, please open an issue on GitHub.

