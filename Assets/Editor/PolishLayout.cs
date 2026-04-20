using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class PolishLayout
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel")?.gameObject;

        // ── 1. Fix camera: remove left black strip by ensuring ortho fills screen ──
        // The black strip on left is from Scene View gizmo panel, not the game.
        // Camera background is already sky blue — OK.

        // ── 2. Right panel width: 28% ──
        if (rightPanel != null)
        {
            var r = rightPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.72f, 0f);
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(rightPanel);
        }

        // ── 3. GroundStrip: left 72% ──
        var ground = canvas?.transform.Find("GroundStrip");
        if (ground != null)
        {
            var r = ground.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0.72f, 0.22f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(ground.gameObject);
        }

        // ── 4. CenterContainer (Timer): top 55% of RightPanel ──
        var center = rightPanel?.transform.Find("CenterContainer");
        if (center != null)
        {
            var r = center.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.05f, 0.48f);
            r.anchorMax        = new Vector2(0.95f, 1.0f);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = new Vector2(0f, -15f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(center.gameObject);
        }

        // ── 5. ButtonBar: middle of RightPanel ──
        var buttonBar = rightPanel?.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.05f, 0.34f);
            r.anchorMax        = new Vector2(0.95f, 0.46f);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // Fix button sizes within ButtonBar
        var startBtn = buttonBar?.Find("StartPauseButton");
        if (startBtn != null)
        {
            var r = startBtn.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0.55f, 1f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = Vector2.zero;
        }
        var resetBtn = buttonBar?.Find("ResetButton");
        if (resetBtn != null)
        {
            var r = resetBtn.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.60f, 0f);
            r.anchorMax = new Vector2(1f, 1f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta = Vector2.zero;
        }

        // ── 6. StatsPanel (FP): bottom of RightPanel ──
        var stats = rightPanel?.transform.Find("StatsPanel");
        if (stats != null)
        {
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(1f, 0.32f);
            r.offsetMin        = new Vector2(8f, 8f);
            r.offsetMax        = new Vector2(-8f, 0f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(stats.gameObject);
        }

        // Layout stats children vertically
        var fpLabel    = stats?.Find("FPLabel");
        var fpText     = stats?.Find("FocusPointsText");
        var incomeText = stats?.Find("IncomeRateText");
        var sessText   = stats?.Find("SessionCountText");

        // Simple manual anchors for each stat row
        SetAnchorText(fpLabel,    0f, 0.75f, 1f, 1.0f,  "FOCUS POINTS", 10f, new Color(0.6f,0.6f,0.6f,1f));
        SetAnchorText(fpText,     0f, 0.48f, 1f, 0.78f, null, 28f, new Color(1f,1f,1f,1f));
        SetAnchorText(incomeText, 0f, 0.24f, 1f, 0.50f, null, 13f, new Color(0.2f,0.85f,0.7f,1f));
        SetAnchorText(sessText,   0f, 0.02f, 1f, 0.26f, null, 12f, new Color(0.5f,0.5f,0.5f,1f));

        // ── 7. FarmPlots: left 72%, 3 plots in bottom row ──
        // Main area world: camera ortho=5, 16:9 → total width=17.8
        // Left 72% of screen → world x from -8.9 to +3.9
        // Center of left area: x = (-8.9 + 3.9) / 2 = -2.5
        float cx = -2.5f;
        float spacing = 3.5f;
        Vector3[] pos = {
            new Vector3(cx - spacing, -3.6f, 0f),
            new Vector3(cx,           -3.6f, 0f),
            new Vector3(cx + spacing, -3.6f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(16f, 16f, 1f);

            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, 0.01f, -0.1f);
                label.localScale    = new Vector3(0.028f, 0.028f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize  = 11f;
                    tmp.rectTransform.sizeDelta = new Vector2(5f, 3f);
                }
                EditorUtility.SetDirty(label.gameObject);
            }

            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.055f, -0.05f);
                barBg.localScale    = new Vector3(0.011f, 0.005f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            EditorUtility.SetDirty(go);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("PolishLayout complete!");
    }

    static void SetAnchorText(Transform t, float ax, float ay, float bx, float by,
                               string overrideText, float fontSize, Color color)
    {
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        if (r == null) return;
        r.anchorMin = new Vector2(ax, ay);
        r.anchorMax = new Vector2(bx, by);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = Vector2.zero;

        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            if (overrideText != null) tmp.text = overrideText;
            tmp.fontSize             = fontSize;
            tmp.color                = color;
            tmp.alignment            = TMPro.TextAlignmentOptions.Center;
        }
        EditorUtility.SetDirty(t.gameObject);
    }
}
