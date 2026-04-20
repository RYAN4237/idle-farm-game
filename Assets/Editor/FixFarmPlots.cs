using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;

public class FixFarmPlots
{
    public static void Execute()
    {
        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        for (int i = 1; i <= 3; i++)
        {
            var plotGO = GameObject.Find("FarmPlot_" + i);
            if (plotGO == null) { Debug.LogWarning("FarmPlot_" + i + " not found"); continue; }

            // ── Fix scale: use world-unit scale, not sprite-pixel scale ──
            plotGO.transform.localScale = Vector3.one;

            // ── Fix SpriteRenderer: set explicit world size ──
            var sr = plotGO.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size     = new Vector2(1.6f, 1.6f); // 1.6 world units square
                sr.color    = new Color(0.35f, 0.28f, 0.20f, 1f);
                sr.sortingOrder = 0;
            }

            // ── Fix BoxCollider2D size to match sprite ──
            var col = plotGO.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(1.6f, 1.6f);

            // ── Rebuild children cleanly ──
            for (int c = plotGO.transform.childCount - 1; c >= 0; c--)
                Object.DestroyImmediate(plotGO.transform.GetChild(c).gameObject);

            // Label above plot
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(plotGO.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, 1.05f, -0.1f);
            labelGO.transform.localScale    = new Vector3(0.5f, 0.5f, 1f);
            var tmp = labelGO.AddComponent<TextMeshPro>();
            tmp.text               = "Plant\n(10 FP)";
            tmp.fontSize           = 8f;
            tmp.alignment          = TextAlignmentOptions.Center;
            tmp.color              = new Color(1f, 1f, 0.8f, 1f);
            tmp.enableWordWrapping = true;
            tmp.sortingOrder       = 3;
            tmp.rectTransform.sizeDelta = new Vector2(3f, 1.5f);

            // Progress bar BG (dark strip at bottom of plot)
            var barBgGO = new GameObject("ProgressBarBG");
            barBgGO.transform.SetParent(plotGO.transform, false);
            barBgGO.transform.localPosition = new Vector3(0f, -0.65f, -0.05f);
            barBgGO.transform.localScale    = Vector3.one;
            var barBgSR = barBgGO.AddComponent<SpriteRenderer>();
            barBgSR.sprite       = uiSprite;
            barBgSR.drawMode     = SpriteDrawMode.Sliced;
            barBgSR.size         = new Vector2(1.4f, 0.18f);
            barBgSR.color        = new Color(0.08f, 0.08f, 0.08f, 0.75f);
            barBgSR.sortingOrder = 1;

            // Progress bar Fill
            var barFillGO = new GameObject("ProgressBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            barFillGO.transform.localPosition = new Vector3(-0.7f, 0f, -0.05f);
            barFillGO.transform.localScale    = new Vector3(0.001f, 1f, 1f);
            var barFillSR = barFillGO.AddComponent<SpriteRenderer>();
            barFillSR.sprite       = uiSprite;
            barFillSR.drawMode     = SpriteDrawMode.Sliced;
            barFillSR.size         = new Vector2(1.4f, 0.18f);
            barFillSR.color        = new Color(0.25f, 0.85f, 0.35f, 1f);
            barFillSR.sortingOrder = 2;

            // Re-wire FarmPlotUI
            var plotUI = plotGO.GetComponent<FarmPlotUI>() ?? plotGO.AddComponent<FarmPlotUI>();
            plotUI.label = tmp;

            EditorUtility.SetDirty(plotGO);
            Debug.Log($"FarmPlot_{i} fixed. Collider=1.6x1.6, Scale=1");
        }

        // ── Fix Physics2DRaycaster ──
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
        {
            cam.gameObject.AddComponent<Physics2DRaycaster>();
            Debug.Log("Physics2DRaycaster added to camera.");
        }

        // ── FarmingSystem ──
        var gm = GameObject.Find("GameManager");
        if (gm != null && gm.GetComponent<FarmingSystem>() == null)
        {
            gm.AddComponent<FarmingSystem>();
            Debug.Log("FarmingSystem added.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixFarmPlots complete!");
    }
}
