using UnityEngine;
using UnityEditor;

public class DiagnoseScrollView
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var path = "ExpandablePanel/ScrollView";
        var sv = canvas?.transform.Find(path);
        if (sv == null) { Debug.LogError("ScrollView not found"); return; }

        // Print bounds of key objects
        void PrintRT(Transform t, string label)
        {
            if (t == null) { Debug.Log($"{label}: NULL"); return; }
            var rt = t.GetComponent<RectTransform>();
            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var rect = rt.rect;
            Debug.Log($"{label}: rect={rect.width:F0}x{rect.height:F0} " +
                      $"BL={corners[0]:F0} TR={corners[2]:F0} " +
                      $"anchors={rt.anchorMin}-{rt.anchorMax} " +
                      $"offset={rt.offsetMin}/{rt.offsetMax}");
        }

        PrintRT(sv, "ScrollView");
        PrintRT(sv.Find("Viewport"), "Viewport");
        PrintRT(sv.Find("Viewport/Content"), "Content");
        PrintRT(sv.Find("Viewport/Content/SeedGrid"), "SeedGrid");
        var grid = sv.Find("Viewport/Content/SeedGrid");
        if (grid != null && grid.childCount > 0)
            PrintRT(grid.GetChild(0), "Cell[0]");

        var scroll = sv.GetComponent<UnityEngine.UI.ScrollRect>();
        if (scroll != null)
            Debug.Log($"ScrollRect: content={scroll.content?.name}, viewport={scroll.viewport?.name}");
    }
}
