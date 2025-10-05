#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packages TFT Sleep Tracker for release using Squirrel (official CLI)

.DESCRIPTION
    This script automates the full Squirrel packaging workflow:
    1. Builds the solution in Release mode
    2. Publishes the app as a self-contained executable
    3. Creates a NuGet package
    4. Releasifies the package to create Setup.exe and update files
    5. Optionally uploads artifacts to GitHub Releases

.PARAMETER Version
    The version number for this release (e.g., "1.0.0")

.PARAMETER Upload
    If specified, uploads the release to GitHub using gh CLI

.PARAMETER SkipTests
    If specified, skips running tests

.EXAMPLE
    .\pack-squirrel.ps1 -Version "1.0.0"
    
.EXAMPLE
    .\pack-squirrel.ps1 -Version "1.0.0" -Upload

.EXAMPLE
    .\pack-squirrel.ps1 -Version "1.0.0" -SkipTests
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [switch]$Upload,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

# Validate version format (e.g., 1.0.0 or 1.0.0-beta)
if ($Version -notmatch '^\d+\.\d+\.\d+(-\w+)?$') {
    Write-Error "Version must be in format X.Y.Z or X.Y.Z-suffix (e.g., 1.0.0 or 1.0.0-beta)"
    exit 1
}

$RepoRoot = Split-Path -Parent $PSScriptRoot
$OutputDir = Join-Path $RepoRoot "dist"
$NupkgDir = Join-Path $RepoRoot "nupkgs"
$AppProject = Join-Path $RepoRoot "TFTSleepTracker.App/TFTSleepTracker.App.csproj"
$PublishDir = Join-Path $RepoRoot "TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish"

Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║        TFT Sleep Tracker - Squirrel Release Packager         ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan

Write-Host "Version: " -NoNewline -ForegroundColor White
Write-Host $Version -ForegroundColor Yellow
Write-Host "Runtime: " -NoNewline -ForegroundColor White
Write-Host "win-x64 (self-contained)" -ForegroundColor Yellow
Write-Host ""

# Step 0: Check prerequisites
Write-Host "[0/7] Checking prerequisites..." -ForegroundColor Green

# Check if dotnet is available
$dotnetInstalled = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnetInstalled) {
    Write-Error ".NET SDK is not installed. Install from https://dotnet.microsoft.com/download"
    exit 1
}

# Check if Squirrel CLI is installed
$squirrelInstalled = Get-Command squirrel -ErrorAction SilentlyContinue
if (-not $squirrelInstalled) {
    Write-Host "  Squirrel CLI not found. Installing..." -ForegroundColor Yellow
    dotnet tool install --global Squirrel
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install Squirrel CLI. Run: dotnet tool install --global Squirrel"
        exit 1
    }
    Write-Host "  ✓ Squirrel CLI installed" -ForegroundColor Green
} else {
    Write-Host "  ✓ Squirrel CLI found" -ForegroundColor Green
}

Write-Host "  ✓ .NET SDK found: " -NoNewline -ForegroundColor Green
dotnet --version
Write-Host ""

# Step 1: Clean previous builds
Write-Host "[1/7] Cleaning previous builds..." -ForegroundColor Green
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
    Write-Host "  Removed: $OutputDir" -ForegroundColor Gray
}
if (Test-Path $NupkgDir) {
    Remove-Item -Path $NupkgDir -Recurse -Force
    Write-Host "  Removed: $NupkgDir" -ForegroundColor Gray
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $NupkgDir -Force | Out-Null
Write-Host "  ✓ Clean complete" -ForegroundColor Green
Write-Host ""

# Step 2: Restore NuGet packages
Write-Host "[2/7] Restoring NuGet packages..." -ForegroundColor Green
Push-Location $RepoRoot
try {
    dotnet restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed"
    }
    Write-Host "  ✓ Packages restored" -ForegroundColor Green
}
finally {
    Pop-Location
}
Write-Host ""

# Step 3: Build in Release mode
Write-Host "[3/7] Building solution in Release mode..." -ForegroundColor Green
Push-Location $RepoRoot
try {
    dotnet build --configuration Release --no-restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed"
    }
    Write-Host "  ✓ Build successful" -ForegroundColor Green
}
finally {
    Pop-Location
}
Write-Host ""

# Step 4: Run tests (optional)
if (-not $SkipTests) {
    Write-Host "[4/7] Running tests..." -ForegroundColor Green
    Push-Location $RepoRoot
    try {
        dotnet test --configuration Release --no-build --verbosity quiet
        if ($LASTEXITCODE -ne 0) {
            throw "Tests failed"
        }
        Write-Host "  ✓ All tests passed" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host "[4/7] Skipping tests..." -ForegroundColor Yellow
}
Write-Host ""

# Step 5: Publish as self-contained
Write-Host "[5/7] Publishing self-contained executable..." -ForegroundColor Green

Push-Location $RepoRoot
try {
    dotnet publish $AppProject `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --output $PublishDir `
        /p:PublishSingleFile=true `
        /p:PublishTrimmed=true `
        /p:Version=$Version
    
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed"
    }
    
    $publishSize = (Get-ChildItem $PublishDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "  ✓ Published to: $PublishDir" -ForegroundColor Green
    Write-Host "  Size: $([math]::Round($publishSize, 2)) MB" -ForegroundColor Gray
}
finally {
    Pop-Location
}
Write-Host ""

# Step 6: Create NuGet package with Squirrel
Write-Host "[6/7] Creating NuGet package..." -ForegroundColor Green

Push-Location $RepoRoot
try {
    # Use squirrel pack to create the NuGet package
    squirrel pack `
        --framework net8.0-windows `
        --packId TFTSleepTracker `
        --packVersion $Version `
        --packDirectory $PublishDir `
        --output $NupkgDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "Squirrel pack failed"
    }
    
    $nupkgFile = Get-ChildItem -Path $NupkgDir -Filter "*.nupkg" | Select-Object -First 1
    Write-Host "  ✓ NuGet package created: $($nupkgFile.Name)" -ForegroundColor Green
}
finally {
    Pop-Location
}
Write-Host ""

# Step 7: Releasify to create installer
Write-Host "[7/7] Creating installer with Squirrel..." -ForegroundColor Green

Push-Location $RepoRoot
try {
    $nupkgPath = Get-ChildItem -Path $NupkgDir -Filter "TFTSleepTracker.$Version.nupkg" | Select-Object -First 1
    
    if (-not $nupkgPath) {
        throw "NuGet package not found: TFTSleepTracker.$Version.nupkg"
    }
    
    squirrel releasify `
        --package $nupkgPath.FullName `
        --releaseDir $OutputDir
    
    if ($LASTEXITCODE -ne 0) {
        throw "Squirrel releasify failed"
    }
    
    Write-Host "  ✓ Installer created successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}
Write-Host ""

# Display results
Write-Host @"
╔═══════════════════════════════════════════════════════════════╗
║                    PACKAGING COMPLETE! ✓                      ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

Write-Host "Release artifacts in: " -NoNewline -ForegroundColor White
Write-Host $OutputDir -ForegroundColor Cyan
Write-Host ""

Get-ChildItem -Path $OutputDir -File | ForEach-Object {
    $sizeKB = [math]::Round($_.Length / 1KB, 0)
    Write-Host "  📦 " -NoNewline -ForegroundColor Yellow
    Write-Host "$($_.Name) " -NoNewline -ForegroundColor White
    Write-Host "($sizeKB KB)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Key files:" -ForegroundColor White
Write-Host "  • Setup.exe        - Distribute this installer to users" -ForegroundColor Cyan
Write-Host "  • RELEASES         - Required for auto-updates" -ForegroundColor Cyan
Write-Host "  • *.nupkg          - Delta/full update packages" -ForegroundColor Cyan
Write-Host ""

# Step 8: Upload to GitHub (optional)
if ($Upload) {
    Write-Host @"
╔═══════════════════════════════════════════════════════════════╗
║                  UPLOADING TO GITHUB RELEASES                 ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Cyan
    
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
        --notes "Release v$Version - Windows Installer`n`nDownload Setup.exe to install the application." `
        --repo albert9f/TFT-sleep-tracker
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create GitHub release"
    }
    Write-Host "  ✓ Release created" -ForegroundColor Green

    # Upload artifacts
    Write-Host "Uploading artifacts..." -ForegroundColor Yellow
    Get-ChildItem -Path $OutputDir -File | ForEach-Object {
        gh release upload $Tag $_.FullName --repo albert9f/TFT-sleep-tracker
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to upload $($_.Name)"
        }
        Write-Host "  ✓ Uploaded: $($_.Name)" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Release published successfully! 🎉" -ForegroundColor Green
    Write-Host "View at: " -NoNewline -ForegroundColor White
    Write-Host "https://github.com/albert9f/TFT-sleep-tracker/releases/tag/$Tag" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host @"

╔═══════════════════════════════════════════════════════════════╗
║                         ALL DONE! ✓                           ║
╚═══════════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green
