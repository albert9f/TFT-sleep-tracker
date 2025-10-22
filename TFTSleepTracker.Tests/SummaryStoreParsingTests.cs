using System.Globalization;
using TFTSleepTracker.Core.Storage;
using Xunit;

namespace TFTSleepTracker.Tests;

/// <summary>
/// Tests for SummaryStore's timestamp parsing using ParseExact for better performance and type safety
/// </summary>
public class SummaryStoreParsingTests
{
    [Fact]
    public async Task GetActivityDataPointsAsync_ParsesIso8601TimestampCorrectly()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "summarystore_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var store = new SummaryStore(testDir);
            var timestamp = new DateTimeOffset(2025, 10, 22, 14, 30, 45, TimeSpan.FromHours(-5));
            var expectedDataPoint = new ActivityDataPoint
            {
                Timestamp = timestamp,
                ActivityType = "TFT",
                DurationMinutes = 60
            };

            // Act - Write and read back
            await store.LogActivityDataPointAsync(expectedDataPoint);
            
            var startDate = timestamp.DateTime.AddHours(-1);
            var endDate = timestamp.DateTime.AddHours(1);
            var results = await store.GetActivityDataPointsAsync(startDate, endDate);

            // Assert
            Assert.Single(results);
            var actual = results[0];
            Assert.Equal(expectedDataPoint.Timestamp, actual.Timestamp);
            Assert.Equal(expectedDataPoint.ActivityType, actual.ActivityType);
            Assert.Equal(expectedDataPoint.DurationMinutes, actual.DurationMinutes);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public async Task GetActivityDataPointsAsync_ParsesMultipleDataPointsCorrectly()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "summarystore_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var store = new SummaryStore(testDir);
            var baseTimestamp = new DateTimeOffset(2025, 10, 22, 10, 0, 0, TimeSpan.Zero);
            var dataPoints = new[]
            {
                new ActivityDataPoint { Timestamp = baseTimestamp, ActivityType = "TFT", DurationMinutes = 30 },
                new ActivityDataPoint { Timestamp = baseTimestamp.AddMinutes(30), ActivityType = "Fortnite", DurationMinutes = 45 },
                new ActivityDataPoint { Timestamp = baseTimestamp.AddHours(1), ActivityType = "Roblox", DurationMinutes = 60 }
            };

            // Act
            foreach (var dp in dataPoints)
            {
                await store.LogActivityDataPointAsync(dp);
            }

            var startDate = baseTimestamp.DateTime.AddMinutes(-10);
            var endDate = baseTimestamp.DateTime.AddHours(2);
            var results = await store.GetActivityDataPointsAsync(startDate, endDate);

            // Assert
            Assert.Equal(3, results.Count);
            for (int i = 0; i < dataPoints.Length; i++)
            {
                Assert.Equal(dataPoints[i].Timestamp, results[i].Timestamp);
                Assert.Equal(dataPoints[i].ActivityType, results[i].ActivityType);
                Assert.Equal(dataPoints[i].DurationMinutes, results[i].DurationMinutes);
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public async Task GetGameTimeSummaryAsync_AggregatesCorrectly()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "summarystore_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var store = new SummaryStore(testDir);
            var baseTimestamp = new DateTimeOffset(2025, 10, 22, 10, 0, 0, TimeSpan.Zero);
            var dataPoints = new[]
            {
                new ActivityDataPoint { Timestamp = baseTimestamp, ActivityType = "TFT", DurationMinutes = 30 },
                new ActivityDataPoint { Timestamp = baseTimestamp.AddMinutes(30), ActivityType = "TFT", DurationMinutes = 45 },
                new ActivityDataPoint { Timestamp = baseTimestamp.AddHours(1), ActivityType = "Fortnite", DurationMinutes = 60 }
            };

            // Act
            foreach (var dp in dataPoints)
            {
                await store.LogActivityDataPointAsync(dp);
            }

            var startDate = baseTimestamp.DateTime.AddMinutes(-10);
            var endDate = baseTimestamp.DateTime.AddHours(2);
            var summary = await store.GetGameTimeSummaryAsync(startDate, endDate);

            // Assert
            Assert.Equal(2, summary.Count);
            Assert.Equal(75, summary["TFT"]); // 30 + 45
            Assert.Equal(60, summary["Fortnite"]);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }

    [Fact]
    public async Task GetActivityDataPointsAsync_HandlesTimestampsWithDifferentOffsets()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "summarystore_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var store = new SummaryStore(testDir);
            
            // Create timestamps with different offsets but in same UTC time range
            var timestamp1 = new DateTimeOffset(2025, 10, 22, 14, 0, 0, TimeSpan.Zero); // UTC
            var timestamp2 = new DateTimeOffset(2025, 10, 22, 15, 30, 0, TimeSpan.Zero); // UTC, 90 minutes later
            
            var dataPoints = new[]
            {
                new ActivityDataPoint { Timestamp = timestamp1, ActivityType = "TFT", DurationMinutes = 30 },
                new ActivityDataPoint { Timestamp = timestamp2, ActivityType = "Fortnite", DurationMinutes = 45 }
            };

            // Act
            foreach (var dp in dataPoints)
            {
                await store.LogActivityDataPointAsync(dp);
            }

            // Query with a wide enough range
            var startDate = new DateTime(2025, 10, 22, 13, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(2025, 10, 22, 16, 0, 0, DateTimeKind.Utc);
            var results = await store.GetActivityDataPointsAsync(startDate, endDate);

            // Assert - Both timestamps should be retrieved
            Assert.Equal(2, results.Count);
            Assert.Equal(timestamp1, results[0].Timestamp);
            Assert.Equal(timestamp2, results[1].Timestamp);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}
