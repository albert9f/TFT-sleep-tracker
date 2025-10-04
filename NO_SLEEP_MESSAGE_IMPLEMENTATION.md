# No-Sleep Detection Feature Implementation

## Overview
This implementation adds a special message feature that detects when a user has zero sleep hours but the Windows app client was online (indicating data was collected). The message "looks like ethan didnt sleep tonight" is sent along with the sleep data, but only once per week to avoid spamming.

## Changes Made

### 1. QueuedUpload Payload Extension
**File:** `TFTSleepTracker.Core/Storage/UploadQueue.cs`
- Added optional `Message` property to `QueuedUpload` class
- Property is nullable (`string?`) so it doesn't break existing payloads
- Serialized to JSON with camelCase naming: `"message"`

### 2. Settings Storage
**File:** `TFTSleepTracker.Core/Storage/AppSettings.cs`
- Added `LastNoSleepMessageDate` property to track when the message was last sent
- Stored as ISO 8601 date string (yyyy-MM-dd)
- Persists to `settings.json` in ProgramData directory

### 3. Data Presence Tracking
**File:** `TFTSleepTracker.App/DailySummaryScheduler.cs`
- Added `HasData` property to `SummaryReadyEventArgs`
- Set to `true` when processing yesterday's summary with data points
- This distinguishes between "no sleep detected" vs "no data collected"

### 4. Message Logic
**File:** `TFTSleepTracker.App/App.xaml.cs`
- Enhanced `OnSummaryReady` method with no-sleep detection logic
- Checks three conditions:
  1. `HasData == true` (app was online collecting data)
  2. `TotalSleepMinutes == 0` (zero sleep detected)
  3. At least 7 days since last message (weekly limit)
- Updates `LastNoSleepMessageDate` when message is sent
- Sets `Message` field in `QueuedUpload` payload

## Weekly Limit Implementation
The weekly limit is enforced using `DateOnly.DayNumber` for accurate day counting:
```csharp
var daysSinceLastMessage = e.Date.DayNumber - lastMessageDate.DayNumber;
if (daysSinceLastMessage >= 7)
{
    shouldSendMessage = true;
}
```

## JSON Payload Format

### With Message (Zero Sleep Detected)
```json
{
  "deviceId": "test-device",
  "date": "2024-01-15",
  "sleepMinutes": 0,
  "computedAt": "2025-10-04T21:29:41.6812917+00:00",
  "message": "looks like ethan didnt sleep tonight"
}
```

### Without Message (Normal Sleep or Limit Not Met)
```json
{
  "deviceId": "test-device",
  "date": "2024-01-16",
  "sleepMinutes": 480,
  "computedAt": "2025-10-04T21:29:41.6859998+00:00",
  "message": null
}
```

## Testing

### Unit Tests (NoSleepMessageTests.cs)
- ✅ `QueuedUpload_SupportsOptionalMessage` - Verifies message field serialization
- ✅ `QueuedUpload_MessageCanBeNull` - Verifies null message handling
- ✅ `AppSettings_SupportsLastNoSleepMessageDate` - Verifies settings persistence
- ✅ `DateOnly_DayNumber_CalculatesDayDifference` - Verifies date math

### Integration Tests (NoSleepMessageIntegrationTests.cs)
- ✅ `NoSleepMessage_FirstTime_MessageSent` - First occurrence sends message
- ✅ `NoSleepMessage_WithinSevenDays_NoMessageSent` - Within 7 days, no message
- ✅ `NoSleepMessage_ExactlySevenDays_MessageSent` - On day 7, message sent
- ✅ `NoSleepMessage_WithSleepDetected_NoMessageSent` - Sleep > 0, no message
- ✅ `NoSleepMessage_NoData_NoMessageSent` - No data present, no message

### Test Results
- **Total Tests:** 37
- **Passed:** 37
- **Failed:** 0
- **Skipped:** 0

## Behavior Summary

| Scenario | HasData | SleepMinutes | Days Since Last | Message Sent? |
|----------|---------|--------------|-----------------|---------------|
| First no-sleep event | ✓ | 0 | N/A | ✓ |
| Second event within 7 days | ✓ | 0 | < 7 | ✗ |
| Event after 7+ days | ✓ | 0 | ≥ 7 | ✓ |
| Normal sleep | ✓ | > 0 | Any | ✗ |
| No data collected | ✗ | 0 | Any | ✗ |

## Backward Compatibility
- The `message` field is optional and defaults to `null`
- Existing Discord bot endpoints should handle the new field gracefully
- If the bot doesn't recognize the field, it will be ignored (standard JSON behavior)
- All existing functionality remains unchanged

## Discord Bot Integration Notes
The Discord bot will need to:
1. Accept the optional `message` field in the payload
2. Display the message prominently when present (e.g., in the Discord channel)
3. Show the message alongside the standard sleep data (date, 0 hours)

Example bot handling:
```javascript
if (payload.message) {
  // Display special message
  await channel.send(`🌙 ${payload.message}`);
}
// Display normal stats
await channel.send(`Date: ${payload.date}, Sleep: ${payload.sleepMinutes} minutes`);
```
