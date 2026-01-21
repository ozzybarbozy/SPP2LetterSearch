# SPP2 Letter Search - Single Instance Launcher
# This script ensures only ONE window opens

# Stop any existing instances
$existing = Get-Process SPP2LetterSearch -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "Closing existing instances..." -ForegroundColor Yellow
    $existing | Stop-Process -Force
    Start-Sleep -Milliseconds 500
}

# Launch fresh instance
$exePath = "c:\LetterMaster\SPP2LetterSearch\bin\Debug\net8.0-windows\SPP2LetterSearch.exe"

if (Test-Path $exePath) {
    Write-Host "Launching SPP2 Letter Search..." -ForegroundColor Green
    Start-Process -FilePath $exePath
} else {
    Write-Host "Executable not found. Building first..." -ForegroundColor Yellow
    cd "c:\LetterMaster\SPP2LetterSearch"
    dotnet build
    Start-Process -FilePath $exePath
}
