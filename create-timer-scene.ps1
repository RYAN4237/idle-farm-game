# 简化版自动化脚本 - 创建 Timer 场景
$projectPath = Get-Location
$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe"

Write-Host "创建 Timer 场景..." -ForegroundColor Cyan

$logFile = "Logs\CreateScene.log"
New-Item -ItemType Directory -Path "Logs" -Force | Out-Null

& $unityExe `
    -quit `
    -batchmode `
    -projectPath "$projectPath" `
    -executeMethod "CreateTimerScene.Execute" `
    -logFile "$logFile"

Write-Host "Unity 执行完成，检查结果..." -ForegroundColor Yellow

if (Test-Path "Assets\Scenes\TimerScene.unity") {
    Write-Host "✅ Timer 场景创建成功！" -ForegroundColor Green
} else {
    Write-Host "❌ 场景创建失败，查看日志:" -ForegroundColor Red
    if (Test-Path $logFile) {
        Get-Content $logFile -Tail 30
    }
}
