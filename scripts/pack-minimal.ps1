param([Parameter(Mandatory=$true)][string]$Version)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$OutputDir = Join-Path $RepoRoot "dist"
$AppProject = Join-Path $RepoRoot "TFTSleepTracker.App/TFTSleepTracker.App.csproj"
$PublishDir = Join-Path $RepoRoot "TFTSleepTracker.App/bin/Release/net8.0-windows/win-x64/publish"

Write-Host "Building TFT Sleep Tracker v$Version with Squirrel..." -ForegroundColor Cyan

# Clean
if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Build
Write-Host "[1/3] Building..." -ForegroundColor Green
dotnet build $RepoRoot\TFTSleepTracker.sln -c Release -v:q
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Publish  
Write-Host "[2/3] Publishing..." -ForegroundColor Green
dotnet publish $AppProject -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:Version=$Version -v:q
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# Copy to release folder with simplified structure for Squirrel
Write-Host "[3/3] Creating release package..." -ForegroundColor Green
$ReleaseDir = Join-Path $OutputDir "app-$Version"
New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null
Copy-Item -Path "$PublishDir\*" -Destination $ReleaseDir -Recurse -Force

# Create a simple RELEASES file manually
$ReleasesContent = @"
TFTSleepTracker-$Version-full.nupkg
"@
Set-Content -Path (Join-Path $OutputDir "RELEASES") -Value $ReleasesContent

# Copy main exe as Setup.exe
Copy-Item -Path "$ReleaseDir\TFTSleepTracker.exe" -Destination (Join-Path $OutputDir "Setup.exe")

Write-Host ""
Write-Host "Release ready in: $OutputDir" -ForegroundColor Green
Write-Host "Files created:" -ForegroundColor Yellow
Get-ChildItem $OutputDir -File | ForEach-Object { Write-Host "  - $($_.Name)" }
Write-Host ""
