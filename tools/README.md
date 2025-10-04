# Tools Directory

This directory contains test harness tools and scripts for the TFT Sleep Tracker project.

## Scripts

### `send_fake.py`

Python script for sending fake sleep data to the Discord bot endpoint for E2E testing.

**Purpose**: Simulate the app's upload behavior to test integration with the Discord bot without needing the full WPF application.

**Requirements**:
- Python 3.6+
- `requests` library (`pip install requests`)

**Usage**:
```bash
python send_fake.py --token <TOKEN> --device-id <DEVICE_ID> --date <YYYY-MM-DD> --minutes <MINUTES>
```

**Documentation**: See [E2E Test Harness Documentation](../docs/E2E_TEST_HARNESS.md) for detailed usage examples and test scenarios.

## Installation

Install Python dependencies:
```bash
pip install requests
```

## Examples

```bash
# Send 8 hours of sleep data
python send_fake.py --token "test-token" --device-id "device-1" --date "2024-01-15" --minutes 480

# Use environment variable for host
export BOT_HOST="http://localhost:8000"
python send_fake.py --token "test-token" --device-id "device-1" --date "2024-01-15" --minutes 480

# Verbose mode for debugging
python send_fake.py --token "test-token" --device-id "device-1" --date "2024-01-15" --minutes 480 --verbose
```

## License

MIT License - Same as the main project.
