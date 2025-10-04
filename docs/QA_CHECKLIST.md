# QA Checklist - TFT Sleep Tracker

This document provides a comprehensive manual QA checklist for testing the TFT Sleep Tracker application. Use this checklist to validate functionality that cannot be easily automated or requires specific system configurations.

## Core Functionality Tests

### Sleep Calculation Edge Cases

- [ ] **Short Idle Burst Under 60 Minutes**
  - **Steps**: Simulate idle time of 45 minutes within the nightly window [23:00, 08:00)
  - **Expected**: 0 sleep minutes counted (below threshold)
  - **Verification**: Check daily summary shows 0 minutes

- [ ] **Spans That Cross Midnight**
  - **Steps**: Start inactivity at 22:30, remain idle until 08:30
  - **Expected**: Only the portion within [23:00, 08:00) counts = 540 minutes, minus 60 minute threshold = 480 minutes
  - **Verification**: Check CSV logs and summary for correct clipping

- [ ] **Activity Reset Right Before Nightly Window**
  - **Steps**: 
    1. Idle from 22:00 to 22:59
    2. Brief activity at 22:59
    3. Idle from 23:00 to 08:00
  - **Expected**: Only the period from 23:00 onwards counts (480 minutes)
  - **Verification**: First idle period should not contribute to sleep total

- [ ] **Micro Activity at 01:11**
  - **Steps**:
    1. Idle from 23:00 to 01:11 (131 minutes)
    2. Brief activity for 4 minutes
    3. Idle from 01:15 to 08:00 (405 minutes)
  - **Expected**: 
    - First span: 131 - 60 = 71 minutes
    - Second span: 405 - 60 = 345 minutes
    - Total: 416 minutes
  - **Verification**: Check that threshold is applied to each continuous inactivity span separately

### DST Transition Tests

- [ ] **DST Fall-Back Transition (November)**
  - **Test Date**: November 3, 2024 (clocks fall back at 2:00 AM)
  - **Steps**: 
    1. Start inactivity at 23:00 EDT (UTC-4)
    2. Stay idle through the DST transition
    3. End inactivity at 08:00 EST (UTC-5)
  - **Expected**: 
    - Real time elapsed: 10 hours
    - Sleep counted: 9 hours (540 minutes) based on local time window
    - After threshold: 480 minutes
  - **Verification**: CSV should show correct timestamps with offset changes

- [ ] **DST Spring-Forward Transition (March)**
  - **Test Date**: March 10, 2024 (clocks spring forward at 2:00 AM)
  - **Steps**:
    1. Start inactivity at 23:00 EST (UTC-5)
    2. Stay idle through the DST transition (2:00-3:00 AM doesn't exist)
    3. End inactivity at 08:00 EDT (UTC-4)
  - **Expected**:
    - Real time elapsed: 8 hours
    - Sleep counted: 9 hours (540 minutes) based on local time window
    - After threshold: 480 minutes
  - **Verification**: Check that the missing hour is handled correctly

### System Power Event Tests

- [ ] **Laptop Sleep/Hibernate**
  - **Steps**:
    1. Start the app
    2. Put laptop to sleep (Windows Sleep mode)
    3. Wait 1 hour
    4. Resume laptop
  - **Expected**: 
    - App resumes tracking correctly
    - LastInput monitor resets after resume
    - No corrupted CSV files
  - **Verification**: Check Windows Event Log for resume events, verify CSV integrity

- [ ] **Laptop Resume After Long Sleep**
  - **Steps**:
    1. Start the app at 22:00
    2. Hibernate laptop at 23:30
    3. Resume at 07:00 next morning
  - **Expected**:
    - Sleep time calculated for period before hibernate
    - No duplicate entries
    - Summary totals are accurate
  - **Verification**: Check summary.json for correct total

- [ ] **Multiple Sleep/Resume Cycles**
  - **Steps**: Perform 3 sleep/resume cycles during the nightly window
  - **Expected**: Each cycle handled independently, no data corruption
  - **Verification**: CSV shows discrete inactivity spans with resume events

### Session Change Tests

- [ ] **Fast User Switching**
  - **Steps**:
    1. Start app as User A
    2. Switch to User B (without logging out User A)
    3. Switch back to User A
  - **Expected**:
    - App continues tracking for User A's session
    - No tracking when User A is not active
  - **Verification**: Check event logs for session switch handling

- [ ] **RDP Connection**
  - **Steps**:
    1. Start app on physical machine
    2. Connect via RDP from another machine
    3. Disconnect RDP (but don't log off)
    4. Check tracking behavior
  - **Expected**:
    - App handles RDP session correctly
    - Inactivity detection works in RDP session
  - **Verification**: CSV logs should reflect RDP session activity

- [ ] **Lock Screen**
  - **Steps**:
    1. Lock Windows (Win+L)
    2. Wait 2 hours
    3. Unlock
  - **Expected**:
    - Inactivity during lock counts toward sleep if in nightly window
    - App continues running in background
  - **Verification**: Check system tray icon is still present after unlock

## Upload Queue & Network Tests

- [ ] **Queue With Network Offline**
  - **Steps**:
    1. Disable network connection
    2. Wait for daily summary trigger (08:05 AM)
    3. Re-enable network after 10 minutes
  - **Expected**:
    - Summary queued in `%ProgramData%\TFTSleepTracker\queue\`
    - Automatic retry with exponential backoff
    - Upload succeeds after network restored
  - **Verification**: Check queue directory for .json files, verify upload in Discord bot

- [ ] **Queue Purge After 7 Days**
  - **Steps**:
    1. Create test queue files older than 7 days (manually edit timestamps if needed)
    2. Trigger purge mechanism
  - **Expected**: Old files removed, recent files retained
  - **Verification**: Check queue directory contents

- [ ] **Invalid Bot Token**
  - **Steps**:
    1. Configure invalid token in settings.json
    2. Trigger summary upload
  - **Expected**:
    - Upload fails with appropriate error
    - Summary remains in queue for retry
    - Event log shows authentication error
  - **Verification**: Windows Event Viewer → Application → TFTSleepTracker

## UI & System Tray Tests

- [ ] **System Tray Icon Visibility**
  - **Steps**: Start app
  - **Expected**: Tray icon appears in notification area
  - **Verification**: Visual check in system tray

- [ ] **Tray Menu - Open**
  - **Steps**: Right-click tray icon → Open
  - **Expected**: Main window opens with current statistics
  - **Verification**: Window displays correctly

- [ ] **Tray Menu - Send Now**
  - **Steps**: 
    1. Wait until after nightly window (e.g., 10:00 AM)
    2. Right-click tray icon → Send Now
  - **Expected**: Yesterday's summary calculated and uploaded immediately
  - **Verification**: Check upload queue and Discord bot for new entry

- [ ] **Tray Menu - Check for Updates**
  - **Steps**: Right-click tray icon → Check for Updates
  - **Expected**: 
    - App checks GitHub releases
    - Shows "No updates" or downloads update
  - **Verification**: Check behavior (note: requires published releases)

- [ ] **Tray Menu - Quit Background**
  - **Steps**: Right-click tray icon → Quit Background
  - **Expected**: 
    - Confirmation dialog appears
    - App exits completely after confirmation
  - **Verification**: Verify app is not in Task Manager

- [ ] **Close Window Hides to Tray**
  - **Steps**: Click X button on main window
  - **Expected**: Window hides but app stays running in tray
  - **Verification**: Tray icon still present

## Autostart Tests

- [ ] **Enable Autostart**
  - **Steps**:
    1. Open main window
    2. Check "Start with Windows" checkbox
  - **Expected**: Registry entry created at `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\TFTSleepTracker`
  - **Verification**: Check registry key exists

- [ ] **Disable Autostart**
  - **Steps**: Uncheck "Start with Windows"
  - **Expected**: Registry entry removed
  - **Verification**: Check registry key removed

- [ ] **Autostart After Reboot**
  - **Steps**:
    1. Enable autostart
    2. Reboot Windows
  - **Expected**: App starts automatically on login
  - **Verification**: Check tray icon appears after login

## Data Storage & Integrity Tests

- [ ] **CSV File Creation**
  - **Steps**: Run app for one day
  - **Expected**: CSV file created at `%APPDATA%\TFTSleepTracker\YYYY-MM-DD.csv`
  - **Verification**: Open CSV and verify format

- [ ] **CSV File Integrity After Crash**
  - **Steps**:
    1. Start app
    2. Force kill process (Task Manager)
    3. Restart app
  - **Expected**: 
    - No corrupted CSV files
    - App resumes normally
  - **Verification**: Check CSV files are valid

- [ ] **Summary.json Persistence**
  - **Steps**: Generate multiple daily summaries
  - **Expected**: Summary.json contains all historical summaries
  - **Verification**: Open `%APPDATA%\TFTSleepTracker\summary.json`

- [ ] **Settings.json Defaults**
  - **Steps**: 
    1. Delete settings.json
    2. Start app
  - **Expected**:
    - New settings.json created with defaults
    - Device ID auto-generated
  - **Verification**: Check settings.json exists with valid structure

## Performance Tests

- [ ] **CPU Usage During Idle**
  - **Steps**: Run app for 1 hour while monitoring CPU
  - **Expected**: CPU usage <1% during normal operation
  - **Verification**: Task Manager → Performance tab

- [ ] **CPU Usage With Intensive Applications**
  - **Steps**:
    1. Start app
    2. Run CPU-intensive task (video rendering, compilation)
  - **Expected**: App continues functioning without impacting performance
  - **Verification**: Check responsiveness of tray menu

- [ ] **Memory Usage**
  - **Steps**: Run app for 24 hours
  - **Expected**: Memory usage stable (<50 MB), no memory leaks
  - **Verification**: Task Manager → Details → Memory column

## Edge Case Scenarios

- [ ] **Multiple Midnight Crossings**
  - **Steps**: Run app continuously for 3 days
  - **Expected**: Each day's sleep calculated separately and correctly
  - **Verification**: Check CSV files and summaries for all days

- [ ] **Time Change (Manual)**
  - **Steps**:
    1. Start app
    2. Manually change system time by 2 hours
  - **Expected**: App handles time change gracefully
  - **Verification**: No crashes, tracking continues

- [ ] **Disk Full Scenario**
  - **Steps**: Fill disk to <10 MB free space
  - **Expected**: App handles write failures gracefully with retries
  - **Verification**: Check event log for IO errors, verify retry logic

## Installation & Update Tests

- [ ] **Fresh Installation**
  - **Steps**: Install from Squirrel installer
  - **Expected**: 
    - App installed to user directory
    - Start menu shortcut created
    - Settings initialized
  - **Verification**: Check installation directory and shortcuts

- [ ] **Update Installation**
  - **Steps**: Install older version, then update
  - **Expected**:
    - Update applied silently
    - Settings and data preserved
    - App restarts automatically
  - **Verification**: Check version number after update

## Acceptance Criteria Validation

For each test scenario:
1. Record the date and time of testing
2. Note any deviations from expected behavior
3. Capture screenshots or logs for issues
4. Mark test as ✅ Pass or ❌ Fail
5. If failed, document steps to reproduce and file an issue

## Test Environment Requirements

- **OS**: Windows 10 (version 1809+) or Windows 11
- **.NET**: .NET 8.0 Runtime installed
- **Permissions**: Standard user account (not admin)
- **Timezone**: Configure test machine to US Eastern for DST tests
- **Network**: Both connected and disconnected scenarios

## Notes

- Some tests (DST transitions) are time-sensitive and should be scheduled accordingly
- RDP tests require two machines or a VM
- Performance tests should be run on representative hardware (mid-range laptop)
- Always backup test data before destructive tests
