using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using TFTSleepTracker.Core.Storage;

namespace TFTSleepTracker.Tests;

public class AppSettingsTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly AppSettingsStore _store;

    public AppSettingsTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}");
        _store = new AppSettingsStore(_testDirectory);
    }

    [Fact]
    public async Task LoadAsync_CreatesDefaultSettingsWithDeviceId()
    {
        // Act
        var settings = await _store.LoadAsync();

        // Assert
        Assert.NotNull(settings);
        Assert.NotEmpty(settings.DeviceId);
        Assert.StartsWith("device-", settings.DeviceId);
    }

    [Fact]
    public async Task SaveAsync_PersistsSettings()
    {
        // Arrange
        var settings = new AppSettings
        {
            BotHost = "https://test.example.com",
            Token = "test-token-123",
            DeviceId = "device-xyz"
        };

        // Act
        await _store.SaveAsync(settings);
        var loaded = await _store.LoadAsync();

        // Assert
        Assert.Equal("https://test.example.com", loaded.BotHost);
        Assert.Equal("test-token-123", loaded.Token);
        Assert.Equal("device-xyz", loaded.DeviceId);
    }

    [Fact]
    public async Task LoadAsync_PreservesDeviceIdAcrossLoads()
    {
        // Arrange
        var firstLoad = await _store.LoadAsync();
        var firstDeviceId = firstLoad.DeviceId;

        // Act
        var secondLoad = await _store.LoadAsync();

        // Assert
        Assert.Equal(firstDeviceId, secondLoad.DeviceId);
    }

    [Fact]
    public async Task SaveAsync_UpdatesLastUpdateCheck()
    {
        // Arrange
        var settings = await _store.LoadAsync();
        var timestamp = DateTimeOffset.Now;
        settings.LastUpdateCheck = timestamp;

        // Act
        await _store.SaveAsync(settings);
        var loaded = await _store.LoadAsync();

        // Assert
        Assert.NotNull(loaded.LastUpdateCheck);
        Assert.Equal(timestamp.Date, loaded.LastUpdateCheck.Value.Date);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
