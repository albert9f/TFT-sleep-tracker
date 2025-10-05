@echo off
REM Quick Release Script for Version 1.0.1
REM Double-click this file on Windows to create the Squirrel release

echo.
echo ========================================
echo  TFT Sleep Tracker - Release 1.0.1
echo ========================================
echo.

REM Check if we're in the scripts directory
if exist "pack-squirrel.ps1" (
    echo Running from scripts directory...
    powershell -ExecutionPolicy Bypass -File .\pack-squirrel.ps1 -Version "1.0.1"
) else if exist "scripts\pack-squirrel.ps1" (
    echo Running from project root...
    powershell -ExecutionPolicy Bypass -File .\scripts\pack-squirrel.ps1 -Version "1.0.1"
) else (
    echo ERROR: Cannot find pack-squirrel.ps1 script!
    echo Please run this script from the project root or scripts folder.
    pause
    exit /b 1
)

echo.
echo ========================================
echo  Release packaging complete!
echo ========================================
echo.
echo Next steps:
echo 1. Check the dist/ folder for your release files
echo 2. Upload them to GitHub releases (tag: v1.0.1)
echo.
echo Or run this script with -Upload flag to auto-upload:
echo   .\scripts\pack-squirrel.ps1 -Version "1.0.1" -Upload
echo.
pause
