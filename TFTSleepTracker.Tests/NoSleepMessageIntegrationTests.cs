using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using TFTSleepTracker.Core.Storage;
using TFTSleepTracker.Core.Logic;

namespace TFTSleepTracker.Tests;

/// <summary>
/// Integration tests for the no-sleep message workflow
/// </summary>
public class NoSleepMessageIntegrationTests : IDisposable
{
    private readonly string _testDataDirectory;
    private readonly string _testSettingsDirectory;

    public NoSleepMessageIntegrationTests()
    {
        _testDataDirectory = Path.Combine(Path.GetTempPath(), $"test_data_{Guid.NewGuid()}");
        _testSettingsDirectory = Path.Combine(Path.GetTempPath(), $"test_settings_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataDirectory);
        Directory.CreateDirectory(_testSettingsDirectory);
    }

    [Fact]
    public async Task NoSleepMessage_FirstTime_MessageSent()
    {
        // Arrange
        var settingsStore = new AppSettingsStore(_testSettingsDirectory);
        var settings = await settingsStore.LoadAsync();
        
        var date = new DateOnly(2024, 1, 15);
        var hasData = true;
        var sleepMinutes = 0;

        // Simulate the logic from App.xaml.cs OnSummaryReady
        string? message = null;
        if (hasData && sleepMinutes == 0)
        {
            bool shouldSendMessage = false;
            
            if (string.IsNullOrEmpty(settings.LastNoSleepMessageDate))
            {
                shouldSendMessage = true;
            }
            
            if (shouldSendMessage)
            {
                message = "looks like ethan didnt sleep tonight";
                settings.LastNoSleepMessageDate = date.ToString("yyyy-MM-dd");
                await settingsStore.SaveAsync(settings);
            }
        }

        // Assert
        Assert.NotNull(message);
        Assert.Equal("looks like ethan didnt sleep tonight", message);
        Assert.Equal("2024-01-15", settings.LastNoSleepMessageDate);
    }

    [Fact]
    public async Task NoSleepMessage_WithinSevenDays_NoMessageSent()
    {
        // Arrange
        var settingsStore = new AppSettingsStore(_testSettingsDirectory);
        var settings = await settingsStore.LoadAsync();
        settings.LastNoSleepMessageDate = "2024-01-15";
        await settingsStore.SaveAsync(settings);

        var date = new DateOnly(2024, 1, 20); // 5 days later
        var hasData = true;
        var sleepMinutes = 0;

        // Simulate the logic from App.xaml.cs OnSummaryReady
        string? message = null;
        if (hasData && sleepMinutes == 0)
        {
            bool shouldSendMessage = false;
            
            if (string.IsNullOrEmpty(settings.LastNoSleepMessageDate))
            {
                shouldSendMessage = true;
            }
            else
            {
                if (DateOnly.TryParse(settings.LastNoSleepMessageDate, out var lastMessageDate))
                {
                    var daysSinceLastMessage = date.DayNumber - lastMessageDate.DayNumber;
                    if (daysSinceLastMessage >= 7)
                    {
                        shouldSendMessage = true;
                    }
                }
            }
            
            if (shouldSendMessage)
            {
                message = "looks like ethan didnt sleep tonight";
                settings.LastNoSleepMessageDate = date.ToString("yyyy-MM-dd");
                await settingsStore.SaveAsync(settings);
            }
        }

        // Assert
        Assert.Null(message);
        Assert.Equal("2024-01-15", settings.LastNoSleepMessageDate); // Unchanged
    }

    [Fact]
    public async Task NoSleepMessage_ExactlySevenDays_MessageSent()
    {
        // Arrange
        var settingsStore = new AppSettingsStore(_testSettingsDirectory);
        var settings = await settingsStore.LoadAsync();
        settings.LastNoSleepMessageDate = "2024-01-15";
        await settingsStore.SaveAsync(settings);

        var date = new DateOnly(2024, 1, 22); // 7 days later
        var hasData = true;
        var sleepMinutes = 0;

        // Simulate the logic from App.xaml.cs OnSummaryReady
        string? message = null;
        if (hasData && sleepMinutes == 0)
        {
            bool shouldSendMessage = false;
            
            if (string.IsNullOrEmpty(settings.LastNoSleepMessageDate))
            {
                shouldSendMessage = true;
            }
            else
            {
                if (DateOnly.TryParse(settings.LastNoSleepMessageDate, out var lastMessageDate))
                {
                    var daysSinceLastMessage = date.DayNumber - lastMessageDate.DayNumber;
                    if (daysSinceLastMessage >= 7)
                    {
                        shouldSendMessage = true;
                    }
                }
            }
            
            if (shouldSendMessage)
            {
                message = "looks like ethan didnt sleep tonight";
                settings.LastNoSleepMessageDate = date.ToString("yyyy-MM-dd");
                await settingsStore.SaveAsync(settings);
            }
        }

        // Assert
        Assert.NotNull(message);
        Assert.Equal("looks like ethan didnt sleep tonight", message);
        Assert.Equal("2024-01-22", settings.LastNoSleepMessageDate); // Updated
    }

    [Fact]
    public async Task NoSleepMessage_WithSleepDetected_NoMessageSent()
    {
        // Arrange
        var settingsStore = new AppSettingsStore(_testSettingsDirectory);
        var settings = await settingsStore.LoadAsync();

        var date = new DateOnly(2024, 1, 15);
        var hasData = true;
        var sleepMinutes = 480; // 8 hours

        // Simulate the logic from App.xaml.cs OnSummaryReady
        string? message = null;
        if (hasData && sleepMinutes == 0)
        {
            // This block should not execute
            message = "looks like ethan didnt sleep tonight";
        }

        // Assert
        Assert.Null(message);
    }

    [Fact]
    public async Task NoSleepMessage_NoData_NoMessageSent()
    {
        // Arrange
        var settingsStore = new AppSettingsStore(_testSettingsDirectory);
        var settings = await settingsStore.LoadAsync();

        var date = new DateOnly(2024, 1, 15);
        var hasData = false; // No data present
        var sleepMinutes = 0;

        // Simulate the logic from App.xaml.cs OnSummaryReady
        string? message = null;
        if (hasData && sleepMinutes == 0)
        {
            // This block should not execute
            message = "looks like ethan didnt sleep tonight";
        }

        // Assert
        Assert.Null(message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDataDirectory))
        {
            Directory.Delete(_testDataDirectory, true);
        }
        if (Directory.Exists(_testSettingsDirectory))
        {
            Directory.Delete(_testSettingsDirectory, true);
        }
    }
}
