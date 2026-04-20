using UnityEngine;
using UnityEditor;

public class FixCanvasGroups
{
    public static void Execute()
    {
        // 找所有有CanvasGroup的UI对象，把alpha恢复为1（除了ExpandablePanel）
        var allCG = Object.FindObjectsOfType<CanvasGroup>();
        foreach (var cg in allCG)
        {
            var path = GetPath(cg.gameObject);
            Debug.Log($"CanvasGroup found: {path}, alpha={cg.alpha}");
            // 不动ExpandablePanel（它用alpha控制面板动画）
            if (!path.Contains("ExpandablePanel"))
            {
                cg.alpha = 1f;
                EditorUtility.SetDirty(cg.gameObject);
                Debug.Log($"  → Reset to alpha=1");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixCanvasGroups done!");
    }

    static string GetPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }
}
