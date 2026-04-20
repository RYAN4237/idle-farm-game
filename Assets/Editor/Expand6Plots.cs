using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;

/// Expands farm from 3 plots to 6 plots in a 2x3 grid
/// Also repositions all plots for better Rusty's Retirement feel
public class Expand6Plots
{
    public static void Execute()
    {
        var farmContainer = GameObject.Find("FarmContainer");
        if (farmContainer == null) { Debug.LogError("FarmContainer not found"); return; }

        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── Grid layout ──
        // Left 70% world x: -8.89 to +3.34, center = -2.78
        // 2 rows: back row y=-0.8, front row y=-2.8
        // 3 cols: spaced 4.0 apart, centered at -2.78
        float cx    = -2.78f;
        float colSp = 4.0f;
        float rowY0 = -0.8f;   // back row
        float rowY1 = -2.8f;   // front row

        var positions = new Vector3[]
        {
            // Row 1 (back) — plots 1,2,3
            new Vector3(cx - colSp, rowY0, 0f),
            new Vector3(cx,         rowY0, 0f),
            new Vector3(cx + colSp, rowY0, 0f),
            // Row 2 (front) — plots 4,5,6
            new Vector3(cx - colSp, rowY1, 0f),
            new Vector3(cx,         rowY1, 0f),
            new Vector3(cx + colSp, rowY1, 0f),
        };

        // Scale: back row slightly smaller for depth illusion
        var scales = new float[] { 16f, 16f, 16f, 20f, 20f, 20f };

        // ── Reposition existing 3 plots ──
        for (int i = 0; i < 3; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = positions[i];
            go.transform.localScale = new Vector3(scales[i], scales[i], 1f);
            FixLabel(go, scales[i]);
            EditorUtility.SetDirty(go);
        }

        // ── Create plots 4, 5, 6 ──
        for (int i = 3; i < 6; i++)
        {
            int plotNum = i + 1;

            // Check if already exists
            if (GameObject.Find("FarmPlot_" + plotNum) != null)
            {
                var existingGo = GameObject.Find("FarmPlot_" + plotNum);
                existingGo.transform.position   = positions[i];
                existingGo.transform.localScale = new Vector3(scales[i], scales[i], 1f);
                FixLabel(existingGo, scales[i]);
                EditorUtility.SetDirty(existingGo);
                continue;
            }

            var plotGO = new GameObject("FarmPlot_" + plotNum);
            plotGO.transform.SetParent(farmContainer.transform);
            plotGO.transform.position   = positions[i];
            plotGO.transform.localScale = new Vector3(scales[i], scales[i], 1f);

            // SpriteRenderer
            var sr     = plotGO.AddComponent<SpriteRenderer>();
            sr.sprite  = uiSprite;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.color   = new Color(0.42f, 0.30f, 0.18f, 1f);
            sr.sortingOrder = 0;

            // BoxCollider2D
            var col  = plotGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.16f, 0.16f);

            // FarmPlot script
            var plot = plotGO.AddComponent<FarmPlot>();
            plot.growthDuration = 10f;
            plot.plantCost      = 10f;
            plot.harvestReward  = 20f;
            plot.emptyColor     = new Color(0.42f, 0.30f, 0.18f, 1f);
            plot.growingColor   = new Color(0.22f, 0.62f, 0.22f, 1f);
            plot.readyColor     = new Color(0.35f, 0.95f, 0.35f, 1f);

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(plotGO.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, 0.006f, -0.1f);
            labelGO.transform.localScale    = new Vector3(0.022f, 0.022f, 1f);
            var tmp = labelGO.AddComponent<TextMeshPro>();
            tmp.text      = "Plant\n(10 FP)";
            tmp.fontSize  = 10f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(1f, 0.92f, 0.65f, 1f);
            tmp.enableWordWrapping = false;
            tmp.textWrappingMode   = TMPro.TextWrappingModes.NoWrap;
            tmp.overflowMode       = TMPro.TextOverflowModes.Overflow;
            tmp.rectTransform.sizeDelta = new Vector2(8f, 5f);
            tmp.sortingOrder = 3;

            // ProgressBarBG
            var barBgGO = new GameObject("ProgressBarBG");
            barBgGO.transform.SetParent(plotGO.transform, false);
            barBgGO.transform.localPosition = new Vector3(0f, -0.050f, -0.05f);
            barBgGO.transform.localScale    = new Vector3(0.012f, 0.006f, 1f);
            var barBgSR = barBgGO.AddComponent<SpriteRenderer>();
            barBgSR.sprite = uiSprite; barBgSR.drawMode = SpriteDrawMode.Simple;
            barBgSR.color  = new Color(0.08f, 0.08f, 0.08f, 0.7f);
            barBgSR.sortingOrder = 1;

            var barFillGO = new GameObject("ProgressBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            barFillGO.transform.localPosition = new Vector3(-0.5f, 0f, -0.05f);
            barFillGO.transform.localScale    = new Vector3(0.001f, 1f, 1f);
            var barFillSR = barFillGO.AddComponent<SpriteRenderer>();
            barFillSR.sprite = uiSprite; barFillSR.drawMode = SpriteDrawMode.Simple;
            barFillSR.color  = new Color(0.25f, 0.85f, 0.35f, 1f);
            barFillSR.sortingOrder = 2;

            // FarmPlotUI
            var plotUI  = plotGO.AddComponent<FarmPlotUI>();
            plotUI.label = tmp;

            EditorUtility.SetDirty(plotGO);
            Debug.Log($"FarmPlot_{plotNum} created at {positions[i]}");
        }

        // ── GroundStrip: taller to cover bottom row bases ──
        var canvas = GameObject.Find("UICanvas");
        var ground = canvas?.transform.Find("GroundStrip");
        if (ground != null)
        {
            var r = ground.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0.70f, 0.10f); // thin strip, front plots sit on it
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(ground.gameObject);
        }

        // ── Physics2DRaycaster: ensure it's still on camera ──
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
        {
            cam.gameObject.AddComponent<Physics2DRaycaster>();
            Debug.Log("Physics2DRaycaster added.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Expand6Plots complete + saved!");
    }

    static void FixLabel(GameObject go, float scale)
    {
        var label = go.transform.Find("Label");
        if (label == null) return;
        // Adjust label scale so text world-size stays consistent regardless of plot scale
        float ls = 20f / scale * 0.022f;
        label.localScale    = new Vector3(ls, ls, 1f);
        label.localPosition = new Vector3(0f, 0.006f, -0.1f);
        var tmp = label.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text     = "Plant\n(10 FP)";
            tmp.fontSize = 10f;
            tmp.color    = new Color(1f, 0.92f, 0.65f, 1f);
            tmp.alignment = TextAlignmentOptions.Center;
        }
        EditorUtility.SetDirty(label.gameObject);
    }
}
