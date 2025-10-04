# E2E Test Harness Documentation

This document describes the end-to-end (E2E) test harness for validating the integration between the TFT Sleep Tracker WPF application and the Discord bot.

## Overview

The E2E test harness simulates the full workflow:
1. **App generates sleep data** → Writes to CSV and creates daily summary
2. **App uploads to bot** → POSTs to `/ingest-sleep?token=<TOKEN>` with JSON payload
3. **Bot receives and stores** → Processes and stores sleep data
4. **User queries bot** → Discord command `/sleep` returns tracked data

The harness focuses on step 2-3, providing tools to simulate the app's upload behavior.

## Tools

### `tools/send_fake.py`

A Python script that sends fake sleep data to the Discord bot for testing.

**Features:**
- Sends POST request to bot's ingest endpoint
- Supports command-line arguments for flexibility
- Environment variable support for bot host
- Verbose mode for debugging
- Input validation (date format, minute range)
- Proper error handling and reporting

**Usage:**

```bash
# Basic usage
python tools/send_fake.py \
  --token "test-token-123" \
  --device-id "device-abc" \
  --date "2024-01-15" \
  --minutes 480

# Use environment variable for host
export BOT_HOST="http://localhost:8000"
python tools/send_fake.py --token "test-token" --device-id "device-abc" --date "2024-01-15" --minutes 480

# Verbose output for debugging
python tools/send_fake.py \
  --token "test-token" \
  --device-id "device-abc" \
  --date "2024-01-15" \
  --minutes 360 \
  --verbose

# Custom bot host
python tools/send_fake.py \
  --host "https://my-bot.example.com" \
  --token "test-token" \
  --device-id "device-abc" \
  --date "2024-01-15" \
  --minutes 420
```

**Arguments:**
- `--token`: Static authentication token (required)
- `--device-id`: Device identifier (required)
- `--date`: Date in YYYY-MM-DD format (required)
- `--minutes`: Sleep minutes, 0-1440 (required)
- `--host`: Bot host URL (optional, defaults to BOT_HOST env var or http://localhost:8000)
- `--verbose` or `-v`: Enable verbose output (optional)

**Exit Codes:**
- `0`: Success
- `1`: Failure (validation error, connection error, HTTP error)

## E2E Test Scenarios

### Scenario 1: Single Day Upload

Simulate a single day's sleep data upload.

```bash
python tools/send_fake.py \
  --token "test-token" \
  --device-id "test-device-1" \
  --date "2024-01-15" \
  --minutes 480 \
  --host "http://localhost:8000"
```

**Expected Result:**
- HTTP 200 response
- Bot stores data for device `test-device-1` on date `2024-01-15`
- `/sleep` command on Discord shows 480 minutes

### Scenario 2: Multiple Dates/Times

Simulate multiple days with varying sleep amounts.

```bash
# Day 1: Good night's sleep (8 hours)
python tools/send_fake.py --token "test-token" --device-id "test-device-2" \
  --date "2024-01-15" --minutes 480

# Day 2: Short sleep (5 hours)
python tools/send_fake.py --token "test-token" --device-id "test-device-2" \
  --date "2024-01-16" --minutes 300

# Day 3: No sleep (0 minutes)
python tools/send_fake.py --token "test-token" --device-id "test-device-2" \
  --date "2024-01-17" --minutes 0

# Day 4: Full night (9 hours)
python tools/send_fake.py --token "test-token" --device-id "test-device-2" \
  --date "2024-01-18" --minutes 540
```

**Expected Result:**
- All 4 requests succeed
- Bot stores data for all 4 dates
- `/sleep` command shows sleep history

### Scenario 3: Various Inactivity Patterns

Simulate different sleep patterns to test edge cases.

```bash
# Very short sleep (just over threshold)
python tools/send_fake.py --token "test-token" --device-id "test-device-3" \
  --date "2024-01-15" --minutes 65

# Average sleep (6.5 hours)
python tools/send_fake.py --token "test-token" --device-id "test-device-3" \
  --date "2024-01-16" --minutes 390

# Maximum sleep (24 hours, edge case)
python tools/send_fake.py --token "test-token" --device-id "test-device-3" \
  --date "2024-01-17" --minutes 1440

# Boundary case (exactly 60 minutes - first hour is threshold)
python tools/send_fake.py --token "test-token" --device-id "test-device-3" \
  --date "2024-01-18" --minutes 60
```

**Expected Result:**
- All edge cases handled correctly
- No validation errors
- Data stored accurately

### Scenario 4: Network Failure Simulation

Test retry logic by simulating network failures.

```bash
# Stop the bot server or use an invalid host
python tools/send_fake.py \
  --host "http://invalid-host:9999" \
  --token "test-token" \
  --device-id "test-device-4" \
  --date "2024-01-15" \
  --minutes 480

# Expected: Connection error, exit code 1
```

**Expected Result:**
- Connection error reported
- Script exits with code 1
- In a real app scenario, the payload would be queued for retry

### Scenario 5: Authentication Failure

Test authentication with invalid token.

```bash
python tools/send_fake.py \
  --token "invalid-token" \
  --device-id "test-device-5" \
  --date "2024-01-15" \
  --minutes 480 \
  --host "http://localhost:8000"

# Expected: HTTP 401 or 403 error
```

**Expected Result:**
- HTTP error (401/403)
- Script exits with code 1
- Bot rejects the request

## GitHub Actions Integration

The GitHub Actions workflow includes E2E test steps:

1. **Setup Python**: Installs Python 3.12
2. **Install Dependencies**: `pip install requests`
3. **Run E2E Test Placeholder**: Demonstrates how E2E tests would be structured

### Current Implementation

The workflow currently includes a **placeholder** for E2E tests. This is because:
- The Discord bot server needs to be running for real E2E tests
- Bot deployment is outside the scope of this repository
- Full E2E testing requires coordinated deployment of both repos

### Future Full E2E Implementation

To enable full E2E testing in CI:

1. **Deploy Bot to Test Environment**:
   - Use GitHub Actions to deploy the bot to a staging server
   - Or use a local bot server (e.g., via Docker container)

2. **Start Bot in Background**:
   ```yaml
   - name: Start Discord bot (background)
     run: |
       cd ../shortgemini  # Assuming bot repo is checked out
       python -m uvicorn main:app --host 0.0.0.0 --port 8000 &
       sleep 5  # Wait for bot to start
   ```

3. **Run Multiple Test Cases**:
   ```yaml
   - name: E2E Test - Multiple scenarios
     env:
       BOT_HOST: http://localhost:8000
       TEST_TOKEN: ${{ secrets.TEST_BOT_TOKEN }}
     run: |
       python tools/send_fake.py --token "$TEST_TOKEN" --device-id "ci-device-1" --date "2024-01-15" --minutes 480
       python tools/send_fake.py --token "$TEST_TOKEN" --device-id "ci-device-1" --date "2024-01-16" --minutes 300
       python tools/send_fake.py --token "$TEST_TOKEN" --device-id "ci-device-1" --date "2024-01-17" --minutes 540
   ```

4. **Verify Results**:
   ```yaml
   - name: Verify bot data
     run: |
       # Use bot's API or database to verify data was stored correctly
       # Or send Discord command and parse response
   ```

## Manual Testing in GitHub Codespaces

You can run E2E tests manually in a GitHub Codespace:

### Step 1: Start the Bot Server

In one terminal:
```bash
# Navigate to bot repo (assuming it's cloned alongside this repo)
cd ../shortgemini

# Install dependencies
pip install -r requirements.txt

# Start bot server
python -m uvicorn main:app --host 0.0.0.0 --port 8000
```

### Step 2: Run E2E Tests

In another terminal:
```bash
cd TFT-sleep-tracker

# Install requests library
pip install requests

# Set bot host
export BOT_HOST="http://localhost:8000"

# Run test scenarios
python tools/send_fake.py --token "test-token" --device-id "codespace-test" --date "2024-01-15" --minutes 480
python tools/send_fake.py --token "test-token" --device-id "codespace-test" --date "2024-01-16" --minutes 300
python tools/send_fake.py --token "test-token" --device-id "codespace-test" --date "2024-01-17" --minutes 540

# Verify results
curl "http://localhost:8000/sleep?device_id=codespace-test" # Example API call
```

### Step 3: Verify on Discord

If the bot is connected to Discord:
1. Open Discord
2. Navigate to the test server
3. Run `/sleep` command
4. Verify data matches what was sent

## Testing on Windows Runners

The E2E test harness is designed to work on `windows-latest` runners in GitHub Actions. Key considerations:

- **Python**: Available by default on GitHub-hosted Windows runners
- **PowerShell**: Used for workflow commands (supports both `pwsh` and `cmd`)
- **Network**: Windows runners can make outbound HTTP requests
- **WPF App**: Can be built and tested on Windows runners (requires .NET 8.0)

## Test Data Patterns

Use these patterns to simulate real-world usage:

### Pattern 1: Consistent Sleeper
```bash
# Person who sleeps 7-8 hours every night
for i in {1..7}; do
  python tools/send_fake.py --token "test" --device-id "consistent" \
    --date "2024-01-$(printf "%02d" $i)" --minutes $((420 + RANDOM % 60))
done
```

### Pattern 2: Irregular Sleeper
```bash
# Person with varying sleep patterns
python tools/send_fake.py --token "test" --device-id "irregular" --date "2024-01-01" --minutes 540  # 9 hours
python tools/send_fake.py --token "test" --device-id "irregular" --date "2024-01-02" --minutes 240  # 4 hours
python tools/send_fake.py --token "test" --device-id "irregular" --date "2024-01-03" --minutes 480  # 8 hours
python tools/send_fake.py --token "test" --device-id "irregular" --date "2024-01-04" --minutes 120  # 2 hours
python tools/send_fake.py --token "test" --device-id "irregular" --date "2024-01-05" --minutes 600  # 10 hours
```

### Pattern 3: Sleep Deprivation
```bash
# Simulating a week of poor sleep
for i in {1..7}; do
  python tools/send_fake.py --token "test" --device-id "deprived" \
    --date "2024-01-$(printf "%02d" $i)" --minutes $((180 + RANDOM % 120))
done
```

## Troubleshooting

### Issue: `requests` module not found
**Solution**: Install the requests library:
```bash
pip install requests
```

### Issue: Connection refused
**Solution**: Verify the bot server is running:
```bash
curl http://localhost:8000/health  # Check if bot responds
```

### Issue: Authentication failure
**Solution**: Verify the token matches the bot's configuration:
```bash
# Check bot's expected token in environment variables or config file
```

### Issue: Invalid date format
**Solution**: Use YYYY-MM-DD format:
```bash
# ✓ Correct
python tools/send_fake.py --date "2024-01-15" ...

# ✗ Wrong
python tools/send_fake.py --date "01/15/2024" ...
```

## Summary

The E2E test harness provides:
- ✅ Script to send fake sleep data (`tools/send_fake.py`)
- ✅ Command-line interface with validation
- ✅ GitHub Actions integration (placeholder)
- ✅ Documentation for manual testing
- ✅ Multiple test scenarios for edge cases

This ensures that the TFT Sleep Tracker app and Discord bot work together seamlessly across different environments (Windows, Linux, Codespaces) and usage patterns.
