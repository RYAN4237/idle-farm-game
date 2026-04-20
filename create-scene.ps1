# Create Timer Scene - Simple Version
$projectPath = Get-Location
$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe"

Write-Host "Creating Timer Scene..." -ForegroundColor Cyan

$logFile = "Logs\CreateScene.log"
New-Item -ItemType Directory -Path "Logs" -Force | Out-Null

& $unityExe -quit -batchmode -projectPath "$projectPath" -executeMethod "CreateTimerScene.Execute" -logFile "$logFile"

Write-Host "Checking result..." -ForegroundColor Yellow

if (Test-Path "Assets\Scenes\TimerScene.unity") {
    Write-Host "SUCCESS! Timer scene created!" -ForegroundColor Green
} else {
    Write-Host "FAILED. Check log:" -ForegroundColor Red
    if (Test-Path $logFile) {
        Get-Content $logFile -Tail 30
    }
}
