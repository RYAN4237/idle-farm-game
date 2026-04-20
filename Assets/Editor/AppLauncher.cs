using UnityEngine;
using UnityEditor;
using System.IO;

/// Launcher: starts both Farm and Timer apps
/// User only needs to click once
public class AppLauncher
{
    [MenuItem("FocusFarm/▶ Launch Both Apps")]
    public static void LaunchBothApps()
    {
        string buildFolder = Path.Combine(Application.dataPath, "../Build");
        string farmExe = Path.Combine(buildFolder, "FarmApp.exe");
        string timerExe = Path.Combine(buildFolder, "TimerApp.exe");

        if (!File.Exists(farmExe))
        {
            Debug.LogError($"FarmApp.exe not found at {farmExe}");
            Debug.LogError("Build Farm scene first!");
            return;
        }

        if (!File.Exists(timerExe))
        {
            Debug.LogError($"TimerApp.exe not found at {timerExe}");
            Debug.LogError("Build Timer scene first!");
            return;
        }

        // Start Farm first (底部条)
        System.Diagnostics.Process.Start(farmExe);
        Debug.Log("✅ Farm App started");

        // Wait 1s, then start Timer (右下角卡片)
        System.Threading.Thread.Sleep(1000);
        System.Diagnostics.Process.Start(timerExe);
        Debug.Log("✅ Timer App started");

        Debug.Log("🌾⏱ Both apps launched!");
    }

    [MenuItem("FocusFarm/2. Build Farm App")]
    public static void BuildFarmApp()
    {
        string scenePath = "Assets/Scenes/DesktopIdleGame.unity";
        
        // 验证场景文件是否存在
        if (!File.Exists(scenePath))
        {
            Debug.LogError($"❌ 场景文件不存在: {scenePath}");
            Debug.LogError("请确保场景已创建或路径正确");
            return;
        }
        
        string[] scenes = { scenePath };
        string path = "Build/FarmApp.exe";
        
        // 确保 Build 文件夹存在
        string buildDir = Path.GetDirectoryName(path);
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
            Debug.Log($"创建构建目录: {buildDir}");
        }
        
        Debug.Log($"开始构建 Farm App...");
        Debug.Log($"场景: {scenePath}");
        Debug.Log($"输出: {path}");
        
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Farm App 构建成功: {path}");
            Debug.Log($"   大小: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"   耗时: {report.summary.totalTime.TotalSeconds:F1} 秒");
        }
        else
        {
            Debug.LogError($"❌ Farm App 构建失败: {report.summary.result}");
            if (report.summary.totalErrors > 0)
                Debug.LogError($"   错误数: {report.summary.totalErrors}");
            if (report.summary.totalWarnings > 0)
                Debug.LogWarning($"   警告数: {report.summary.totalWarnings}");
        }
    }

    [MenuItem("FocusFarm/3. Build Timer App")]
    public static void BuildTimerApp()
    {
        string scenePath = "Assets/Scenes/TimerScene.unity";
        
        // 验证场景文件是否存在
        if (!File.Exists(scenePath))
        {
            Debug.LogError($"❌ 场景文件不存在: {scenePath}");
            Debug.LogError("请先运行: FocusFarm → 1. Create Timer Scene");
            return;
        }
        
        string[] scenes = { scenePath };
        string path = "Build/TimerApp.exe";
        
        // 确保 Build 文件夹存在
        string buildDir = Path.GetDirectoryName(path);
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
            Debug.Log($"创建构建目录: {buildDir}");
        }
        
        Debug.Log($"开始构建 Timer App...");
        Debug.Log($"场景: {scenePath}");
        Debug.Log($"输出: {path}");
        
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"✅ Timer App 构建成功: {path}");
            Debug.Log($"   大小: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"   耗时: {report.summary.totalTime.TotalSeconds:F1} 秒");
        }
        else
        {
            Debug.LogError($"❌ Timer App 构建失败: {report.summary.result}");
            if (report.summary.totalErrors > 0)
                Debug.LogError($"   错误数: {report.summary.totalErrors}");
            if (report.summary.totalWarnings > 0)
                Debug.LogWarning($"   警告数: {report.summary.totalWarnings}");
        }
    }
}
