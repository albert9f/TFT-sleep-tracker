using System.Text.Json;

namespace TFTSleepTracker.Core.Storage;

/// <summary>
/// Application settings stored in %ProgramData%\TFTSleepTracker\settings.json
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Discord bot host URL (e.g., "https://bot.example.com")
    /// </summary>
    public string BotHost { get; set; } = "http://35.212.220.200";

    /// <summary>
    /// Static authentication token for bot endpoint
    /// </summary>
    public string Token { get; set; } = "weatheryETHAN";

    /// <summary>
    /// Unique device identifier (auto-generated on first run)
    /// </summary>
    public string DeviceId { get; set; } = "";

    /// <summary>
    /// Last update check timestamp (ISO 8601)
    /// </summary>
    public DateTimeOffset? LastUpdateCheck { get; set; }

    /// <summary>
    /// Last date when "no sleep" message was sent (yyyy-MM-dd)
    /// </summary>
    public string? LastNoSleepMessageDate { get; set; }
}

/// <summary>
/// Manages application settings persistence
/// </summary>
public class AppSettingsStore
{
    private readonly string _settingsDirectory;
    private readonly string _settingsFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettingsStore(string settingsDirectory)
    {
        _settingsDirectory = settingsDirectory ?? throw new ArgumentNullException(nameof(settingsDirectory));
        _settingsFilePath = Path.Combine(_settingsDirectory, "settings.json");
        EnsureDirectoryExists();
    }

    /// <summary>
    /// Loads settings from disk, or returns default settings if file doesn't exist
    /// </summary>
    public async Task<AppSettings> LoadAsync()
    {
        if (!File.Exists(_settingsFilePath))
        {
            var defaultSettings = new AppSettings
            {
                DeviceId = GenerateDeviceId()
            };
            await SaveAsync(defaultSettings);
            return defaultSettings;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            
            // Ensure device ID exists
            if (settings != null && string.IsNullOrEmpty(settings.DeviceId))
            {
                settings.DeviceId = GenerateDeviceId();
                await SaveAsync(settings);
            }

            return settings ?? new AppSettings { DeviceId = GenerateDeviceId() };
        }
        catch
        {
            // If file is corrupted, return default settings
            return new AppSettings { DeviceId = GenerateDeviceId() };
        }
    }

    /// <summary>
    /// Saves settings to disk atomically
    /// </summary>
    public async Task SaveAsync(AppSettings settings)
    {
        var tempFilePath = _settingsFilePath + ".tmp";

        try
        {
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            await File.WriteAllTextAsync(tempFilePath, json);

            // Atomic move
            File.Move(tempFilePath, _settingsFilePath, overwrite: true);
        }
        catch
        {
            // Clean up temp file if something went wrong
            if (File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            throw;
        }
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_settingsDirectory))
        {
            Directory.CreateDirectory(_settingsDirectory);
        }
    }

    private static string GenerateDeviceId()
    {
        return $"device-{Guid.NewGuid():N}";
    }
}
