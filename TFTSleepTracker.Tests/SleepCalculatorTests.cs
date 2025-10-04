using TFTSleepTracker.Core.Logic;
using Xunit;

namespace TFTSleepTracker.Tests;

public class SleepCalculatorTests
{
    [Fact]
    public void ComputeSleepMinutes_EdgeWindowClipping_OnlyCounts23To08Range()
    {
        // Arrange: User idle from 22:30 to 08:30, but only [23:00, 08:00) should count
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        // Create an inactivity span from 22:30 to 08:30 (10 hours = 600 minutes)
        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(22.5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8.5), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Only [23:00, 08:00) = 9 hours = 540 minutes
        // After 60-minute threshold: 540 - 60 = 480 minutes
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_MicroActivityResetsSpan()
    {
        // Arrange: User idle, then brief activity (micro), then idle again
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // First inactivity span: 23:00 to 01:00 (2 hours = 120 minutes)
            (
                start: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(1), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Micro activity: 01:00 to 01:05 (5 minutes)
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(1), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(5), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Second inactivity span: 01:05 to 08:00 (6 hours 55 minutes = 415 minutes)
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: 
        // First span: 120 minutes - 60 threshold = 60 minutes
        // Second span: 415 minutes - 60 threshold = 355 minutes
        // Total: 60 + 355 = 415 minutes
        Assert.Equal(415, result);
    }

    [Fact]
    public void ComputeSleepMinutes_FallBackNight_HandlesDSTCorrectly()
    {
        // Arrange: Fall back night (US: 2024-11-03, clocks go back at 2 AM)
        // When clocks fall back, 1:00 AM - 2:00 AM occurs twice
        // The night window should still be [23:00, 08:00) in local time
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        // Create a timezone that has DST (US Eastern)
        var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        
        // Before DST transition: EDT (UTC-4)
        // After DST transition: EST (UTC-5)
        var fallBackDate = new DateTime(2024, 11, 3); // Day when DST ends in US
        
        // User goes idle at 23:00 EDT (UTC-4) and stays idle through the fall-back
        // 23:00 EDT on Nov 2 → 08:00 EST on Nov 3
        var idleStart = new DateTimeOffset(
            new DateTime(2024, 11, 2, 23, 0, 0, DateTimeKind.Unspecified),
            TimeSpan.FromHours(-4) // EDT offset
        );
        
        // At 2:00 AM EDT, clocks fall back to 1:00 AM EST
        // So 08:00 EST is actually 10 hours of real time from 23:00 EDT
        var idleEnd = new DateTimeOffset(
            new DateTime(2024, 11, 3, 8, 0, 0, DateTimeKind.Unspecified),
            TimeSpan.FromHours(-5) // EST offset
        );

        var intervals = new[]
        {
            (start: idleStart, end: idleEnd, wasActive: false)
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: The actual duration is 10 hours (600 minutes) in real time
        // But we should count based on local time window [23:00, 08:00) = 9 hours = 540 minutes
        // After threshold: 540 - 60 = 480 minutes
        // Note: This is the expected behavior - count local time window, not wall clock time
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_ThresholdAppliedToClippedRange()
    {
        // Arrange: Verify threshold is applied AFTER clipping to nightly window
        // User idle 22:30 to 23:30 (60 minutes total, but only 30 minutes in [23:00, 08:00))
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(22.5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(23.5), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: After clipping [23:00, 23:30] = 30 minutes
        // 30 - 60 threshold = max(0, -30) = 0 minutes
        Assert.Equal(0, result);
    }

    [Fact]
    public void ComputeSleepMinutes_MultipleNightsWithEdgeClipping()
    {
        // Arrange: Spans multiple nights, each should be clipped to [23:00, 08:00)
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // Night 1: 22:00 to 08:30
            (
                start: new DateTimeOffset(baseDate.AddHours(22), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8.5), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Active during the day
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(9), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(22), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Night 2: 22:00 to 09:00
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(22), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(2).AddHours(9), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: 
        // Night 1: [23:00, 08:00) = 540 minutes - 60 threshold = 480 minutes
        // Night 2: [23:00, 08:00) = 540 minutes - 60 threshold = 480 minutes
        // Total: 480 + 480 = 960 minutes
        Assert.Equal(960, result);
    }

    [Fact]
    public void ComputeSleepMinutes_FallBackNight_ActualRealTimeValidation()
    {
        // Arrange: This test explicitly validates the 10-hour real time scenario
        // During fall-back, the user is actually idle for 10 real hours (600 minutes)
        // But we should count based on the local time window which is 9 hours (540 minutes)
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        // 23:00 EDT (UTC-4) to 08:00 EST (UTC-5) on fall-back night
        var idleStart = new DateTimeOffset(2024, 11, 2, 23, 0, 0, TimeSpan.FromHours(-4));
        var idleEnd = new DateTimeOffset(2024, 11, 3, 8, 0, 0, TimeSpan.FromHours(-5));

        // Verify the actual duration is 10 hours in real time
        var actualDuration = idleEnd - idleStart;
        Assert.Equal(10, actualDuration.TotalHours); // Real time is 10 hours

        var intervals = new[]
        {
            (start: idleStart, end: idleEnd, wasActive: false)
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Despite 10 hours of real time, we count 9 hours (540 minutes) based on local time window
        // After threshold: 540 - 60 = 480 minutes
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_SpringForwardNight_LoseAnHour()
    {
        // Arrange: Spring forward night (US: 2024-03-10, clocks spring forward at 2 AM)
        // When clocks spring forward, 2:00 AM - 3:00 AM doesn't exist
        // The night window should still be [23:00, 08:00) in local time
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        // User goes idle at 23:00 EST (UTC-5) and stays idle through the spring-forward
        // 23:00 EST on Mar 9 → 08:00 EDT on Mar 10
        var idleStart = new DateTimeOffset(2024, 3, 9, 23, 0, 0, TimeSpan.FromHours(-5));
        var idleEnd = new DateTimeOffset(2024, 3, 10, 8, 0, 0, TimeSpan.FromHours(-4));

        // Verify the actual duration is 8 hours in real time (due to spring forward)
        var actualDuration = idleEnd - idleStart;
        Assert.Equal(8, actualDuration.TotalHours); // Real time is only 8 hours

        var intervals = new[]
        {
            (start: idleStart, end: idleEnd, wasActive: false)
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Local time window is still 9 hours [23:00, 08:00) = 540 minutes
        // After threshold: 540 - 60 = 480 minutes
        Assert.Equal(480, result);
    }
}
