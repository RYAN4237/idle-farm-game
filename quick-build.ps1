# 一键创建、构建、启动 FocusFarm 双 App
# 这个脚本会直接调用 Unity 执行所有操作

param(
    [switch]$CreateScene,
    [switch]$BuildFarm,
    [switch]$BuildTimer,
    [switch]$Launch,
    [switch]$All
)

$ErrorActionPreference = "Continue"
$projectPath = $PWD
$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe"
$logDir = "Logs"

if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
}

Write-Host "🌾⏱ FocusFarm 自动化脚本" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan

# 检查 Unity 是否存在
if (-not (Test-Path $unityExe)) {
    Write-Host "❌ Unity 未找到: $unityExe" -ForegroundColor Red
    Write-Host "请检查 Unity 安装路径" -ForegroundColor Yellow
    exit 1
}

function Invoke-UnityMethod {
    param(
        [string]$Method,
        [string]$LogName,
        [string]$Description
    )
    
    Write-Host "`n[$Description]" -ForegroundColor Yellow
    $logFile = Join-Path $logDir "$LogName.log"
    
    $arguments = @(
        "-quit",
        "-batchmode",
        "-projectPath", "`"$projectPath`"",
        "-executeMethod", $Method,
        "-logFile", "`"$logFile`""
    )
    
    Write-Host "执行: $Method" -ForegroundColor Gray
    Write-Host "日志: $logFile" -ForegroundColor Gray
    
    $process = Start-Process -FilePath $unityExe -ArgumentList $arguments -NoNewWindow -PassThru -Wait
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ 成功" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ 失败 (退出码: $($process.ExitCode))" -ForegroundColor Red
        
        # 显示日志尾部
        if (Test-Path $logFile) {
            Write-Host "`n最后 20 行日志:" -ForegroundColor Yellow
            Get-Content $logFile -Tail 20 | ForEach-Object {
                if ($_ -match "error|exception|failed") {
                    Write-Host $_ -ForegroundColor Red
                } else {
                    Write-Host $_ -ForegroundColor Gray
                }
            }
        }
        return $false
    }
}

function Test-SceneExists {
    param([string]$ScenePath)
    
    $fullPath = Join-Path $projectPath $ScenePath
    return Test-Path $fullPath
}

function Test-ExeExists {
    param([string]$ExePath)
    
    $fullPath = Join-Path $projectPath $ExePath
    if (Test-Path $fullPath) {
        $info = Get-Item $fullPath
        $sizeMB = [math]::Round($info.Length / 1MB, 1)
        Write-Host "   找到: $ExePath ($sizeMB MB)" -ForegroundColor Green
        return $true
    }
    return $false
}

# ═══════════════════════════════════════════════════════
# 主流程
# ═══════════════════════════════════════════════════════

if ($All) {
    $CreateScene = $true
    $BuildFarm = $true
    $BuildTimer = $true
    $Launch = $true
}

# 步骤 1: 创建 Timer 场景
if ($CreateScene -or $All) {
    $result = Invoke-UnityMethod -Method "CreateTimerScene.Execute" -LogName "CreateScene" -Description "创建 Timer 场景"
    if (-not $result) {
        Write-Host "`n⚠️  场景创建失败，但继续尝试..." -ForegroundColor Yellow
    }
    
    Start-Sleep -Seconds 2
    
    if (Test-SceneExists "Assets\Scenes\TimerScene.unity") {
        Write-Host "✅ TimerScene.unity 已创建" -ForegroundColor Green
    } else {
        Write-Host "❌ TimerScene.unity 未找到" -ForegroundColor Red
    }
}

# 步骤 2: 构建 Farm App
if ($BuildFarm -or $All) {
    $result = Invoke-UnityMethod -Method "AppLauncher.BuildFarmApp" -LogName "BuildFarm" -Description "构建 Farm App"
    if ($result) {
        $null = Test-ExeExists "Build\FarmApp.exe"
    }
}

# 步骤 3: 构建 Timer App
if ($BuildTimer -or $All) {
    $result = Invoke-UnityMethod -Method "AppLauncher.BuildTimerApp" -LogName "BuildTimer" -Description "构建 Timer App"
    if ($result) {
        $null = Test-ExeExists "Build\TimerApp.exe"
    }
}

# 步骤 4: 启动应用
if ($Launch -or $All) {
    Write-Host "`n[启动应用]" -ForegroundColor Yellow
    
    $farmExe = "Build\FarmApp.exe"
    $timerExe = "Build\TimerApp.exe"
    
    if (-not (Test-Path $farmExe)) {
        Write-Host "❌ FarmApp.exe 不存在，请先构建" -ForegroundColor Red
    } elseif (-not (Test-Path $timerExe)) {
        Write-Host "❌ TimerApp.exe 不存在，请先构建" -ForegroundColor Red
    } else {
        Write-Host "启动 Farm App..." -ForegroundColor Gray
        Start-Process -FilePath $farmExe
        
        Start-Sleep -Seconds 2
        
        Write-Host "启动 Timer App..." -ForegroundColor Gray
        Start-Process -FilePath $timerExe
        
        Write-Host "`n✅ 应用已启动！" -ForegroundColor Green
        Write-Host "   Farm: 屏幕底部" -ForegroundColor White
        Write-Host "   Timer: 右下角" -ForegroundColor White
    }
}

Write-Host "`n======================================" -ForegroundColor Cyan
Write-Host "完成！" -ForegroundColor Green
Write-Host "======================================`n" -ForegroundColor Cyan
