param(
    [string]$TvAddress = "192.168.0.121:26101"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$tizen = "D:\Software\Tizen\TizenStudio\tools\ide\bin\tizen"
$npm = "D:\Software\nodejs\npm.cmd"
$backend = Join-Path (Split-Path -Parent $root) "mitube.service"

Write-Host "=== MiTube Tizen - Deploy ===" -ForegroundColor Cyan

# Step 1: Build webpack
Write-Host "`n[1/5] Building webpack bundle..." -ForegroundColor Yellow
Push-Location $root
$env:PATH = "D:\Software\nodejs;$env:PATH"
& $npm run build 2>&1
if ($LASTEXITCODE -ne 0) { throw "webpack failed" }
Write-Host "  OK" -ForegroundColor Green

# Step 2: Clean and build Tizen web
Write-Host "[2/5] Running tizen build-web..." -ForegroundColor Yellow
if (Test-Path "$root\.buildResult") {
    Remove-Item -LiteralPath "$root\.buildResult" -Recurse -Force
}
& $tizen build-web -- $root 2>&1
if ($LASTEXITCODE -ne 0) { throw "tizen build-web failed" }
Write-Host "  OK" -ForegroundColor Green

# Step 3: Remove node_modules from build (not excluded automatically)
Write-Host "[3/5] Removing node_modules from .buildResult..." -ForegroundColor Yellow
Remove-Item -LiteralPath "$root\.buildResult\node_modules" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "  OK" -ForegroundColor Green

# Step 4: Sign and package wgt
Write-Host "[4/5] Packaging signed .wgt..." -ForegroundColor Yellow
& $tizen package -t wgt -s ADn -- "$root\.buildResult" 2>&1
if ($LASTEXITCODE -ne 0) { throw "tizen package failed" }
Write-Host "  OK" -ForegroundColor Green

# Step 5: Install on TV
Write-Host "[5/5] Installing on TV ($TvAddress)..." -ForegroundColor Yellow
& $tizen install -s $TvAddress -n "$root\.buildResult\MiTube.wgt" 2>&1
if ($LASTEXITCODE -ne 0) { throw "tizen install failed" }
Write-Host "  OK" -ForegroundColor Green

Write-Host "`n=== Deploy complete ===" -ForegroundColor Cyan
Pop-Location
