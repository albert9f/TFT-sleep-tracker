# TFT Sleep Tracker

A Windows desktop application that monitors user activity to track sleep patterns during nighttime hours (11 PM - 8 AM).

## Features

- **Automatic Activity Tracking**: Monitors keyboard and mouse activity to detect inactivity periods
- **Daily Sleep Summaries**: Calculates total sleep minutes based on inactivity during nightly windows
- **System Tray Integration**: Runs quietly in the background with a system tray icon
- **Autostart**: Automatically starts with Windows login
- **Upload Queue**: Queues sleep summaries for upload to a Discord bot endpoint with retry logic
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

## Packaging & Releases

### Creating a Release Package

Use the provided PowerShell script to create release packages:

```powershell
# Build and package (local)
.\scripts\pack.ps1 -Version "1.0.0"

# Build, package, and upload to GitHub Releases
.\scripts\pack.ps1 -Version "1.0.0" -Upload
```

The script will:
1. Clean previous builds
2. Restore NuGet packages
3. Build the solution in Release mode
4. Run all tests
5. Publish the app
6. Create a Squirrel release package
7. Optionally upload to GitHub Releases (requires `gh` CLI)

### Manual Packaging

```bash
# Publish the app
dotnet publish TFTSleepTracker.App/TFTSleepTracker.App.csproj \
  --configuration Release \
  --output ./publish

# Install Clowd.Squirrel CLI
dotnet tool install --global Clowd.Squirrel

# Create release package
squirrel pack \
  --packId TFTSleepTracker \
  --packVersion 1.0.0 \
  --packDirectory ./publish \
  --releaseDir ./releases
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

