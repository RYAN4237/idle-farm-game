using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixCatBarWidth
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Try both paths
        Transform catBar = canvas.transform.Find("ExpandablePanel/Middle/CatBar");
        if (catBar == null) catBar = canvas.transform.Find("ExpandablePanel/CatBar");
        if (catBar == null) { Debug.LogError("CatBar not found! Hierarchy:"); return; }

        // Fix VLG - don't expand width
        var catVLG = catBar.GetComponent<VerticalLayoutGroup>();
        if (catVLG != null)
        {
            catVLG.childForceExpandWidth = false;
            EditorUtility.SetDirty(catBar.gameObject);
        }

        // Fix each button - lock at 26px wide
        foreach (Transform child in catBar)
        {
            var le = child.GetComponent<LayoutElement>();
            if (le == null) le = child.gameObject.AddComponent<LayoutElement>();
            le.minWidth       = 26;
            le.preferredWidth = 26;
            le.minHeight      = 26;
            le.preferredHeight= 26;
            EditorUtility.SetDirty(child.gameObject);
            Debug.Log($"Fixed: {child.name} → 26x26");
        }

        // Also fix the CatBar LE
        var catLE = catBar.GetComponent<LayoutElement>();
        if (catLE != null) { catLE.preferredWidth = 28; catLE.minWidth = 28; }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixCatBarWidth done!");
    }
}
