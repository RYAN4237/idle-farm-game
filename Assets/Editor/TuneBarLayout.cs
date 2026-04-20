using UnityEngine;
using UnityEditor;
using TMPro;

public class TuneBarLayout
{
    public static void Execute()
    {
        // ── 1. Camera orthographic size ──
        // Bar = 220px tall. We want 2 rows of plots (scale=10, 1.6 world units each)
        // + spacing between rows. Total height needed ~4 world units.
        // ortho size = half height → set to 2.8 to give some padding
        var cam = Camera.main;
        if (cam != null)
        {
            cam.orthographicSize = 2.8f;
            EditorUtility.SetDirty(cam.gameObject);
        }

        // ── 2. Resize farm plots to fit in 220px bar ──
        // scale=10 → 1.6 world units. 2 rows = 3.2 + gap. ortho=2.8 → height=5.6
        // That fits! Reposition to center:
        float cx = -3.0f, sp = 2.5f;
        float ry0 =  1.0f;   // back row  y
        float ry1 = -1.0f;   // front row y

        var pos = new Vector3[]
        {
            new Vector3(cx-sp, ry0, 0f), new Vector3(cx, ry0, 0f), new Vector3(cx+sp, ry0, 0f),
            new Vector3(cx-sp, ry1, 0f), new Vector3(cx, ry1, 0f), new Vector3(cx+sp, ry1, 0f),
        };

        for (int i = 0; i < 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(10f, 10f, 1f);

            // Fix label scale for new plot scale
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localScale    = new Vector3(0.028f, 0.028f, 1f);
                label.localPosition = new Vector3(0f, 0.005f, -0.1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null) tmp.fontSize = 10f;
                EditorUtility.SetDirty(label.gameObject);
            }

            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.055f, -0.05f);
                barBg.localScale    = new Vector3(0.012f, 0.006f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }

            EditorUtility.SetDirty(go);
        }

        // ── 3. Fix FP display positioning in stats panel ──
        var canvas = GameObject.Find("UICanvas");
        var statsPanel = canvas?.transform.Find("TopRightPanel");
        if (statsPanel != null)
        {
            // Reanchor stats to fill left 10% better
            var r = statsPanel.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.002f, 0.05f);
            r.anchorMax = new Vector2(0.095f, 0.95f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(statsPanel.gameObject);

            // Fix individual stat positions
            FixStat(statsPanel, "FPLabel",         0f,0.82f,1f,1.00f, 9f,  new Color(0.5f,0.5f,0.5f,1f));
            FixStat(statsPanel, "FocusPointsText", 0f,0.45f,1f,0.84f, 26f, Color.white);
            FixStat(statsPanel, "IncomeRateText",  0f,0.22f,1f,0.46f, 11f, new Color(0.2f,0.85f,0.7f,1f));
            FixStat(statsPanel, "SessionCountText",0f,0.02f,1f,0.24f,  9f, new Color(0.4f,0.4f,0.4f,1f));
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("TuneBarLayout complete + saved!");
    }

    static void FixStat(Transform parent, string name,
        float ax, float ay, float bx, float by, float fontSize, Color color)
    {
        var t = parent.Find(name);
        if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) { tmp.fontSize = fontSize; tmp.color = color; tmp.alignment = TMPro.TextAlignmentOptions.Center; }
        EditorUtility.SetDirty(t.gameObject);
    }
}
