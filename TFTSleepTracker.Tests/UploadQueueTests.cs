using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using TFTSleepTracker.Core.Storage;

namespace TFTSleepTracker.Tests;

public class UploadQueueTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly UploadQueue _queue;

    public UploadQueueTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"test_queue_{Guid.NewGuid()}");
        _queue = new UploadQueue(_testDirectory);
    }

    [Fact]
    public async Task EnqueueAsync_CreatesFileInQueue()
    {
        // Arrange
        var upload = new QueuedUpload
        {
            DeviceId = "test-device",
            Date = "2024-01-15",
            SleepMinutes = 480,
            ComputedAt = DateTimeOffset.Now
        };

        // Act
        await _queue.EnqueueAsync(upload);

        // Assert
        var files = Directory.GetFiles(_testDirectory, "*.json");
        Assert.Single(files);
    }

    [Fact]
    public async Task GetOldestFile_ReturnsOldestByName()
    {
        // Arrange
        await _queue.EnqueueAsync(new QueuedUpload
        {
            DeviceId = "test",
            Date = "2024-01-15",
            SleepMinutes = 480,
            ComputedAt = DateTimeOffset.Now
        });

        await Task.Delay(10); // Ensure different timestamps

        await _queue.EnqueueAsync(new QueuedUpload
        {
            DeviceId = "test",
            Date = "2024-01-16",
            SleepMinutes = 500,
            ComputedAt = DateTimeOffset.Now
        });

        // Act
        var oldestFile = _queue.GetOldestFile();

        // Assert
        Assert.NotNull(oldestFile);
        Assert.Contains("2024-01-15", oldestFile);
    }

    [Fact]
    public async Task ReadFileAsync_ReturnsUpload()
    {
        // Arrange
        var upload = new QueuedUpload
        {
            DeviceId = "test-device",
            Date = "2024-01-15",
            SleepMinutes = 480,
            ComputedAt = DateTimeOffset.Now
        };

        await _queue.EnqueueAsync(upload);
        var file = _queue.GetOldestFile();

        // Act
        var result = await _queue.ReadFileAsync(file!);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-device", result.DeviceId);
        Assert.Equal("2024-01-15", result.Date);
        Assert.Equal(480, result.SleepMinutes);
    }

    [Fact]
    public async Task DeleteFile_RemovesFile()
    {
        // Arrange
        await _queue.EnqueueAsync(new QueuedUpload
        {
            DeviceId = "test",
            Date = "2024-01-15",
            SleepMinutes = 480,
            ComputedAt = DateTimeOffset.Now
        });

        var file = _queue.GetOldestFile();
        Assert.NotNull(file);

        // Act
        _queue.DeleteFile(file);

        // Assert
        Assert.False(File.Exists(file));
        Assert.Equal(0, _queue.GetPendingCount());
    }

    [Fact]
    public async Task PurgeOldFiles_RemovesFilesOlderThanDays()
    {
        // Arrange
        await _queue.EnqueueAsync(new QueuedUpload
        {
            DeviceId = "test",
            Date = "2024-01-15",
            SleepMinutes = 480,
            ComputedAt = DateTimeOffset.Now
        });

        var file = _queue.GetOldestFile();
        Assert.NotNull(file);

        // Make the file appear old
        var fileInfo = new FileInfo(file);
        fileInfo.CreationTimeUtc = DateTime.UtcNow.AddDays(-8);
        fileInfo.LastWriteTimeUtc = DateTime.UtcNow.AddDays(-8);

        // Act
        _queue.PurgeOldFiles(7);

        // Assert
        Assert.Equal(0, _queue.GetPendingCount());
    }

    [Fact]
    public void GetPendingCount_ReturnsCorrectCount()
    {
        // Arrange & Act - initially empty
        var count = _queue.GetPendingCount();

        // Assert
        Assert.Equal(0, count);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}
