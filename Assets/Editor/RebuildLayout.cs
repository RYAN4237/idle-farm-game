using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// Rebuilds the entire UI layout:
/// - Right panel (25% width): Pomodoro timer + FP display
/// - Main area (75% width): Farm plots at bottom, open space above
public class RebuildLayout
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. Camera: sky-blue background like Rusty's Retirement ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags    = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.53f, 0.81f, 0.92f, 1f); // sky blue
            EditorUtility.SetDirty(cam.gameObject);
        }

        // ── 2. Remove old layout elements ──
        DestroyChild(canvas, "TopBackground");
        DestroyChild(canvas, "BackgroundPanel");
        DestroyChild(canvas, "FarmLabel");
        DestroyChild(canvas, "CycleDots");

        // ── 3. Main background - grass/farm area (left 75%) ──
        // We'll use camera bg color for the sky, so canvas bg can be transparent
        // Add a ground strip at the bottom
        CreateGroundStrip(canvas);

        // ── 4. Right panel background ──
        var rightPanel = GetOrCreate(canvas, "RightPanel");
        {
            var img  = rightPanel.GetComponent<Image>() ?? rightPanel.AddComponent<Image>();
            img.color = new Color(0.12f, 0.14f, 0.17f, 0.92f);
            img.raycastTarget = false;

            var r = rightPanel.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.75f, 0f);
            r.anchorMax        = Vector2.one;
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            rightPanel.transform.SetSiblingIndex(0);
            EditorUtility.SetDirty(rightPanel);
        }

        // ── 5. Move Timer elements into RightPanel ──
        // CenterContainer → inside RightPanel, centered
        var center = canvas.transform.Find("CenterContainer");
        if (center != null)
        {
            center.SetParent(rightPanel.transform, false);
            var r = center.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0.5f);
            r.anchorMax        = new Vector2(1f, 1f);
            r.offsetMin        = new Vector2(10f, 0f);
            r.offsetMax        = new Vector2(-10f, -20f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = new Vector2(-20f, 0f);
            EditorUtility.SetDirty(center.gameObject);
        }

        // ButtonBar → inside RightPanel
        var buttonBar = canvas.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            buttonBar.SetParent(rightPanel.transform, false);
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.1f, 0.35f);
            r.anchorMax        = new Vector2(0.9f, 0.50f);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // TopRightPanel → bottom of RightPanel (FP display)
        var topRight = canvas.transform.Find("TopRightPanel");
        if (topRight != null)
        {
            topRight.SetParent(rightPanel.transform, false);
            topRight.name = "StatsPanel";
            var r = topRight.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(1f, 0.33f);
            r.offsetMin        = new Vector2(10f, 10f);
            r.offsetMax        = new Vector2(-10f, 0f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(topRight.gameObject);
        }

        // PopupAnchor → center of main area
        var popup = canvas.transform.Find("PopupAnchor");
        if (popup != null)
        {
            var r = popup.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.375f, 0.5f);
            r.anchorMax        = new Vector2(0.375f, 0.5f);
            r.anchoredPosition = Vector2.zero;
            EditorUtility.SetDirty(popup.gameObject);
        }

        // DurationLabel stays in CenterContainer — no change needed

        // ── 6. FP Label at top of stats ──
        var fpLabel = rightPanel.transform.Find("StatsPanel/FPLabel");
        if (fpLabel == null) fpLabel = rightPanel.transform.Find("StatsPanel")
            ?.GetComponentInChildren<TextMeshProUGUI>()?.transform;

        // ── 7. Reposition FarmPlots ──
        // Main area = left 75% of screen. Camera ortho size=5, 16:9 ratio
        // World width ≈ ±8.9, left 75% = world x from -8.9 to +4.45(-ish)
        // Place 6 plots in a 2-row × 3-col grid at bottom of main area
        // Row 1 (front): y = -3.5, Row 2 (back): y = -1.8
        // Actually we have 3 plots — spread in one row at bottom

        // 3 plots in bottom row, centered in main area (left 75%)
        // Main area center x = (−8.9 + 4.45)/2 = −2.2
        float mainCenterX = -2.2f;
        Vector3[] plotPos = new Vector3[]
        {
            new Vector3(mainCenterX - 3.2f, -3.5f, 0f),
            new Vector3(mainCenterX,        -3.5f, 0f),
            new Vector3(mainCenterX + 3.2f, -3.5f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = plotPos[i];
            go.transform.localScale = new Vector3(14f, 14f, 1f);

            // Fix collider
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            // Fix label to be inside plot, small font
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, 0f, -0.1f);
                label.localScale    = new Vector3(0.032f, 0.032f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize  = 12f;
                    tmp.rectTransform.sizeDelta = new Vector2(4f, 3f);
                }
            }

            EditorUtility.SetDirty(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("RebuildLayout complete! Timer on right, Farm on left.");
    }

    static void CreateGroundStrip(GameObject canvas)
    {
        var go  = GetOrCreate(canvas, "GroundStrip");
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color         = new Color(0.35f, 0.55f, 0.20f, 1f); // grass green
        img.raycastTarget = false;

        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0f);
        r.anchorMax = new Vector2(0.75f, 0.18f); // bottom 18% of main area
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        go.transform.SetSiblingIndex(1);
        EditorUtility.SetDirty(go);
    }

    static GameObject GetOrCreate(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void DestroyChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }
}
