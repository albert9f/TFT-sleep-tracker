param([string]$Version = "1.0.1")

$RepoRoot = "C:\Users\Workdesk\TFT-sleep-tracker"
$WorkDir = "$RepoRoot\squirrel-build"
$PublishDir = "$RepoRoot\TFTSleepTracker.App\bin\Release\net8.0-windows\win-x64\publish"

Write-Host "=== Creating Squirrel Package v$Version ===" -ForegroundColor Cyan

# Clean and create work directory
if (Test-Path $WorkDir) { Remove-Item $WorkDir -Recurse -Force }
New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null

# Step 1: Build
Write-Host "`n[1/4] Building application..." -ForegroundColor Green
dotnet publish "$RepoRoot\TFTSleepTracker.App\TFTSleepTracker.App.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:Version=$Version `
    -v:minimal

if ($LASTEXITCODE -ne 0) { 
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1 
}

# Step 2: Create NuGet package structure
Write-Host "`n[2/4] Creating NuGet package..." -ForegroundColor Green
$LibDir = "$WorkDir\lib\net8.0-windows"
New-Item -ItemType Directory -Path $LibDir -Force | Out-Null
Copy-Item -Path "$PublishDir\*" -Destination $LibDir -Recurse -Force

# Create nuspec
$nuspecContent = @"
<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>TFTSleepTracker</id>
    <version>$Version</version>
    <title>TFT Sleep Tracker</title>
    <authors>albert9f</authors>
    <description>TFT Sleep Tracker Application</description>
  </metadata>
  <files>
    <file src="lib\**" target="lib" />
  </files>
</package>
"@
Set-Content -Path "$WorkDir\TFTSleepTracker.nuspec" -Value $nuspecContent

# Step 3: Pack with NuGet
Write-Host "`n[3/4] Packing with NuGet..." -ForegroundColor Green
Push-Location $WorkDir
& "$env:TEMP\nuget.exe" pack TFTSleepTracker.nuspec -NoPackageAnalysis -NoDefaultExcludes
Pop-Location

$nupkg = Get-ChildItem $WorkDir -Filter "*.nupkg" -File | Select-Object -First 1
if (!$nupkg) { 
    Write-Host "ERROR: NuGet package not created!" -ForegroundColor Red
    exit 1 
}
Write-Host "   Created: $($nupkg.Name)" -ForegroundColor Yellow

# Step 4: Releasify with Squirrel
Write-Host "`n[4/4] Creating Squirrel release..." -ForegroundColor Green
$ReleaseDir = "$WorkDir\Releases"
New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null

& "$env:TEMP\Squirrel\Squirrel.exe" --releasify $nupkg.FullName -r $ReleaseDir --no-msi

if ($LASTEXITCODE -ne 0) { 
    Write-Host "ERROR: Squirrel releasify failed!" -ForegroundColor Red
    exit 1 
}

# Show results
Write-Host "`n=== SUCCESS! ===" -ForegroundColor Green
Write-Host "`nRelease files created in: $ReleaseDir" -ForegroundColor Cyan
Write-Host "`nFiles to upload to GitHub:" -ForegroundColor Yellow
Get-ChildItem $ReleaseDir -File | ForEach-Object { 
    $sizeMB = [math]::Round($_.Length / 1MB, 2)
    Write-Host "  - $($_.Name) ($sizeMB MB)" -ForegroundColor White 
}

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "  1. Upload ALL files from $ReleaseDir to GitHub release v$Version"
Write-Host "  2. Make sure the release is PUBLISHED (not draft)"
Write-Host "  3. Users on v1.0.0 will auto-update within 7 days"
