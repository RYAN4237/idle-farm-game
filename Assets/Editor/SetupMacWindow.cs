using UnityEngine;
using UnityEditor;

public class SetupMacWindow
{
    public static void Execute()
    {
        // Add MacOSWindowManager to GameManager if not already present
        var gm = GameObject.Find("GameManager");
        if (gm == null) { Debug.LogError("GameManager not found"); return; }

        if (gm.GetComponent<MacOSWindowManager>() == null)
        {
            var mgr = gm.AddComponent<MacOSWindowManager>();
            mgr.barHeight    = 200;
            mgr.bottomOffset = 0;
            Debug.Log("[Setup] Added MacOSWindowManager to GameManager");
        }
        else
        {
            Debug.Log("[Setup] MacOSWindowManager already present");
        }

        // Configure Player Settings for macOS transparent window
        PlayerSettings.fullScreenMode      = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth  = 1920;
        PlayerSettings.defaultScreenHeight = 200;
        PlayerSettings.resizableWindow     = false;
        PlayerSettings.runInBackground     = true;

        // Set camera background to transparent
        var cam = Camera.main;
        if (cam != null)
        {
            var so = new UnityEditor.SerializedObject(cam);
            so.FindProperty("m_ClearFlags").intValue    = 2; // SolidColor
            so.FindProperty("m_BackGroundColor").colorValue = new Color(0,0,0,0);
            so.ApplyModifiedProperties();
            Debug.Log("[Setup] Camera background set to transparent (0,0,0,0)");
        }

        EditorUtility.SetDirty(gm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[Setup] Done. Player Settings: Windowed 1920x200, runInBackground=true");
    }
}
