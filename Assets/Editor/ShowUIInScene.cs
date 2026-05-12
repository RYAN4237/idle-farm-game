using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

/// Temporarily moves ExpandablePanel to SHOWN position so we can see it in scene view,
/// and frames the scene view to show the full game including UI.
public class ShowUIInScene
{
    [MenuItem("Farm/Show UI In Scene (Preview)")]
    public static void ShowPanel()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) return;

        var panelT = canvasGO.transform.Find("ExpandablePanel");
        if (panelT != null)
        {
            var rt = panelT.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, 100f); // shown
            EditorUtility.SetDirty(rt);
            Debug.Log("[ShowUI] Panel shown at x=0");
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Repaint
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    [MenuItem("Farm/Hide UI Panel (Reset)")]
    public static void HidePanel()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) return;

        var panelT = canvasGO.transform.Find("ExpandablePanel");
        if (panelT != null)
        {
            var rt = panelT.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(260f, 100f); // hidden
            EditorUtility.SetDirty(rt);
        }

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
