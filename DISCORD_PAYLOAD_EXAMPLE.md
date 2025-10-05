# Discord Payload Format

## What Gets Sent to Discord

Every hour on the hour, the app sends completed daily summaries to your Discord bot.

## Payload Structure

```json
{
  "deviceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "date": "2025-10-03",
  "sleepMinutes": 480,
  "computedAt": "2025-10-04T14:00:00.000Z"
}
```

## Field Descriptions

| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `deviceId` | string | Anonymous unique identifier for this device/user | `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"` |
| `date` | string | The date for which sleep was calculated (ISO 8601) | `"2025-10-03"` |
| `sleepMinutes` | number | Total sleep minutes during nightly window (11 PM - 8 AM) | `480` |
| `computedAt` | string | When this summary was computed (ISO 8601) | `"2025-10-04T14:00:00.000Z"` |

## Important Notes

### What IS Sent:
- ✅ Daily totals only (complete days)
- ✅ Anonymous device ID (auto-generated)
- ✅ Sleep duration in minutes
- ✅ Date of the sleep session

### What is NOT Sent:
- ❌ Current/incomplete day
- ❌ Raw activity data
- ❌ Keystroke information
- ❌ Mouse positions
- ❌ Application names
- ❌ Personal information
- ❌ Computer name or username

## Example Scenarios

### Scenario 1: Hourly Sync at 2:00 PM on Oct 4
**Sends**: Yesterday's complete data (Oct 3)
```json
{
  "deviceId": "device-123",
  "date": "2025-10-03",
  "sleepMinutes": 465,
  "computedAt": "2025-10-04T14:00:00Z"
}
```

### Scenario 2: Multiple Days Caught Up
If the app was offline for 3 days and comes back online at 3:00 PM on Oct 4:

**First upload** (Oct 1):
```json
{
  "deviceId": "device-123",
  "date": "2025-10-01",
  "sleepMinutes": 450,
  "computedAt": "2025-10-04T15:00:00Z"
}
```

**Second upload** (Oct 2):
```json
{
  "deviceId": "device-123",
  "date": "2025-10-02",
  "sleepMinutes": 420,
  "computedAt": "2025-10-04T15:00:00Z"
}
```

**Third upload** (Oct 3):
```json
{
  "deviceId": "device-123",
  "date": "2025-10-03",
  "sleepMinutes": 480,
  "computedAt": "2025-10-04T15:00:00Z"
}
```

**Does NOT send** (Oct 4):
❌ Today is incomplete, so it's excluded

## HTTP Request Format

The app makes an HTTP POST request to your Discord bot:

```http
POST https://your-discord-bot.example.com/ingest-sleep?token=your-secret-token
Content-Type: application/json

{
  "deviceId": "device-123",
  "date": "2025-10-03",
  "sleepMinutes": 480,
  "computedAt": "2025-10-04T14:00:00Z"
}
```

## Retry Behavior

If the upload fails (network error, bot offline, etc.):
- The payload is saved to `%ProgramData%\TFTSleepTracker\queue\{timestamp}_{guid}.json`
- The app retries with exponential backoff:
  - 1st retry: 2 seconds
  - 2nd retry: 4 seconds
  - 3rd retry: 8 seconds
  - ...up to 60 seconds max

## Discord Bot Requirements

Your Discord bot endpoint should:

1. **Accept POST requests** to `/ingest-sleep`
2. **Validate the token** query parameter
3. **Parse JSON body** with the structure shown above
4. **Return 2xx status** on success
5. **Return 4xx/5xx** on error (app will retry)

## Testing the Integration

Use the included Python test script:

```bash
cd tools
python send_fake.py --host https://your-bot.example.com --token your-token
```

This sends fake data to verify your bot endpoint is working.

---

*Last updated: October 4, 2025*
