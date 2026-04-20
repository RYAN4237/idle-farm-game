# 在 Unity Editor 中执行所有步骤（不需要重启 Unity）
# 用法：.\build-in-editor.ps1

$projectPath = "C:\Users\Administrator\Idle Game"

Write-Host "🌾⏱ FocusFarm 一键构建（Editor 模式）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n⚠️  请确保 Unity Editor 已打开项目" -ForegroundColor Yellow
Write-Host "然后按任意键继续..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host "`n[1/4] 创建 Timer 场景..." -ForegroundColor Yellow
Write-Host "执行：Window → General → Console 查看输出" -ForegroundColor Gray

# 创建临时 C# 脚本来执行所有步骤
$tempScript = @"
using UnityEditor;
using UnityEngine;

public class AutoBuildAll
{
    [MenuItem("FocusFarm/🚀 一键构建全部")]
    public static void ExecuteAll()
    {
        Debug.Log("=====================================");
        Debug.Log("🌾⏱ 开始自动化构建流程");
        Debug.Log("=====================================");

        // Step 1: 创建 Timer 场景
        Debug.Log("[1/4] 创建 Timer 场景...");
        try
        {
            CreateTimerScene.Execute();
            Debug.Log("✅ Timer 场景创建成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Timer 场景创建失败: " + e.Message);
            return;
        }

        // Step 2: 构建 Farm App
        Debug.Log("[2/4] 构建 Farm App...");
        try
        {
            AppLauncher.BuildFarmApp();
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Farm App 构建失败: " + e.Message);
            return;
        }

        // Step 3: 构建 Timer App
        Debug.Log("[3/4] 构建 Timer App...");
        try
        {
            AppLauncher.BuildTimerApp();
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Timer App 构建失败: " + e.Message);
            return;
        }

        // Step 4: 启动应用
        Debug.Log("[4/4] 启动应用...");
        try
        {
            AppLauncher.LaunchBothApps();
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ 应用启动失败: " + e.Message);
            return;
        }

        Debug.Log("=====================================");
        Debug.Log("🎉 构建完成！应用已启动");
        Debug.Log("Farm 位置: 屏幕底部");
        Debug.Log("Timer 位置: 右下角");
        Debug.Log("=====================================");
    }
}
"@

$tempScriptPath = "$projectPath\Assets\Editor\AutoBuildAll.cs"
Set-Content -Path $tempScriptPath -Value $tempScript -Encoding UTF8

Write-Host "✅ 已创建自动化脚本" -ForegroundColor Green
Write-Host "`n📋 下一步操作：" -ForegroundColor Cyan
Write-Host "1. 在 Unity 中等待脚本编译完成" -ForegroundColor White
Write-Host "2. 菜单栏 → FocusFarm → 🚀 一键构建全部" -ForegroundColor White
Write-Host "`n或者直接在 Unity Console 执行：" -ForegroundColor Cyan
Write-Host "   AutoBuildAll.ExecuteAll();" -ForegroundColor Yellow
Write-Host "`n提示：构建过程需要 3-5 分钟，请耐心等待" -ForegroundColor Gray
