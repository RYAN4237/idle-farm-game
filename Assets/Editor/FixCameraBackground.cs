using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// Adds a full-screen background Image to UICanvas so the game background
/// color is visible even in UI-only screenshots.
public class FixCameraBackground
{
    [MenuItem("Farm/Fix Camera Background")]
    public static void Execute()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) { Debug.LogError("UICanvas not found"); return; }

        // Remove old background if exists
        var oldBg = canvasGO.transform.Find("Background");
        if (oldBg != null) Object.DestroyImmediate(oldBg.gameObject);

        // Create full-screen background (behind everything, sibling index 0)
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        bgGO.transform.SetSiblingIndex(0); // behind all other UI

        var rt = bgGO.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = bgGO.AddComponent<Image>();
        // Match camera background color: grass green
        img.color = new Color(0.47f, 0.71f, 0.34f, 1f);
        img.raycastTarget = false; // don't block clicks

        EditorUtility.SetDirty(bgGO);

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixCameraBackground] Added grass-green background to UICanvas.");
    }
}
