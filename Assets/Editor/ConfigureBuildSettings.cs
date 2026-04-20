using UnityEngine;
using UnityEditor;

public class ConfigureBuildSettings
{
    public static void Execute()
    {
        // ── Player Settings ───────────────────────────────────────────
        // Window mode: Windowed (NOT fullscreen) so we can control the window
        PlayerSettings.fullScreenMode       = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow      = false;

        // Start size: full width, farm strip height
        // Will be overridden at runtime by WindowManager
        PlayerSettings.defaultScreenWidth   = 1920;
        PlayerSettings.defaultScreenHeight  = 160;

        // No splash screen, no Unity logo for ambient app feel
        PlayerSettings.SplashScreen.show    = false;
        PlayerSettings.runInBackground      = true; // keep running when unfocused

        // Product name
        PlayerSettings.productName          = "Focus Farm";
        PlayerSettings.companyName          = "YourStudio";

        // ── Quality: low for ambient ──────────────────────────────────
        QualitySettings.vSyncCount          = 0;
        Application.targetFrameRate         = 15;

        // ── Build target: Windows standalone ─────────────────────────
        // (Timer app can be a separate build if needed later)

        Debug.Log("[BuildSettings] Configured for ambient desktop app");
        Debug.Log("  Mode: Windowed 1920x160");
        Debug.Log("  runInBackground: true");
        Debug.Log("  No splash screen");
        Debug.Log("  Target FPS: 15 (ambient idle)");

        AssetDatabase.SaveAssets();
    }
}
