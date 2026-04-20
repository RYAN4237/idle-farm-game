using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class FinalLayoutFix
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel")?.gameObject;
        if (rightPanel == null) { Debug.LogError("RightPanel not found"); return; }

        // ════════════════════════════════
        // RIGHT PANEL — Timer side
        // ════════════════════════════════

        // Right panel: rightmost 27% of screen
        SetRect(rightPanel, 0.73f, 0f, 1f, 1f);

        // ── Timer circle: fixed square size centered in top half ──
        var center = rightPanel.transform.Find("CenterContainer");
        if (center != null)
        {
            // Anchor to top-center, fixed size 240×240
            var r = center.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(240f, 260f);
            r.anchoredPosition = new Vector2(0f, -10f);
            EditorUtility.SetDirty(center.gameObject);
        }

        // ── Buttons: below timer ──
        var buttonBar = rightPanel.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f, 1f);
            r.anchorMax        = new Vector2(0.5f, 1f);
            r.pivot            = new Vector2(0.5f, 1f);
            r.sizeDelta        = new Vector2(230f, 55f);
            r.anchoredPosition = new Vector2(0f, -278f);
            EditorUtility.SetDirty(buttonBar.gameObject);

            // Fix individual buttons
            FixButton(buttonBar.Find("StartPauseButton"), 0f, 0f, 0.55f, 1f);
            FixButton(buttonBar.Find("ResetButton"),      0.60f, 0f, 1f, 1f);
        }

        // ── Stats panel: below buttons ──
        var stats = rightPanel.transform.Find("StatsPanel");
        if (stats != null)
        {
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(1f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(-16f, 160f);
            r.anchoredPosition = new Vector2(0f, 8f);
            EditorUtility.SetDirty(stats.gameObject);

            // Rearrange stat text elements
            PositionStat(stats, "FPLabel",         0f, 0.80f, 1f, 1.00f, 10f, new Color(0.55f,0.55f,0.55f,1f));
            PositionStat(stats, "FocusPointsText",  0f, 0.50f, 1f, 0.82f, 30f, Color.white);
            PositionStat(stats, "IncomeRateText",   0f, 0.28f, 1f, 0.52f, 13f, new Color(0.20f,0.85f,0.70f,1f));
            PositionStat(stats, "SessionCountText", 0f, 0.04f, 1f, 0.30f, 11f, new Color(0.45f,0.45f,0.45f,1f));
        }

        // ════════════════════════════════
        // LEFT / FARM AREA
        // ════════════════════════════════

        // Grass strip: left 73%, bottom 30%
        var ground = canvas.transform.Find("GroundStrip");
        if (ground != null)
        {
            SetRect(ground.gameObject, 0f, 0f, 0.73f, 0.30f);
            var img = ground.GetComponent<Image>();
            if (img != null) img.color = new Color(0.28f, 0.50f, 0.16f, 1f);
            EditorUtility.SetDirty(ground.gameObject);
        }

        // ── Farm plots: sit ON the grass, evenly spaced ──
        // Camera ortho=5 → world height ±5. Screen bottom = y=-5.
        // Grass top = bottom 30% of screen = world y = -5 + 10*0.30 = -2.0
        // Plot scale=20 → world size = 20×0.16 = 3.2 units. Center at y=-2.4 → top at -0.8 ✓
        // Left 73% of screen → world x from -8.9 to +3.6. Center = -2.65

        float cx = -2.65f;
        float sp = 4.2f;
        Vector3[] plotPos = {
            new Vector3(cx - sp, -2.4f, 0f),
            new Vector3(cx,      -2.4f, 0f),
            new Vector3(cx + sp, -2.4f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;

            go.transform.position   = plotPos[i];
            go.transform.localScale = new Vector3(20f, 20f, 1f);

            // Collider matches native sprite size
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            // Label: centered inside plot
            var label = go.transform.Find("Label");
            if (label != null)
            {
                // At scale 20, localScale 0.022 → world text height ≈ 0.022×20=0.44 units
                label.localPosition = new Vector3(0f, 0.008f, -0.1f);
                label.localScale    = new Vector3(0.022f, 0.022f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize            = 11f;
                    tmp.rectTransform.sizeDelta = new Vector2(6f, 4f);
                    tmp.color = new Color(1f, 0.95f, 0.75f, 1f);
                }
                EditorUtility.SetDirty(label.gameObject);
            }

            // Progress bar: thin strip at bottom
            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.048f, -0.05f);
                barBg.localScale    = new Vector3(0.012f, 0.006f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            EditorUtility.SetDirty(go);
        }

        // PopupAnchor: center of farm area
        var popup = canvas.transform.Find("PopupAnchor");
        if (popup != null)
        {
            var r = popup.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.365f, 0.5f);
            r.anchorMax = new Vector2(0.365f, 0.5f);
            r.anchoredPosition = Vector2.zero;
            EditorUtility.SetDirty(popup.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FinalLayoutFix complete!");
    }

    static void SetRect(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>();
        if (r == null) return;
        r.anchorMin = new Vector2(ax, ay);
        r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
    }

    static void FixButton(Transform t, float ax, float ay, float bx, float by)
    {
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax, ay);
        r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        EditorUtility.SetDirty(t.gameObject);
    }

    static void PositionStat(Transform parent, string name,
        float ax, float ay, float bx, float by, float fontSize, Color color)
    {
        var t = parent?.Find(name);
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax, ay);
        r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.fontSize  = fontSize;
            tmp.color     = color;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
        }
        EditorUtility.SetDirty(t.gameObject);
    }
}
