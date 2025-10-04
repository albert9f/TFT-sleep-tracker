using TFTSleepTracker.Core.Logic;
using Xunit;

namespace TFTSleepTracker.Tests;

/// <summary>
/// Comprehensive edge case tests for sleep calculation logic
/// </summary>
public class SleepCalculatorEdgeCasesTests
{
    [Fact]
    public void ComputeSleepMinutes_ShortIdleBurstUnder60Minutes_NoSleepCounted()
    {
        // Arrange: User idle for only 45 minutes within the nightly window
        // Should not count as sleep because it's under the 60-minute threshold
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(23).AddMinutes(45), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: 45 minutes - 60 threshold = 0 (negative clamped to 0)
        Assert.Equal(0, result);
    }

    [Fact]
    public void ComputeSleepMinutes_ShortIdleBurstOutsideWindow_NoSleepCounted()
    {
        // Arrange: User idle for 90 minutes, but entirely outside the [23:00, 08:00) window
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(10), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(11).AddMinutes(30), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Outside window = 0 minutes
        Assert.Equal(0, result);
    }

    [Fact]
    public void ComputeSleepMinutes_MidnightCrossing_FullNightCounted()
    {
        // Arrange: Inactivity span crosses midnight boundary
        // Idle from 22:00 to 09:00 next day
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(22), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(9), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Clipped to [23:00, 08:00) = 9 hours = 540 minutes
        // After threshold: 540 - 60 = 480 minutes
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_ActivityResetsBeforeNightlyWindow_NoDoubleCounting()
    {
        // Arrange: User idle from 22:00 to 22:55, then active at 22:55, then idle again from 23:00
        // Only the second idle period should count
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // First idle: 22:00 to 22:55 (55 minutes, but outside window)
            (
                start: new DateTimeOffset(baseDate.AddHours(22), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(22).AddMinutes(55), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Activity: 22:55 to 23:00 (resets inactivity)
            (
                start: new DateTimeOffset(baseDate.AddHours(22).AddMinutes(55), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Second idle: 23:00 to 08:00 (9 hours = 540 minutes)
            (
                start: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Only the second span counts: 540 - 60 = 480 minutes
        // First span is outside window and is also reset by activity
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_MicroActivityAt0111_ResetsInactivityTimer()
    {
        // Arrange: Inactivity from 23:00 to 01:11, micro activity at 01:11, then inactivity until 08:00
        // The 60-minute threshold is applied INSIDE the window to each continuous span
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // First inactivity: 23:00 to 01:11 (131 minutes)
            (
                start: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(11), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Micro activity: 01:11 to 01:15 (4 minutes)
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(11), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(15), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Second inactivity: 01:15 to 08:00 (405 minutes)
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(15), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert:
        // First span: 131 - 60 = 71 minutes
        // Second span: 405 - 60 = 345 minutes
        // Total: 71 + 345 = 416 minutes
        Assert.Equal(416, result);
    }

    [Fact]
    public void ComputeSleepMinutes_60MinuteThresholdAppliedInsideWindow()
    {
        // Arrange: Test that the 60-minute threshold is applied to the clipped window portion
        // Idle from 22:30 to 07:30 (9 hours), but clipped to [23:00, 07:30) = 8.5 hours = 510 minutes
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(22.5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(7.5), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Clipped to [23:00, 07:30) = 510 minutes
        // After threshold: 510 - 60 = 450 minutes
        Assert.Equal(450, result);
    }

    [Fact]
    public void ComputeSleepMinutes_ActivityRightBeforeNightlyWindow_OnlyWindowPortionCounts()
    {
        // Arrange: User idle from 20:00 to 22:59, active at 22:59, then idle 23:00 to 08:00
        // Only the period from 23:00 onwards should count
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // Early idle: 20:00 to 22:59 (outside window)
            (
                start: new DateTimeOffset(baseDate.AddHours(20), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(22).AddMinutes(59), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Activity: 22:59 to 23:00
            (
                start: new DateTimeOffset(baseDate.AddHours(22).AddMinutes(59), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Night idle: 23:00 to 08:00
            (
                start: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Only [23:00, 08:00) counts: 540 - 60 = 480 minutes
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_InactivityFrom2230To0730WithActivityAt0115()
    {
        // Arrange: Given inactivity from 22:30 to 07:30 with a 5-minute active blip at 01:15
        // Only the clipped intervals within [23:00, 08:00) beyond first 60 minutes count
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // First inactivity: 22:30 to 01:15
            (
                start: new DateTimeOffset(baseDate.AddHours(22.5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(15), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Activity blip: 01:15 to 01:20
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(15), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(20), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Second inactivity: 01:20 to 07:30
            (
                start: new DateTimeOffset(baseDate.AddDays(1).AddHours(1).AddMinutes(20), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(7.5), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert:
        // First span clipped to [23:00, 01:15) = 135 minutes -> 135 - 60 = 75 minutes
        // Second span [01:20, 07:30) = 370 minutes -> 370 - 60 = 310 minutes
        // Total: 75 + 310 = 385 minutes
        Assert.Equal(385, result);
    }

    [Fact]
    public void ComputeSleepMinutes_NoActivityOutsideWindowCounted()
    {
        // Arrange: Multiple inactivity periods, some outside the window
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            // Morning inactivity (outside window): 09:00 to 10:30
            (
                start: new DateTimeOffset(baseDate.AddHours(9), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(10.5), TimeSpan.FromHours(-5)),
                wasActive: false
            ),
            // Active during day
            (
                start: new DateTimeOffset(baseDate.AddHours(10.5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddHours(22.5), TimeSpan.FromHours(-5)),
                wasActive: true
            ),
            // Night inactivity (inside window): 22:30 to 08:30
            (
                start: new DateTimeOffset(baseDate.AddHours(22.5), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8.5), TimeSpan.FromHours(-5)),
                wasActive: false
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: Only night span clipped to [23:00, 08:00) = 540 minutes
        // After threshold: 540 - 60 = 480 minutes
        // Morning inactivity is completely ignored
        Assert.Equal(480, result);
    }

    [Fact]
    public void ComputeSleepMinutes_EmptyIntervals_ReturnsZero()
    {
        // Arrange: No intervals provided
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);
        var intervals = Array.Empty<(DateTimeOffset, DateTimeOffset, bool)>();

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: No data = 0 sleep
        Assert.Equal(0, result);
    }

    [Fact]
    public void ComputeSleepMinutes_AllActiveIntervals_ReturnsZero()
    {
        // Arrange: User active all night (no sleep)
        var nightly = new LocalTimeWindow(new TimeOnly(23, 0), new TimeOnly(8, 0));
        var threshold = TimeSpan.FromMinutes(60);

        var baseDate = new DateTime(2024, 1, 15);
        var intervals = new[]
        {
            (
                start: new DateTimeOffset(baseDate.AddHours(23), TimeSpan.FromHours(-5)),
                end: new DateTimeOffset(baseDate.AddDays(1).AddHours(8), TimeSpan.FromHours(-5)),
                wasActive: true
            )
        };

        // Act
        var result = SleepCalculator.ComputeSleepMinutes(intervals, threshold, nightly);

        // Assert: All active = 0 sleep
        Assert.Equal(0, result);
    }
}
