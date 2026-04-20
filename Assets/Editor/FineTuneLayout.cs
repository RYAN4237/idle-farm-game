using UnityEngine;
using UnityEditor;
using TMPro;

public class FineTuneLayout
{
    public static void Execute()
    {
        var canvas     = GameObject.Find("UICanvas");
        var rightPanel = canvas?.transform.Find("RightPanel")?.gameObject;

        // ── 1. Farm plots: larger + sit ON the grass (y = -3.1) ──
        // Camera ortho=5, screen bottom = world y=-5, grass top ≈ y=-2.8
        // Plot world height = 16×0.16 = 2.56 units → center at y = -3.0 puts top at -1.72
        float cx = -2.5f;
        float sp = 3.8f;
        Vector3[] pos = {
            new Vector3(cx - sp, -3.0f, 0f),
            new Vector3(cx,      -3.0f, 0f),
            new Vector3(cx + sp, -3.0f, 0f),
        };

        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(18f, 18f, 1f);  // 18×0.16=2.88 units

            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            // Label: centered, tiny
            var label = go.transform.Find("Label");
            if (label != null)
            {
                label.localPosition = new Vector3(0f, 0.01f, -0.1f);
                label.localScale    = new Vector3(0.025f, 0.025f, 1f);
                var tmp = label.GetComponent<TextMeshPro>();
                if (tmp != null)
                {
                    tmp.fontSize            = 11f;
                    tmp.rectTransform.sizeDelta = new Vector2(5f, 3.5f);
                }
                EditorUtility.SetDirty(label.gameObject);
            }

            var barBg = go.transform.Find("ProgressBarBG");
            if (barBg != null)
            {
                barBg.localPosition = new Vector3(0f, -0.05f, -0.05f);
                barBg.localScale    = new Vector3(0.01f, 0.005f, 1f);
                EditorUtility.SetDirty(barBg.gameObject);
            }
            EditorUtility.SetDirty(go);
        }

        // ── 2. Grass strip: taller, cover bottom 28% ──
        var ground = canvas?.transform.Find("GroundStrip");
        if (ground != null)
        {
            var r = ground.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0.72f, 0.28f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var img = ground.GetComponent<UnityEngine.UI.Image>();
            if (img != null) img.color = new Color(0.30f, 0.52f, 0.18f, 1f);
            EditorUtility.SetDirty(ground.gameObject);
        }

        // ── 3. Timer ring: top 58% of right panel ──
        var center = rightPanel?.transform.Find("CenterContainer");
        if (center != null)
        {
            var r = center.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.02f, 0.50f);
            r.anchorMax        = new Vector2(0.98f, 1.00f);
            r.offsetMin        = new Vector2(0f, -10f);
            r.offsetMax        = new Vector2(0f, -10f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(center.gameObject);
        }

        // ── 4. Button bar: 34-48% of right panel ──
        var buttonBar = rightPanel?.transform.Find("ButtonBar");
        if (buttonBar != null)
        {
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0.04f, 0.35f);
            r.anchorMax = new Vector2(0.96f, 0.48f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(buttonBar.gameObject);
        }

        // ── 5. Stats: bottom 33% ──
        var stats = rightPanel?.transform.Find("StatsPanel");
        if (stats != null)
        {
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0.00f);
            r.anchorMax = new Vector2(1f, 0.33f);
            r.offsetMin = new Vector2(6f, 6f);
            r.offsetMax = new Vector2(-6f, 0f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(stats.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FineTuneLayout complete!");
    }
}
