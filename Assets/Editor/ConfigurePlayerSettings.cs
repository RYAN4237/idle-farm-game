using UnityEngine;
using UnityEditor;

public class ConfigurePlayerSettings
{
    public static void Execute()
    {
        // Window size: full screen width × slim bar height
        PlayerSettings.defaultScreenWidth  = 1920;
        PlayerSettings.defaultScreenHeight = 220;

        // Borderless windowed
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

        // No taskbar icon (handled by WS_EX_TOOLWINDOW in TransparentWindow.cs)
        PlayerSettings.visibleInBackground = true;

        // App name
        PlayerSettings.productName = "Focus Farm";

        // No splash screen
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.SplashScreen.showUnityLogo = false;

        // Resizable off (fixed bar size)
        PlayerSettings.resizableWindow = false;

        AssetDatabase.SaveAssets();
        Debug.Log("Player Settings configured: 1920×220, Windowed, no border.");
    }
}
