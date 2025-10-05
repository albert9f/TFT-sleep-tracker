#!/usr/bin/env pwsh
<#
.SYNOPSIS
    One-click release script for TFT Sleep Tracker v1.0.1

.DESCRIPTION
    This script packages and optionally uploads version 1.0.1 to GitHub.
    It's a convenience wrapper around pack-squirrel.ps1 with the version pre-filled.

.PARAMETER Upload
    If specified, automatically uploads to GitHub releases after building

.PARAMETER SkipTests
    If specified, skips running tests during the build

.EXAMPLE
    .\release-1.0.1.ps1
    
.EXAMPLE
    .\release-1.0.1.ps1 -Upload

.EXAMPLE
    .\release-1.0.1.ps1 -SkipTests -Upload
#>

param(
    [Parameter(Mandatory=$false)]
    [switch]$Upload,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

$Version = "1.0.1"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PackScript = Join-Path $ScriptDir "pack-squirrel.ps1"

Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║           TFT Sleep Tracker - Release v$Version              ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

if (-not (Test-Path $PackScript)) {
    Write-Error "Cannot find pack-squirrel.ps1 at: $PackScript"
    exit 1
}

Write-Host "Starting release process for version $Version..." -ForegroundColor Yellow
Write-Host ""

# Build arguments for pack-squirrel.ps1
$PackArgs = @{
    Version = $Version
}

if ($Upload) {
    $PackArgs['Upload'] = $true
    Write-Host "  ✓ Will upload to GitHub after build" -ForegroundColor Green
}

if ($SkipTests) {
    $PackArgs['SkipTests'] = $true
    Write-Host "  ✓ Will skip tests" -ForegroundColor Yellow
}

Write-Host ""

# Run the packaging script
& $PackScript @PackArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Packaging failed!"
    exit $LASTEXITCODE
}

Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║                  RELEASE v$Version COMPLETE! ✓               ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

if (-not $Upload) {
    Write-Host ""
    Write-Host "Release files are ready in the dist/ folder!" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To upload to GitHub, run:" -ForegroundColor White
    Write-Host "  .\release-1.0.1.ps1 -Upload" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Or manually upload the dist/ folder contents to:" -ForegroundColor White
    Write-Host "  https://github.com/albert9f/TFT-sleep-tracker/releases/new" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "Auto-updates will roll out to users on 1.0.0 within 7 days! 🎉" -ForegroundColor Green
Write-Host ""
