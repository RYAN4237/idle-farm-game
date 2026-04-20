using UnityEditor;
using UnityEngine;

/// 一键执行所有构建步骤
/// 菜单：FocusFarm → 🚀 一键构建全部
public class AutoBuildAll
{
    [MenuItem("FocusFarm/🚀 一键构建全部")]
    public static void ExecuteAll()
    {
        Debug.Log("=====================================");
        Debug.Log("🌾⏱ 开始自动化构建流程");
        Debug.Log("=====================================");

        // Step 0: 诊断检查
        Debug.Log("\n[0/4] 运行诊断检查...");
        BuildDiagnostics.CheckBuildStatus();

        // Step 1: 创建 Timer 场景
        Debug.Log("\n[1/4] 创建 Timer 场景...");
        try
        {
            CreateTimerScene.Execute();
            Debug.Log("✅ Timer 场景创建成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Timer 场景创建失败: {e.Message}");
            Debug.LogError("详细错误: " + e.ToString());
            return;
        }

        // Step 2: 构建 Farm App
        Debug.Log("\n[2/4] 构建 Farm App（需要 1-2 分钟）...");
        try
        {
            AppLauncher.BuildFarmApp();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Farm App 构建失败: {e.Message}");
            Debug.LogError("详细错误: " + e.ToString());
            return;
        }

        // Step 3: 构建 Timer App
        Debug.Log("\n[3/4] 构建 Timer App（需要 1-2 分钟）...");
        try
        {
            AppLauncher.BuildTimerApp();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Timer App 构建失败: {e.Message}");
            Debug.LogError("详细错误: " + e.ToString());
            return;
        }

        // Step 4: 启动应用
        Debug.Log("\n[4/4] 启动应用...");
        try
        {
            AppLauncher.LaunchBothApps();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 应用启动失败: {e.Message}");
            Debug.LogError("详细错误: " + e.ToString());
            return;
        }

        Debug.Log("\n=====================================");
        Debug.Log("🎉 构建完成！应用已启动");
        Debug.Log("Farm 位置: 屏幕底部");
        Debug.Log("Timer 位置: 右下角");
        Debug.Log("=====================================");
    }

    [MenuItem("FocusFarm/📋 仅创建场景")]
    public static void OnlyCreateScene()
    {
        Debug.Log("[Step 1] 创建 Timer 场景...");
        CreateTimerScene.Execute();
    }

    [MenuItem("FocusFarm/🔨 仅构建 Apps")]
    public static void OnlyBuild()
    {
        Debug.Log("[Step 1/2] 构建 Farm App...");
        AppLauncher.BuildFarmApp();
        Debug.Log("[Step 2/2] 构建 Timer App...");
        AppLauncher.BuildTimerApp();
        Debug.Log("✅ 构建完成");
    }

    [MenuItem("FocusFarm/▶ 仅启动 Apps")]
    public static void OnlyLaunch()
    {
        AppLauncher.LaunchBothApps();
    }
}
