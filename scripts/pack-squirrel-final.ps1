param([string]$Version = "1.0.1")

$ErrorActionPreference = "Stop"
$RepoRoot = "C:\Users\Workdesk\TFT-sleep-tracker"
$PublishDir = "$RepoRoot\TFTSleepTracker.App\bin\Release\net8.0-windows\win-x64\publish"
$OutputDir = "$RepoRoot\dist"
$NupkgDir = "$RepoRoot\nupkgs"

Write-Host "Creating Squirrel release for v$Version..." -ForegroundColor Cyan

# Clean
if (Test-Path $OutputDir) { Remove-Item $OutputDir -Recurse -Force }
if (Test-Path $NupkgDir) { Remove-Item $NupkgDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path $NupkgDir -Force | Out-Null

# Build and publish
Write-Host "[1/3] Building and publishing..." -ForegroundColor Green
dotnet publish "$RepoRoot\TFTSleepTracker.App\TFTSleepTracker.App.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:Version=$Version -v:q

# Create nuspec
Write-Host "[2/3] Creating NuGet package..." -ForegroundColor Green
$nuspec = @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>TFTSleepTracker</id>
    <version>$Version</version>
    <title>TFT Sleep Tracker</title>
    <authors>albert9f</authors>
    <description>Sleep tracking application</description>
  </metadata>
  <files>
    <file src="lib\**" target="lib" />
  </files>
</package>
"@
Set-Content -Path "$RepoRoot\TFTSleepTracker.nuspec" -Value $nuspec

# Copy to lib folder
$LibDir = "$RepoRoot\lib\net8.0-windows"
if (Test-Path "$RepoRoot\lib") { Remove-Item "$RepoRoot\lib" -Recurse -Force }
New-Item -ItemType Directory -Path $LibDir -Force | Out-Null
Copy-Item -Path "$PublishDir\*" -Destination $LibDir -Recurse -Force

# Pack with nuget
& "$env:TEMP\nuget.exe" pack "$RepoRoot\TFTSleepTracker.nuspec" -OutputDirectory $NupkgDir -NoPackageAnalysis -NoDefaultExcludes

# Clean temp files
Remove-Item "$RepoRoot\lib" -Recurse -Force
Remove-Item "$RepoRoot\TFTSleepTracker.nuspec"

# Releasify with Squirrel
Write-Host "[3/3] Creating Squirrel release..." -ForegroundColor Green
$nupkg = Get-ChildItem $NupkgDir -Filter "*.nupkg" | Select-Object -First 1
& "$env:TEMP\Squirrel\Squirrel.exe" --releasify $nupkg.FullName --releaseDir $OutputDir --no-msi

Write-Host ""
Write-Host "Release complete!" -ForegroundColor Green
Write-Host "Files in $OutputDir :" -ForegroundColor Yellow
Get-ChildItem $OutputDir -File | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor White }
Write-Host ""
Write-Host "Upload these files to GitHub release v$Version" -ForegroundColor Cyan
