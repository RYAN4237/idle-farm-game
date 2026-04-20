# 关闭所有 FocusFarm 进程
# 用法：.\stop.ps1

Write-Host "🛑 停止 FocusFarm 应用..." -ForegroundColor Yellow

$processes = @("FarmApp", "TimerApp")
$stopped = $false

foreach ($proc in $processes) {
    $running = Get-Process -Name $proc -ErrorAction SilentlyContinue
    if ($running) {
        Stop-Process -Name $proc -Force
        Write-Host "✅ 已停止 $proc" -ForegroundColor Green
        $stopped = $true
    }
}

if (-not $stopped) {
    Write-Host "ℹ️  没有运行中的 FocusFarm 进程" -ForegroundColor Gray
} else {
    Write-Host "`n🎉 所有应用已停止" -ForegroundColor Cyan
}
