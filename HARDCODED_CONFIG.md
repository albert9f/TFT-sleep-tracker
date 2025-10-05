# Hardcoded Configuration Documentation

## Overview

The TFT Sleep Tracker application has been configured with hardcoded credentials to connect directly to your Google Cloud VM without requiring manual configuration.

## Configuration Details

**VM Information:**
- **IP Address**: `35.212.220.200`
- **Protocol**: HTTPS
- **Endpoint**: `/ingest-sleep`
- **Authentication Token**: `weatheryETHAN`

**Full URL**: `https://35.212.220.200/ingest-sleep?token=weatheryETHAN`

## How It Works

### Data Flow

```
Windows PC (TFT Sleep Tracker)
        ↓
   HTTP POST Request
        ↓
Google Cloud VM (35.212.220.200)
        ↓
Discord Bot Server
        ↓
Database Storage
        ↓
Discord /sleep Command
```

### Automatic Sync Schedule

The application automatically sends sleep data:
- **Every hour** at `:00` (e.g., 1:00, 2:00, 3:00)
- Sends data for the **previous 7 days** (excludes today)
- Retries failed uploads with exponential backoff

### Test Button

Users can also manually test the connection:
- Click "🧪 Send Test Data to Discord" button
- Sends a 2-hour sleep session with random September 2001 date
- Provides instant feedback (green = success, red = error)

## API Contract

### Request Format

**Method**: POST  
**URL**: `https://35.212.220.200/ingest-sleep?token=weatheryETHAN`  
**Content-Type**: `application/json`

**Body**:
```json
{
  "deviceId": "device-abc123def456",
  "date": "2025-10-04",
  "sleepMinutes": 480,
  "computedAt": "2025-10-04T08:05:00Z"
}
```

### Response Format

**Success**: HTTP 200 OK  
**Unauthorized**: HTTP 401 (invalid token)  
**Server Error**: HTTP 500

## VM Endpoint Implementation

Your Google Cloud VM needs to implement this endpoint:

### Node.js/Express Example

```javascript
const express = require('express');
const app = express();

app.use(express.json());

app.post('/ingest-sleep', async (req, res) => {
  try {
    // 1. Validate token
    if (req.query.token !== 'weatheryETHAN') {
      return res.status(401).json({ error: 'Unauthorized' });
    }

    // 2. Extract payload
    const { deviceId, date, sleepMinutes, computedAt } = req.body;

    // 3. Validate data
    if (!deviceId || !date || sleepMinutes === undefined) {
      return res.status(400).json({ error: 'Missing required fields' });
    }

    // 4. Save to database
    await database.saveSleepData({
      deviceId,
      date,
      sleepMinutes,
      computedAt: computedAt || new Date().toISOString()
    });

    // 5. Log success
    console.log(`Received sleep data: ${deviceId} - ${date} - ${sleepMinutes} min`);

    // 6. Return success
    res.status(200).json({ success: true });
  } catch (error) {
    console.error('Error processing sleep data:', error);
    res.status(500).json({ error: 'Internal server error' });
  }
});

app.listen(443, () => {
  console.log('Sleep tracker endpoint listening on port 443 (HTTPS)');
});
```

### Python/Flask Example

```python
from flask import Flask, request, jsonify
import sqlite3
from datetime import datetime

app = Flask(__name__)

@app.route('/ingest-sleep', methods=['POST'])
def ingest_sleep():
    try:
        # 1. Validate token
        if request.args.get('token') != 'weatheryETHAN':
            return jsonify({'error': 'Unauthorized'}), 401
        
        # 2. Extract payload
        data = request.get_json()
        device_id = data.get('deviceId')
        date = data.get('date')
        sleep_minutes = data.get('sleepMinutes')
        computed_at = data.get('computedAt', datetime.utcnow().isoformat())
        
        # 3. Validate data
        if not all([device_id, date, sleep_minutes is not None]):
            return jsonify({'error': 'Missing required fields'}), 400
        
        # 4. Save to database
        save_sleep_data(device_id, date, sleep_minutes, computed_at)
        
        # 5. Log success
        print(f"Received sleep data: {device_id} - {date} - {sleep_minutes} min")
        
        # 6. Return success
        return jsonify({'success': True}), 200
    except Exception as e:
        print(f"Error processing sleep data: {e}")
        return jsonify({'error': 'Internal server error'}), 500

def save_sleep_data(device_id, date, sleep_minutes, computed_at):
    # Your database logic here
    pass

if __name__ == '__main__':
    # For HTTPS, you'll need SSL certificates
    app.run(host='0.0.0.0', port=443, ssl_context='adhoc')
```

## SSL/HTTPS Setup

Since the app is configured for HTTPS, your VM **must** have a valid SSL certificate.

### Option 1: Let's Encrypt (Recommended - Free)

```bash
# Install Certbot
sudo apt-get update
sudo apt-get install certbot python3-certbot-nginx

# Get certificate (requires domain name)
sudo certbot --nginx -d yourdomain.com

# Auto-renewal
sudo certbot renew --dry-run
```

### Option 2: Self-Signed Certificate (Development Only)

```bash
# Generate self-signed cert
openssl req -x509 -newkey rsa:4096 -nodes \
  -keyout key.pem -out cert.pem -days 365

# Note: Windows will show security warnings with self-signed certs
```

### Option 3: Disable HTTPS (Not Recommended)

If you can't set up SSL, you can change the app to use HTTP:

1. Edit `AppSettings.cs`: Change to `"http://35.212.220.200"`
2. Rebuild: `dotnet publish ...`
3. Update VM to listen on port 80 instead of 443

## Firewall Configuration

Your Google Cloud VM already has these firewall rules enabled:
- ✅ `http-server` (port 80)
- ✅ `https-server` (port 443)

No additional firewall changes needed!

## Testing the Endpoint

### From Your VM (Localhost)

```bash
curl -X POST "https://localhost/ingest-sleep?token=weatheryETHAN" \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "test-device",
    "date": "2025-10-04",
    "sleepMinutes": 120,
    "computedAt": "2025-10-04T12:00:00Z"
  }'
```

### From External Client

```bash
curl -X POST "https://35.212.220.200/ingest-sleep?token=weatheryETHAN" \
  -H "Content-Type: application/json" \
  -d '{
    "deviceId": "test-device",
    "date": "2025-10-04",
    "sleepMinutes": 120,
    "computedAt": "2025-10-04T12:00:00Z"
  }'
```

Expected response: `{"success": true}` or `200 OK`

### From Windows App

1. Download `TFTSleepTracker.exe`
2. Run the application
3. Click "🧪 Send Test Data to Discord"
4. Check for green success message
5. Verify data received on VM

## Discord Bot Integration

Your Discord bot should fetch data from the database when users run `/sleep`:

```javascript
// Discord bot command handler
client.on('interactionCreate', async interaction => {
  if (interaction.commandName === 'sleep') {
    const userId = interaction.user.id;
    
    // Fetch sleep data from database
    const sleepData = await database.getSleepDataForUser(userId);
    
    // Format and send response
    const embed = {
      title: "Sleep Statistics",
      fields: [
        { name: "Date", value: sleepData.date },
        { name: "Sleep Duration", value: `${sleepData.sleepMinutes} minutes` },
        { name: "Hours", value: `${(sleepData.sleepMinutes / 60).toFixed(1)} hours` }
      ]
    };
    
    await interaction.reply({ embeds: [embed] });
  }
});
```

## Security Considerations

### ⚠️ Important Security Notes

1. **Token Visibility**: The token `weatheryETHAN` is compiled into the executable and can be extracted by reverse engineering. This is acceptable for personal/trusted use but not for public distribution.

2. **HTTPS is Critical**: Without SSL/TLS encryption, the token and data are sent in plain text over the internet.

3. **IP Address**: Using a raw IP address (35.212.220.200) instead of a domain name means:
   - No DNS-based DDoS protection
   - IP could change if VM is recreated
   - Harder to remember/document

4. **Rate Limiting**: Consider implementing rate limiting on your endpoint to prevent abuse.

### Recommended Security Enhancements

```javascript
// Add rate limiting
const rateLimit = require('express-rate-limit');

const limiter = rateLimit({
  windowMs: 15 * 60 * 1000, // 15 minutes
  max: 100 // limit each IP to 100 requests per windowMs
});

app.use('/ingest-sleep', limiter);

// Add request logging
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.path} - ${req.ip}`);
  next();
});

// Validate deviceId format
function isValidDeviceId(deviceId) {
  return /^device-[a-f0-9]{32}$/.test(deviceId);
}
```

## Troubleshooting

### App Shows "Upload Failed"

**Possible causes:**
1. VM endpoint not implemented
2. SSL certificate issues
3. Firewall blocking connection
4. VM is down
5. Wrong port

**Debug steps:**
```bash
# Check if VM is reachable
ping 35.212.220.200

# Check if HTTPS port is open
telnet 35.212.220.200 443

# Check endpoint with curl
curl -v https://35.212.220.200/ingest-sleep?token=weatheryETHAN

# Check VM logs
journalctl -u your-app-name -f
```

### SSL Certificate Errors

If using self-signed certificates, Windows will show security warnings. To bypass (development only):

```csharp
// Add to UploadService.cs constructor (NOT RECOMMENDED FOR PRODUCTION)
var handler = new HttpClientHandler();
handler.ServerCertificateCustomValidationCallback = 
    (message, cert, chain, sslPolicyErrors) => true;
_httpClient = new HttpClient(handler);
```

### Discord Not Showing Data

1. Verify data is reaching VM (check logs)
2. Check database contains the data
3. Verify Discord bot is fetching from correct database
4. Test Discord bot's `/sleep` command handler

### Port Already in Use

```bash
# Find what's using port 443
sudo lsof -i :443

# Kill the process (if safe)
sudo kill -9 <PID>

# Or use a different port and rebuild app
```

## Monitoring and Maintenance

### Log Rotation

```bash
# Setup logrotate for your app
sudo nano /etc/logrotate.d/sleep-tracker

# Add:
/var/log/sleep-tracker/*.log {
    daily
    rotate 7
    compress
    delaycompress
    missingok
    notifempty
}
```

### Health Check Endpoint

```javascript
app.get('/health', (req, res) => {
  res.status(200).json({
    status: 'healthy',
    timestamp: new Date().toISOString(),
    uptime: process.uptime()
  });
});
```

### Metrics

Consider tracking:
- Total requests received
- Failed authentications
- Average sleep duration
- Unique devices
- Request rate per hour

## Backup and Recovery

### Database Backups

```bash
# Backup SQLite database (if using SQLite)
sqlite3 /path/to/database.db .dump > backup-$(date +%Y%m%d).sql

# Backup PostgreSQL (if using Postgres)
pg_dump dbname > backup-$(date +%Y%m%d).sql
```

### Automated Backups

```bash
# Create backup script
nano /home/user/backup.sh

#!/bin/bash
DATE=$(date +%Y%m%d)
sqlite3 /path/to/database.db .dump > /backups/backup-$DATE.sql
find /backups -name "backup-*.sql" -mtime +30 -delete

# Make executable
chmod +x /home/user/backup.sh

# Add to crontab (daily at 3 AM)
crontab -e
0 3 * * * /home/user/backup.sh
```

## Change Log

### Version 1.0 (October 4, 2025)
- Hardcoded VM IP: 35.212.220.200
- Hardcoded token: weatheryETHAN
- HTTPS protocol enabled
- Test button added for manual testing
- Hourly automatic sync configured

## Future Enhancements

Consider these improvements:

1. **Dynamic Configuration**: Move credentials back to settings.json for easier updates
2. **Domain Name**: Register a domain and point it to the VM IP
3. **Load Balancing**: Add multiple VM instances behind a load balancer
4. **CDN**: Use Cloud CDN for better performance
5. **Database**: Migrate to managed database (Cloud SQL)
6. **Authentication**: Implement JWT tokens instead of static token
7. **Webhooks**: Add real-time Discord webhook integration
8. **Analytics**: Track sleep patterns and trends
9. **Multi-User**: Support multiple Discord users/servers
10. **Mobile App**: Create companion mobile app

## Support

For issues or questions:
1. Check VM logs: `journalctl -u your-app-name -f`
2. Test endpoint manually with curl
3. Verify firewall rules in Google Cloud Console
4. Check Discord bot is running
5. Review this documentation

## Files Modified

- `TFTSleepTracker.Core/Storage/AppSettings.cs`
  - Changed `BotHost` default from `"https://localhost:5000"` to `"https://35.212.220.200"`
  - Changed `Token` default from `""` to `"weatheryETHAN"`

## Rebuilding with Different Credentials

If you need to change the hardcoded values:

1. Edit `TFTSleepTracker.Core/Storage/AppSettings.cs`
2. Change the default values:
   ```csharp
   public string BotHost { get; set; } = "https://NEW-IP-OR-DOMAIN";
   public string Token { get; set; } = "NEW-TOKEN";
   ```
3. Rebuild:
   ```bash
   cd /workspaces/TFT-sleep-tracker
   dotnet publish TFTSleepTracker.App/TFTSleepTracker.App.csproj \
     -c Release -r win-x64 --self-contained true \
     -p:PublishSingleFile=true \
     -p:IncludeNativeLibrariesForSelfExtract=true \
     -o release/
   ```
4. Download the new `release/TFTSleepTracker.exe`

---

**Last Updated**: October 4, 2025  
**Executable Size**: 162 MB  
**Target VM**: 35.212.220.200  
**Protocol**: HTTPS  
**Status**: ✅ Ready for Deployment
