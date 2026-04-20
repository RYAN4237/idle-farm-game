using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixScrollPanel
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var panel  = canvas?.transform.Find("ExpandablePanel");
        if (panel == null) { Debug.LogError("Panel not found"); return; }

        // ── Add vertical scrollbar ────────────────────────────────────
        var scrollView = panel.Find("ScrollView");
        if (scrollView == null) { Debug.LogError("ScrollView not found"); return; }

        var scroll = scrollView.GetComponent<ScrollRect>();

        // Add a scrollbar GO
        var oldSB = scrollView.Find("Scrollbar");
        if (oldSB != null) Object.DestroyImmediate(oldSB.gameObject);

        var sbGO = new GameObject("Scrollbar");
        sbGO.transform.SetParent(scrollView, false);
        var sbRT = sbGO.AddComponent<RectTransform>();
        // Right edge, full height
        sbRT.anchorMin = new Vector2(1,0); sbRT.anchorMax = new Vector2(1,1);
        sbRT.pivot     = new Vector2(1,0.5f);
        sbRT.sizeDelta = new Vector2(8, 0);
        sbRT.anchoredPosition = Vector2.zero;

        var sbImg = sbGO.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#3a2a10", out Color sbBg);
        sbImg.color = sbBg;

        var sb = sbGO.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;

        // Scrollbar handle
        var handle = new GameObject("Handle");
        handle.transform.SetParent(sbGO.transform, false);
        var hRT = handle.AddComponent<RectTransform>();
        hRT.anchorMin = Vector2.zero; hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(1,0); hRT.offsetMax = new Vector2(-1,0);
        var hImg = handle.AddComponent<Image>();
        ColorUtility.TryParseHtmlString("#8a6a20", out Color hCol);
        hImg.color = hCol;
        sb.handleRect = hRT;
        sb.targetGraphic = hImg;

        scroll.verticalScrollbar = sb;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scroll.verticalScrollbarSpacing = -3;

        EditorUtility.SetDirty(scrollView.gameObject);

        // ── Fix grid cell size to fit 3 columns in ~160px width ───────
        var grid = panel.Find("ScrollView/Viewport/Content/SeedGrid");
        if (grid != null)
        {
            var glg = grid.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                // Panel=200px, catbar=34px, scrollbar=8px, padding=4px
                // Available = 200-34-8-4 = 154px
                // 3 cols with 3px gap: (154 - 2*3) / 3 = 49px per cell
                glg.cellSize = new Vector2(49, 54);
                glg.spacing  = new Vector2(3, 3);
                EditorUtility.SetDirty(grid.gameObject);
                Debug.Log($"Grid cell size: {glg.cellSize}");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixScrollPanel] Scrollbar added, grid resized!");
    }
}
