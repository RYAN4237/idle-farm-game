using UnityEngine;
using UnityEditor;
using System.IO;

/// 诊断工具：检查构建前置条件
public class BuildDiagnostics
{
    [MenuItem("FocusFarm/🔍 检查构建状态")]
    public static void CheckBuildStatus()
    {
        Debug.Log("=====================================");
        Debug.Log("🔍 FocusFarm 构建状态检查");
        Debug.Log("=====================================");

        // 1. 检查场景文件
        Debug.Log("\n[场景文件]");
        CheckScene("Assets/Scenes/DesktopIdleGame.unity", "Farm App");
        CheckScene("Assets/Scenes/TimerScene.unity", "Timer App");

        // 2. 检查构建目录
        Debug.Log("\n[构建目录]");
        string buildDir = Path.Combine(Application.dataPath, "../Build");
        if (Directory.Exists(buildDir))
        {
            Debug.Log($"✅ Build 目录存在: {buildDir}");
            
            // 检查已构建的 exe
            CheckExe(Path.Combine(buildDir, "FarmApp.exe"), "Farm App");
            CheckExe(Path.Combine(buildDir, "TimerApp.exe"), "Timer App");
        }
        else
        {
            Debug.LogWarning($"⚠️  Build 目录不存在，将在构建时自动创建");
        }

        // 3. 检查关键脚本
        Debug.Log("\n[关键脚本]");
        CheckScript("Assets/Editor/CreateTimerScene.cs");
        CheckScript("Assets/Editor/AppLauncher.cs");
        CheckScript("Assets/Editor/AutoBuildAll.cs");
        CheckScript("Assets/Scripts/Systems/FocusSystem.cs");
        CheckScript("Assets/Scripts/Systems/WindowManager.cs");
        CheckScript("Assets/Scripts/Systems/TimerWindowManager.cs");

        // 4. 检查 TextMeshPro
        Debug.Log("\n[依赖检查]");
        var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        if (tmpType != null)
            Debug.Log("✅ TextMeshPro 已安装");
        else
            Debug.LogError("❌ TextMeshPro 未安装，请导入：Window → TextMeshPro → Import TMP Essential Resources");

        Debug.Log("\n=====================================");
        Debug.Log("检查完成！");
        Debug.Log("=====================================");
    }

    static void CheckScene(string path, string name)
    {
        if (File.Exists(path))
        {
            var info = new FileInfo(path);
            Debug.Log($"✅ {name} 场景存在: {path}");
            Debug.Log($"   大小: {info.Length / 1024} KB");
            Debug.Log($"   修改时间: {info.LastWriteTime}");
        }
        else
        {
            Debug.LogError($"❌ {name} 场景不存在: {path}");
            if (name == "Timer App")
                Debug.LogError("   → 运行: FocusFarm → 1. Create Timer Scene");
        }
    }

    static void CheckExe(string path, string name)
    {
        if (File.Exists(path))
        {
            var info = new FileInfo(path);
            Debug.Log($"   ✅ {name} 已构建: {Path.GetFileName(path)}");
            Debug.Log($"      大小: {info.Length / (1024 * 1024)} MB");
            Debug.Log($"      构建时间: {info.LastWriteTime}");
        }
        else
        {
            Debug.Log($"   ⚠️  {name} 未构建");
        }
    }

    static void CheckScript(string path)
    {
        if (File.Exists(path))
            Debug.Log($"   ✅ {Path.GetFileName(path)}");
        else
            Debug.LogWarning($"   ⚠️  {Path.GetFileName(path)} 不存在");
    }
}
