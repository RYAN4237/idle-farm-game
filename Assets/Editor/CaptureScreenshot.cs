using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CaptureScreenshot
{
    [MenuItem("Farm/Capture Screenshot")]
    public static void Capture()
    {
        string path = "/Users/I755634/farm_screenshot.png";
        ScreenCapture.CaptureScreenshot(path, 2);
        Debug.Log($"[Screenshot] Saved to {path}");
    }

    [MenuItem("Farm/Save Scene Now")]
    public static void SaveScene()
    {
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[SaveScene] Saved.");
    }
}
