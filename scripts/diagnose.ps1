#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Diagnostic script for Squirrel packaging environment

.DESCRIPTION
    Checks your environment and reports any issues that might prevent
    successful packaging of the TFT Sleep Tracker installer.
#>

$ErrorActionPreference = "Continue"

Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║         TFT Sleep Tracker - Environment Diagnostics          ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

# 1. .NET SDK Check
Write-Host "[1] Checking .NET SDK..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ .NET SDK installed: $dotnetVersion" -ForegroundColor Green
        
        if ($dotnetVersion -match '^(\d+)\.') {
            $major = [int]$Matches[1]
            if ($major -lt 8) {
                Write-Host "  ⚠ Warning: .NET 8 SDK recommended (you have $dotnetVersion)" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "  ✗ .NET SDK not found or error" -ForegroundColor Red
    }
} catch {
    Write-Host "  ✗ .NET SDK not found" -ForegroundColor Red
    Write-Host "    Install from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
}

# 2. Squirrel CLI Check
Write-Host "`n[2] Checking Squirrel CLI..." -ForegroundColor Green
try {
    $squirrelVersion = squirrel --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Squirrel CLI installed" -ForegroundColor Green
        Write-Host "    Version info: $squirrelVersion" -ForegroundColor Gray
    } else {
        Write-Host "  ✗ Squirrel CLI not found" -ForegroundColor Red
        Write-Host "    Install: dotnet tool install --global Squirrel" -ForegroundColor Yellow
    }
} catch {
    Write-Host "  ✗ Squirrel CLI not found" -ForegroundColor Red
    Write-Host "    Install: dotnet tool install --global Squirrel" -ForegroundColor Yellow
}

# 3. PowerShell Version
Write-Host "`n[3] Checking PowerShell..." -ForegroundColor Green
Write-Host "  ✓ PowerShell version: $($PSVersionTable.PSVersion)" -ForegroundColor Green
Write-Host "    Edition: $($PSVersionTable.PSEdition)" -ForegroundColor Gray

# 4. Operating System
Write-Host "`n[4] Checking Operating System..." -ForegroundColor Green
if ($IsWindows -or $env:OS -match "Windows") {
    Write-Host "  ✓ Running on Windows" -ForegroundColor Green
    try {
        $osInfo = Get-CimInstance Win32_OperatingSystem -ErrorAction SilentlyContinue
        if ($osInfo) {
            Write-Host "    OS: $($osInfo.Caption)" -ForegroundColor Gray
            Write-Host "    Version: $($osInfo.Version)" -ForegroundColor Gray
            Write-Host "    Architecture: $($osInfo.OSArchitecture)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "    (Could not get detailed OS info)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ⚠ Not running on Windows" -ForegroundColor Yellow
    Write-Host "    WPF apps require Windows to build" -ForegroundColor Yellow
}

# 5. Current Directory
Write-Host "`n[5] Checking Current Directory..." -ForegroundColor Green
$currentDir = Get-Location
Write-Host "  Current path: $currentDir" -ForegroundColor Gray
Write-Host "  Path length: $($currentDir.Path.Length) characters" -ForegroundColor Gray

if ($currentDir.Path.Length -gt 200) {
    Write-Host "  ⚠ Warning: Path is long (may cause issues)" -ForegroundColor Yellow
    Write-Host "    Consider moving repository closer to root (e.g., C:\code\TFT\)" -ForegroundColor Yellow
}

# 6. Repository Structure
Write-Host "`n[6] Checking Repository Structure..." -ForegroundColor Green
$requiredFiles = @(
    "pack-squirrel.ps1",
    "pack-simple.ps1",
    "build-installer.bat",
    "..\TFTSleepTracker.App\TFTSleepTracker.App.csproj",
    "..\TFTSleepTracker.sln"
)

$allPresent = $true
foreach ($file in $requiredFiles) {
    if (Test-Path $file) {
        Write-Host "  ✓ Found: $file" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Missing: $file" -ForegroundColor Red
        $allPresent = $false
    }
}

if (-not $allPresent) {
    Write-Host "  ⚠ Some files missing - are you in the scripts/ folder?" -ForegroundColor Yellow
}

# 7. Disk Space
Write-Host "`n[7] Checking Disk Space..." -ForegroundColor Green
try {
    $drive = Get-PSDrive C -ErrorAction SilentlyContinue
    if ($drive) {
        $freeGB = [math]::Round($drive.Free / 1GB, 2)
        Write-Host "  Free space: $freeGB GB" -ForegroundColor Gray
        
        if ($freeGB -lt 1) {
            Write-Host "  ⚠ Warning: Low disk space (need ~1 GB for build)" -ForegroundColor Yellow
        } else {
            Write-Host "  ✓ Sufficient disk space" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "  ⚠ Could not check disk space" -ForegroundColor Yellow
}

# 8. .NET Global Tools Path
Write-Host "`n[8] Checking .NET Global Tools Path..." -ForegroundColor Green
$pathDirs = $env:PATH -split [IO.Path]::PathSeparator
$dotnetToolsPath = Join-Path $env:USERPROFILE ".dotnet\tools"

if ($pathDirs -contains $dotnetToolsPath -or $pathDirs -like "*$dotnetToolsPath*") {
    Write-Host "  ✓ .NET tools path in PATH" -ForegroundColor Green
    Write-Host "    Path: $dotnetToolsPath" -ForegroundColor Gray
} else {
    Write-Host "  ⚠ .NET tools path not in PATH" -ForegroundColor Yellow
    Write-Host "    Expected: $dotnetToolsPath" -ForegroundColor Yellow
    Write-Host "    Add to PATH and restart PowerShell" -ForegroundColor Yellow
}

# 9. GitHub CLI (optional)
Write-Host "`n[9] Checking GitHub CLI (optional)..." -ForegroundColor Green
try {
    $ghVersion = gh --version 2>&1 | Select-Object -First 1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ GitHub CLI installed: $ghVersion" -ForegroundColor Green
        Write-Host "    (Required for -Upload flag)" -ForegroundColor Gray
    } else {
        Write-Host "  ○ GitHub CLI not installed (optional)" -ForegroundColor Gray
        Write-Host "    Install from: https://cli.github.com/" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ○ GitHub CLI not installed (optional)" -ForegroundColor Gray
    Write-Host "    Install from: https://cli.github.com/" -ForegroundColor Gray
}

# 10. Test Build (optional check)
Write-Host "`n[10] Checking if solution builds..." -ForegroundColor Green
Write-Host "    (Skipping - run manually: dotnet build)" -ForegroundColor Gray

# Summary
Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║                            SUMMARY                            ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

$issues = @()

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $issues += "Install .NET 8 SDK"
}
if (-not (Get-Command squirrel -ErrorAction SilentlyContinue)) {
    $issues += "Install Squirrel CLI: dotnet tool install --global Squirrel"
}
if (-not ($IsWindows -or $env:OS -match "Windows")) {
    $issues += "Must run on Windows to build WPF apps"
}
if ($currentDir.Path.Length -gt 200) {
    $issues += "Consider shorter path for repository"
}

if ($issues.Count -eq 0) {
    Write-Host "✓ All checks passed! You're ready to build." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next step: Run this command to create installer:" -ForegroundColor White
    Write-Host "  .\pack-squirrel.ps1 -Version `"1.0.0`"" -ForegroundColor Cyan
} else {
    Write-Host "⚠ Issues found that may prevent building:" -ForegroundColor Yellow
    Write-Host ""
    foreach ($issue in $issues) {
        Write-Host "  • $issue" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Fix these issues and run diagnostics again." -ForegroundColor White
}

Write-Host ""
Write-Host "For more help, see: TROUBLESHOOTING.md" -ForegroundColor Gray
Write-Host ""
