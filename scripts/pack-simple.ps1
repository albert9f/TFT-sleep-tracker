#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Simple packaging script - just the essential commands

.DESCRIPTION
    Minimal script showing the exact Squirrel commands needed.
    Use this as a reference or if the full script has issues.

.PARAMETER Version
    Version number (e.g., "1.0.0")
#>

param([Parameter(Mandatory=$true)][string]$Version)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot

Write-Host "Building TFT Sleep Tracker v$Version..." -ForegroundColor Cyan

# Clean
Remove-Item "$Root/dist" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$Root/nupkgs" -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory "$Root/dist" | Out-Null
New-Item -ItemType Directory "$Root/nupkgs" | Out-Null

# Publish
Write-Host "[1/3] Publishing app..." -ForegroundColor Green
dotnet publish "$Root/TFTSleepTracker.App/TFTSleepTracker.App.csproj" `
    -c Release `
    -r win-x64 `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=true `
    /p:SelfContained=true `
    /p:Version=$Version

if ($LASTEXITCODE -ne 0) { exit 1 }

$publishDir = "$Root/TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish"

# Pack
Write-Host "[2/3] Creating NuGet package..." -ForegroundColor Green
squirrel pack `
    --framework net8.0-windows `
    --packId TFTSleepTracker `
    --packVersion $Version `
    --packDirectory $publishDir `
    --output "$Root/nupkgs"

if ($LASTEXITCODE -ne 0) { exit 1 }

# Releasify
Write-Host "[3/3] Creating installer..." -ForegroundColor Green
squirrel releasify `
    --package "$Root/nupkgs/TFTSleepTracker.$Version.nupkg" `
    --releaseDir "$Root/dist"

if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "`n✓ Done! Installer is in: $Root/dist/Setup.exe" -ForegroundColor Green
Write-Host "`nDistribute Setup.exe to your users." -ForegroundColor Cyan
