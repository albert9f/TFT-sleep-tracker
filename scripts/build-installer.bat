@echo off
REM TFT Sleep Tracker - Windows Installer Builder
REM Simple batch wrapper for the PowerShell packaging script

setlocal enabledelayedexpansion

echo.
echo ========================================
echo  TFT Sleep Tracker - Package Builder
echo ========================================
echo.

REM Check if version was provided
if "%~1"=="" (
    echo ERROR: Version number required!
    echo.
    echo Usage:
    echo   build-installer.bat [version]
    echo.
    echo Examples:
    echo   build-installer.bat 1.0.0
    echo   build-installer.bat 1.0.1
    echo.
    pause
    exit /b 1
)

set VERSION=%~1

echo Building version: %VERSION%
echo.

REM Check if PowerShell is available
where pwsh >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    echo Using PowerShell Core...
    pwsh -ExecutionPolicy Bypass -File "%~dp0pack-squirrel.ps1" -Version "%VERSION%"
) else (
    where powershell >nul 2>nul
    if %ERRORLEVEL% EQU 0 (
        echo Using Windows PowerShell...
        powershell -ExecutionPolicy Bypass -File "%~dp0pack-squirrel.ps1" -Version "%VERSION%"
    ) else (
        echo ERROR: PowerShell not found!
        echo Please install PowerShell to use this script.
        pause
        exit /b 1
    )
)

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo  BUILD SUCCESSFUL!
    echo ========================================
    echo.
    echo Your installer is ready in: ..\dist\Setup.exe
    echo.
    echo You can now distribute Setup.exe to users.
    echo.
) else (
    echo.
    echo ========================================
    echo  BUILD FAILED!
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
