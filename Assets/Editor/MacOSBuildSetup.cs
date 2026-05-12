using UnityEngine;
using UnityEditor;

/// Configures macOS Player Settings for transparent borderless window.
public class MacOSBuildSetup
{
    [MenuItem("Tools/Setup macOS Transparent Window")]
    public static void Setup()
    {
        // macOS: enable transparent window (requires Metal)
        PlayerSettings.macOS.targetOSVersion = "10.14";

        // Allow transparent background
        PlayerSettings.runInBackground = true;

        // Set window to borderless/transparent in Player Settings
        // The actual transparency is done via Objective-C runtime in MacOSWindowManager
        // but we need to set these build settings:

        // Resolution & Presentation
        PlayerSettings.defaultIsNativeResolution = true;
        PlayerSettings.resizableWindow = false;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth  = 1920;
        PlayerSettings.defaultScreenHeight = 200;

        // macOS specific: disable the default window chrome in menu bar
        // This must be done at runtime via NSWindow API (MacOSWindowManager.cs)

        Debug.Log("[Setup] macOS Player Settings configured for transparent bottom bar.");
        Debug.Log("  defaultScreenWidth=1920, defaultScreenHeight=200, Windowed mode");
        Debug.Log("  Make sure URP Camera background color = (0,0,0,0)");
        Debug.Log("  Add MacOSWindowManager to a persistent GameObject in the scene.");
    }
}
