#!/usr/bin/env python3
"""
E2E Test Harness - Fake Sleep Data Sender

This script sends fake sleep summary data to the Discord bot ingest endpoint
for testing the integration between the TFT Sleep Tracker app and the bot.

Usage:
    python send_fake.py --token <TOKEN> --device-id <DEVICE_ID> --date <YYYY-MM-DD> --minutes <MINUTES> [--host <HOST>]

Example:
    python send_fake.py --token "test-token-123" --device-id "device-abc" --date "2024-01-15" --minutes 480 --host "http://localhost:8000"

Environment Variables:
    BOT_HOST: Default bot host URL (can be overridden with --host)
"""

import argparse
import json
import os
import sys
from datetime import datetime
from typing import Optional

try:
    import requests
except ImportError:
    print("Error: requests library not found. Install with: pip install requests", file=sys.stderr)
    sys.exit(1)


def send_fake_data(
    host: str,
    token: str,
    device_id: str,
    date: str,
    minutes: int,
    verbose: bool = False
) -> bool:
    """
    Send fake sleep data to the bot ingest endpoint.
    
    Args:
        host: Bot host URL (e.g., "http://localhost:8000")
        token: Static authentication token
        device_id: Device identifier
        date: Date in YYYY-MM-DD format
        minutes: Sleep minutes to report
        verbose: Print detailed output
    
    Returns:
        True if successful, False otherwise
    """
    # Validate date format
    try:
        datetime.strptime(date, "%Y-%m-%d")
    except ValueError:
        print(f"Error: Invalid date format '{date}'. Use YYYY-MM-DD format.", file=sys.stderr)
        return False
    
    # Validate minutes
    if minutes < 0 or minutes > 1440:
        print(f"Error: Minutes must be between 0 and 1440 (got {minutes})", file=sys.stderr)
        return False
    
    # Build URL
    url = f"{host.rstrip('/')}/ingest-sleep"
    
    # Build payload
    payload = {
        "deviceId": device_id,
        "date": date,
        "sleepMinutes": minutes
    }
    
    # Build request
    params = {"token": token}
    headers = {"Content-Type": "application/json"}
    
    if verbose:
        print(f"Sending POST to: {url}")
        print(f"Query params: {params}")
        print(f"Payload: {json.dumps(payload, indent=2)}")
    
    try:
        response = requests.post(url, params=params, json=payload, headers=headers, timeout=10)
        
        if verbose:
            print(f"Response status: {response.status_code}")
            print(f"Response body: {response.text}")
        
        if response.status_code == 200:
            print(f"✓ Successfully sent sleep data: {date} - {minutes} minutes for device {device_id}")
            return True
        else:
            print(f"✗ Failed to send data. Status: {response.status_code}, Response: {response.text}", file=sys.stderr)
            return False
    
    except requests.exceptions.ConnectionError as e:
        print(f"✗ Connection error: Could not connect to {url}. Is the bot server running?", file=sys.stderr)
        if verbose:
            print(f"   Details: {e}", file=sys.stderr)
        return False
    
    except requests.exceptions.Timeout:
        print(f"✗ Timeout: Request to {url} timed out after 10 seconds", file=sys.stderr)
        return False
    
    except Exception as e:
        print(f"✗ Unexpected error: {e}", file=sys.stderr)
        return False


def main():
    parser = argparse.ArgumentParser(
        description="Send fake sleep data to Discord bot for E2E testing",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  # Send 480 minutes (8 hours) of sleep for 2024-01-15
  python send_fake.py --token "test-token" --device-id "device-abc" --date "2024-01-15" --minutes 480

  # Use environment variable for host
  export BOT_HOST="http://localhost:8000"
  python send_fake.py --token "test-token" --device-id "device-abc" --date "2024-01-15" --minutes 420

  # Verbose output
  python send_fake.py --token "test-token" --device-id "device-abc" --date "2024-01-15" --minutes 360 --verbose
        """
    )
    
    parser.add_argument("--token", required=True, help="Static authentication token")
    parser.add_argument("--device-id", required=True, help="Device identifier")
    parser.add_argument("--date", required=True, help="Date in YYYY-MM-DD format")
    parser.add_argument("--minutes", type=int, required=True, help="Sleep minutes (0-1440)")
    parser.add_argument("--host", default=os.getenv("BOT_HOST", "http://localhost:8000"),
                        help="Bot host URL (default: BOT_HOST env var or http://localhost:8000)")
    parser.add_argument("--verbose", "-v", action="store_true", help="Verbose output")
    
    args = parser.parse_args()
    
    success = send_fake_data(
        host=args.host,
        token=args.token,
        device_id=args.device_id,
        date=args.date,
        minutes=args.minutes,
        verbose=args.verbose
    )
    
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()
