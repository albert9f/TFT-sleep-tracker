#!/usr/bin/env node
/**
 * Simple Express server for receiving TFT Sleep Tracker data
 * 
 * Install: npm install express
 * Run: node vm-server-example.js
 */

const express = require('express');
const app = express();

// Middleware to parse JSON bodies
app.use(express.json());

// Log all incoming requests
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.path} - ${req.ip}`);
  next();
});

// Health check endpoint
app.get('/health', (req, res) => {
  res.status(200).json({ status: 'healthy', timestamp: new Date().toISOString() });
});

// Sleep data ingestion endpoint
app.post('/ingest-sleep', (req, res) => {
  try {
    // 1. Check authentication token
    const token = req.query.token;
    if (token !== 'weatheryETHAN') {
      console.log(`❌ Unauthorized attempt with token: ${token}`);
      return res.status(401).json({ error: 'Unauthorized' });
    }

    // 2. Extract payload
    const { deviceId, date, sleepMinutes, computedAt } = req.body;

    // 3. Validate required fields
    if (!deviceId || !date || sleepMinutes === undefined) {
      console.log(`❌ Missing required fields in payload:`, req.body);
      return res.status(400).json({ error: 'Missing required fields' });
    }

    // 4. Log the received data (in production, save to database here)
    console.log(`✅ Received sleep data:`);
    console.log(`   Device: ${deviceId}`);
    console.log(`   Date: ${date}`);
    console.log(`   Sleep: ${sleepMinutes} minutes (${(sleepMinutes / 60).toFixed(1)} hours)`);
    console.log(`   Computed: ${computedAt}`);

    // 5. TODO: Save to your database
    // await database.saveSleepData({ deviceId, date, sleepMinutes, computedAt });

    // 6. Return success
    res.status(200).json({ 
      success: true, 
      message: 'Sleep data received',
      received: {
        deviceId,
        date,
        sleepMinutes,
        hours: (sleepMinutes / 60).toFixed(1)
      }
    });
  } catch (error) {
    console.error('❌ Error processing sleep data:', error);
    res.status(500).json({ error: 'Internal server error' });
  }
});

// Start server
const PORT = process.env.PORT || 80;
app.listen(PORT, '0.0.0.0', () => {
  console.log('╔══════════════════════════════════════════════════════════════╗');
  console.log('║                                                              ║');
  console.log('║           TFT Sleep Tracker Server Running! 🌙              ║');
  console.log('║                                                              ║');
  console.log('╚══════════════════════════════════════════════════════════════╝');
  console.log('');
  console.log(`✅ Server listening on port ${PORT}`);
  console.log(`📍 Endpoint: http://YOUR_VM_IP/ingest-sleep`);
  console.log(`🔑 Token: weatheryETHAN`);
  console.log('');
  console.log('Test with:');
  console.log(`curl -X POST "http://localhost/ingest-sleep?token=weatheryETHAN" \\`);
  console.log(`  -H "Content-Type: application/json" \\`);
  console.log(`  -d '{"deviceId":"test","date":"2025-10-04","sleepMinutes":120}'`);
  console.log('');
  console.log('Press Ctrl+C to stop');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
});

// Graceful shutdown
process.on('SIGTERM', () => {
  console.log('Shutting down gracefully...');
  process.exit(0);
});
