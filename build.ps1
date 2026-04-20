# FocusFarm 自动化构建脚本
# 用法：在 PowerShell 中运行 .\build.ps1

$ErrorActionPreference = "Stop"

# ── 配置 ──────────────────────────────────────────────────────
$projectPath = "C:\Users\Administrator\Idle Game"
$unityPath = "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe"  # 自动查找
$buildFolder = "$projectPath\Build"

Write-Host "🌾⏱ FocusFarm 自动化构建脚本" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# ── 1. 查找 Unity 编辑器 ──────────────────────────────────────
Write-Host "`n[1/5] 查找 Unity 编辑器..." -ForegroundColor Yellow

$unityExe = Get-ChildItem -Path "C:\Program Files\Unity\Hub\Editor" -Filter "Unity.exe" -Recurse -ErrorAction SilentlyContinue | 
    Select-Object -First 1 -ExpandProperty FullName

if (-not $unityExe) {
    Write-Host "❌ 未找到 Unity.exe，请手动指定路径" -ForegroundColor Red
    exit 1
}

Write-Host "✅ 找到 Unity: $unityExe" -ForegroundColor Green

# ── 2. 创建 Timer 场景 ────────────────────────────────────────
Write-Host "`n[2/5] 创建 Timer 场景..." -ForegroundColor Yellow

$createSceneArgs = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "CreateTimerScene.Execute",
    "-logFile", "$projectPath\Logs\CreateTimerScene.log"
)

Write-Host "执行命令: Unity.exe $($createSceneArgs -join ' ')" -ForegroundColor Gray

& $unityExe $createSceneArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Timer 场景创建成功" -ForegroundColor Green
} else {
    Write-Host "⚠️ Timer 场景创建可能失败，检查日志: Logs\CreateTimerScene.log" -ForegroundColor Yellow
}

# ── 3. 构建 Farm App ──────────────────────────────────────────
Write-Host "`n[3/5] 构建 Farm App..." -ForegroundColor Yellow

$buildFarmArgs = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "AppLauncher.BuildFarmApp",
    "-logFile", "$projectPath\Logs\BuildFarm.log"
)

& $unityExe $buildFarmArgs

if ($LASTEXITCODE -eq 0 -and (Test-Path "$buildFolder\FarmApp.exe")) {
    Write-Host "✅ Farm App 构建成功: Build\FarmApp.exe" -ForegroundColor Green
} else {
    Write-Host "❌ Farm App 构建失败，检查日志: Logs\BuildFarm.log" -ForegroundColor Red
    exit 1
}

# ── 4. 构建 Timer App ─────────────────────────────────────────
Write-Host "`n[4/5] 构建 Timer App..." -ForegroundColor Yellow

$buildTimerArgs = @(
    "-quit",
    "-batchmode",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "AppLauncher.BuildTimerApp",
    "-logFile", "$projectPath\Logs\BuildTimer.log"
)

& $unityExe $buildTimerArgs

if ($LASTEXITCODE -eq 0 -and (Test-Path "$buildFolder\TimerApp.exe")) {
    Write-Host "✅ Timer App 构建成功: Build\TimerApp.exe" -ForegroundColor Green
} else {
    Write-Host "❌ Timer App 构建失败，检查日志: Logs\BuildTimer.log" -ForegroundColor Red
    exit 1
}

# ── 5. 启动双 App ─────────────────────────────────────────────
Write-Host "`n[5/5] 启动应用..." -ForegroundColor Yellow

$farmExe = "$buildFolder\FarmApp.exe"
$timerExe = "$buildFolder\TimerApp.exe"

if (-not (Test-Path $farmExe)) {
    Write-Host "❌ FarmApp.exe 不存在" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $timerExe)) {
    Write-Host "❌ TimerApp.exe 不存在" -ForegroundColor Red
    exit 1
}

Write-Host "启动 Farm App..." -ForegroundColor Gray
Start-Process -FilePath $farmExe

Start-Sleep -Seconds 2

Write-Host "启动 Timer App..." -ForegroundColor Gray
Start-Process -FilePath $timerExe

Write-Host "`n=================================" -ForegroundColor Cyan
Write-Host "🎉 完成！应用已启动" -ForegroundColor Green
Write-Host "`nFarm 位置: 屏幕底部" -ForegroundColor White
Write-Host "Timer 位置: 右下角" -ForegroundColor White
Write-Host "`n提示：关闭应用后可重新运行此脚本" -ForegroundColor Gray
