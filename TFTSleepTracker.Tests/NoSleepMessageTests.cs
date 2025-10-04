using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using TFTSleepTracker.Core.Storage;

namespace TFTSleepTracker.Tests;

/// <summary>
/// Tests for the no-sleep message feature
/// </summary>
public class NoSleepMessageTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly UploadQueue _queue;

    public NoSleepMessageTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_nosleep_{Guid.NewGuid()}");
        _queue = new UploadQueue(_testDirectory);
    }

    [Fact]
    public async Task QueuedUpload_SupportsOptionalMessage()
    {
        // Arrange
        var upload = new QueuedUpload
        {
            DeviceId = "test-device",
            Date = "2024-01-15",
            SleepMinutes = 0,
            ComputedAt = DateTimeOffset.Now,
            Message = "looks like ethan didnt sleep tonight"
        };

        // Act
        await _queue.EnqueueAsync(upload);
        var file = _queue.GetOldestFile();
        var result = await _queue.ReadFileAsync(file!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.SleepMinutes);
        Assert.Equal("looks like ethan didnt sleep tonight", result.Message);
    }

    [Fact]
    public async Task QueuedUpload_MessageCanBeNull()
    {
        // Arrange
        var upload = new QueuedUpload
        {
            DeviceId = "test-device",
            Date = "2024-01-15",
            SleepMinutes = 480,
            ComputedAt = DateTimeOffset.Now,
            Message = null
        };

        // Act
        await _queue.EnqueueAsync(upload);
        var file = _queue.GetOldestFile();
        var result = await _queue.ReadFileAsync(file!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(480, result.SleepMinutes);
        Assert.Null(result.Message);
    }

    [Fact]
    public async Task AppSettings_SupportsLastNoSleepMessageDate()
    {
        // Arrange
        var settingsDir = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}");
        var settingsStore = new AppSettingsStore(settingsDir);

        try
        {
            // Act - Load default settings
            var settings = await settingsStore.LoadAsync();
            Assert.Null(settings.LastNoSleepMessageDate);

            // Act - Save with LastNoSleepMessageDate
            settings.LastNoSleepMessageDate = "2024-01-15";
            await settingsStore.SaveAsync(settings);

            // Act - Reload and verify
            var reloadedSettings = await settingsStore.LoadAsync();

            // Assert
            Assert.Equal("2024-01-15", reloadedSettings.LastNoSleepMessageDate);
        }
        finally
        {
            if (Directory.Exists(settingsDir))
            {
                Directory.Delete(settingsDir, true);
            }
        }
    }

    [Fact]
    public void DateOnly_DayNumber_CalculatesDayDifference()
    {
        // Arrange
        var date1 = new DateOnly(2024, 1, 15);
        var date2 = new DateOnly(2024, 1, 22); // 7 days later
        var date3 = new DateOnly(2024, 1, 23); // 8 days later

        // Act & Assert
        Assert.Equal(7, date2.DayNumber - date1.DayNumber);
        Assert.Equal(8, date3.DayNumber - date1.DayNumber);
        Assert.True((date2.DayNumber - date1.DayNumber) >= 7);
        Assert.True((date3.DayNumber - date1.DayNumber) >= 7);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
