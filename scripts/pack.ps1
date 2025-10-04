#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packages TFT Sleep Tracker for release using Clowd.Squirrel

.DESCRIPTION
    This script:
    1. Builds the solution in Release mode
    2. Creates a Squirrel release package using Clowd.Squirrel
    3. Optionally uploads artifacts to GitHub Releases

.PARAMETER Version
    The version number for this release (e.g., "1.0.0")

.PARAMETER Upload
    If specified, uploads the release to GitHub using gh CLI

.EXAMPLE
    .\pack.ps1 -Version "1.0.0"
    
.EXAMPLE
    .\pack.ps1 -Version "1.0.0" -Upload
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [switch]$Upload
)

$ErrorActionPreference = "Stop"

# Validate version format (e.g., 1.0.0 or 1.0.0-beta)
if ($Version -notmatch '^\d+\.\d+\.\d+(-\w+)?$') {
    Write-Error "Version must be in format X.Y.Z or X.Y.Z-suffix (e.g., 1.0.0 or 1.0.0-beta)"
    exit 1
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
$OutputDir = Join-Path $RepoRoot "releases"
$AppProject = Join-Path $RepoRoot "TFTSleepTracker.App/TFTSleepTracker.App.csproj"

Write-Host "TFT Sleep Tracker Release Packager" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host ""

# Step 1: Clean previous builds
Write-Host "[1/6] Cleaning previous builds..." -ForegroundColor Green
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null

# Step 2: Restore NuGet packages
Write-Host "[2/6] Restoring NuGet packages..." -ForegroundColor Green
Push-Location $RepoRoot
try {
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed"
    }
}
finally {
    Pop-Location
}

# Step 3: Build in Release mode
Write-Host "[3/6] Building solution in Release mode..." -ForegroundColor Green
Push-Location $RepoRoot
try {
    dotnet build --configuration Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed"
    }
}
finally {
    Pop-Location
}

# Step 4: Run tests
Write-Host "[4/6] Running tests..." -ForegroundColor Green
Push-Location $RepoRoot
try {
    dotnet test --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed"
    }
}
finally {
    Pop-Location
}

# Step 5: Package with Clowd.Squirrel
Write-Host "[5/6] Creating Squirrel release package..." -ForegroundColor Green

$PublishDir = Join-Path $RepoRoot "TFTSleepTracker.App/bin/Release/net8.0-windows/publish"

# Publish the app
Push-Location $RepoRoot
try {
    dotnet publish $AppProject `
        --configuration Release `
        --output $PublishDir `
        --no-build `
        --self-contained false
    
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed"
    }
}
finally {
    Pop-Location
}

# Install Clowd.Squirrel CLI tool if not already installed
Write-Host "Installing Clowd.Squirrel CLI tool..." -ForegroundColor Yellow
dotnet tool install --global Clowd.Squirrel --version 2.11.1 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    # Tool might already be installed, try updating
    dotnet tool update --global Clowd.Squirrel --version 2.11.1 2>&1 | Out-Null
}

# Create Squirrel release
Write-Host "Packaging with Squirrel..." -ForegroundColor Yellow
$SquirrelArgs = @(
    "pack",
    "--packId", "TFTSleepTracker",
    "--packVersion", $Version,
    "--packDirectory", $PublishDir,
    "--releaseDir", $OutputDir
)

& squirrel @SquirrelArgs

if ($LASTEXITCODE -ne 0) {
    throw "Squirrel packaging failed"
}

Write-Host "[6/6] Package created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Release artifacts are in: $OutputDir" -ForegroundColor Cyan
Get-ChildItem -Path $OutputDir | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor Gray
}
Write-Host ""

# Step 6: Upload to GitHub (optional)
if ($Upload) {
    Write-Host "[UPLOAD] Uploading to GitHub Releases..." -ForegroundColor Green
    
    # Check if gh CLI is installed
    $ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $ghInstalled) {
        Write-Error "GitHub CLI (gh) is not installed. Install it from https://cli.github.com/"
        exit 1
    }

    $Tag = "v$Version"
    
    # Create release
    Write-Host "Creating GitHub release $Tag..." -ForegroundColor Yellow
    gh release create $Tag `
        --title "TFT Sleep Tracker v$Version" `
        --notes "Release v$Version" `
        --repo albert9f/TFT-sleep-tracker
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub release"
    }

    # Upload artifacts
    Write-Host "Uploading artifacts..." -ForegroundColor Yellow
    Get-ChildItem -Path $OutputDir -File | ForEach-Object {
        gh release upload $Tag $_.FullName --repo albert9f/TFT-sleep-tracker
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to upload $($_.Name)"
        }
        Write-Host "  Uploaded: $($_.Name)" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Release published successfully!" -ForegroundColor Green
    Write-Host "View at: https://github.com/albert9f/TFT-sleep-tracker/releases/tag/$Tag" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Done! ✓" -ForegroundColor Green
