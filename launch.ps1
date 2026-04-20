# 快速启动脚本（仅启动已构建的 App）
# 用法：.\launch.ps1

$buildFolder = "C:\Users\Administrator\Idle Game\Build"
$farmExe = "$buildFolder\FarmApp.exe"
$timerExe = "$buildFolder\TimerApp.exe"

Write-Host "🚀 启动 FocusFarm..." -ForegroundColor Cyan

if (-not (Test-Path $farmExe)) {
    Write-Host "❌ FarmApp.exe 不存在，请先运行构建脚本" -ForegroundColor Red
    Write-Host "   运行: .\build.ps1" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $timerExe)) {
    Write-Host "❌ TimerApp.exe 不存在，请先运行构建脚本" -ForegroundColor Red
    Write-Host "   运行: .\build.ps1" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ 启动 Farm App (底部条)" -ForegroundColor Green
Start-Process -FilePath $farmExe

Start-Sleep -Seconds 1

Write-Host "✅ 启动 Timer App (右下角)" -ForegroundColor Green
Start-Process -FilePath $timerExe

Write-Host "`n🎉 应用已启动！" -ForegroundColor Cyan
