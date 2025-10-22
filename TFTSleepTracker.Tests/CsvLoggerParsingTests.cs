using System.Globalization;
using TFTSleepTracker.Core.Storage;
using Xunit;

namespace TFTSleepTracker.Tests;

/// <summary>
/// Tests for CsvLogger's timestamp parsing using ParseExact for better performance and type safety
/// </summary>
public class CsvLoggerParsingTests
{
    [Fact]
    public async Task GetDataPointsAsync_ParsesIso8601TimestampCorrectly()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "csvlogger_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var logger = new CsvLogger(testDir);
            var timestamp = new DateTimeOffset(2025, 10, 22, 14, 30, 45, TimeSpan.FromHours(-5));
            var expectedDataPoint = new ActivityDataPoint
            {
                Timestamp = timestamp,
                IsActive = true,
                InactivityMinutes = 5.5,
                SleepMinutesIncrement = 10
            };

            // Act - Write and read back
            await logger.AppendDataPointAsync(expectedDataPoint);
            var date = DateOnly.FromDateTime(timestamp.LocalDateTime.Date);
            var results = await logger.GetDataPointsAsync(date);

            // Assert
            Assert.Single(results);
            var actual = results[0];
            Assert.Equal(expectedDataPoint.Timestamp, actual.Timestamp);
            Assert.Equal(expectedDataPoint.IsActive, actual.IsActive);
            Assert.Equal(expectedDataPoint.InactivityMinutes, actual.InactivityMinutes);
            Assert.Equal(expectedDataPoint.SleepMinutesIncrement, actual.SleepMinutesIncrement);
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
    public async Task GetDataPointsAsync_ParsesMultipleDataPointsCorrectly()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "csvlogger_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var logger = new CsvLogger(testDir);
            var baseTimestamp = new DateTimeOffset(2025, 10, 22, 10, 0, 0, TimeSpan.Zero);
            var dataPoints = new[]
            {
                new ActivityDataPoint { Timestamp = baseTimestamp, IsActive = true, InactivityMinutes = 0, SleepMinutesIncrement = 0 },
                new ActivityDataPoint { Timestamp = baseTimestamp.AddMinutes(30), IsActive = false, InactivityMinutes = 30, SleepMinutesIncrement = 30 },
                new ActivityDataPoint { Timestamp = baseTimestamp.AddHours(1), IsActive = true, InactivityMinutes = 0, SleepMinutesIncrement = 0 }
            };

            // Act
            foreach (var dp in dataPoints)
            {
                await logger.AppendDataPointAsync(dp);
            }

            var date = DateOnly.FromDateTime(baseTimestamp.LocalDateTime.Date);
            var results = await logger.GetDataPointsAsync(date);

            // Assert
            Assert.Equal(3, results.Count);
            for (int i = 0; i < dataPoints.Length; i++)
            {
                Assert.Equal(dataPoints[i].Timestamp, results[i].Timestamp);
                Assert.Equal(dataPoints[i].IsActive, results[i].IsActive);
                Assert.Equal(dataPoints[i].InactivityMinutes, results[i].InactivityMinutes);
                Assert.Equal(dataPoints[i].SleepMinutesIncrement, results[i].SleepMinutesIncrement);
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
    public async Task GetDataPointsAsync_HandlesMalformedTimestampGracefully()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), "csvlogger_test_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        try
        {
            var csvFilePath = Path.Combine(testDir, "2025-10-22.csv");
            // Use the exact format that .NET produces with ToString("o")
            var timestamp1 = new DateTimeOffset(2025, 10, 22, 10, 0, 0, TimeSpan.Zero);
            var timestamp2 = new DateTimeOffset(2025, 10, 22, 11, 0, 0, TimeSpan.Zero);
            var csvContent = "timestamp,is_active,inactivity_minutes,sleep_minutes_increment\n" +
                            $"{timestamp1.ToString("o")},true,0,0\n" +
                            "invalid-timestamp,false,30,30\n" +
                            $"{timestamp2.ToString("o")},true,0,0\n";
            await File.WriteAllTextAsync(csvFilePath, csvContent);

            var logger = new CsvLogger(testDir);
            var date = new DateOnly(2025, 10, 22);

            // Act
            var results = await logger.GetDataPointsAsync(date);

            // Assert - Should skip the malformed line
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
