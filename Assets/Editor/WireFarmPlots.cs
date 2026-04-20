using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;

public class WireFarmPlots
{
    public static void Execute()
    {
        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── FarmingSystem on GameManager ──
        var gm = GameObject.Find("GameManager");
        if (gm != null && gm.GetComponent<FarmingSystem>() == null)
            gm.AddComponent<FarmingSystem>();

        // ── Wire each plot ──
        for (int i = 1; i <= 3; i++)
        {
            var plotGO = GameObject.Find("FarmPlot_" + i);
            if (plotGO == null) { Debug.LogWarning("FarmPlot_" + i + " not found"); continue; }

            if (plotGO.GetComponent<FarmPlot>() == null)
                plotGO.AddComponent<FarmPlot>();

            // Clear old children
            for (int c = plotGO.transform.childCount - 1; c >= 0; c--)
                Object.DestroyImmediate(plotGO.transform.GetChild(c).gameObject);

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(plotGO.transform, false);
            labelGO.transform.localPosition = new Vector3(0f, 0.72f, -0.1f);
            labelGO.transform.localScale    = new Vector3(0.22f, 0.22f, 1f);
            var tmp = labelGO.AddComponent<TextMeshPro>();
            tmp.text = "Plant\n(10 FP)";
            tmp.fontSize = 12f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            tmp.sortingOrder = 3;

            // Progress bar BG
            var barBgGO = new GameObject("ProgressBarBG");
            barBgGO.transform.SetParent(plotGO.transform, false);
            barBgGO.transform.localPosition = new Vector3(0f, -0.55f, -0.1f);
            barBgGO.transform.localScale    = new Vector3(0.85f, 0.10f, 1f);
            var barBgSR = barBgGO.AddComponent<SpriteRenderer>();
            barBgSR.sprite = uiSprite;
            barBgSR.color  = new Color(0.1f, 0.1f, 0.1f, 0.6f);
            barBgSR.sortingOrder = 1;

            // Progress bar Fill
            var barFillGO = new GameObject("ProgressBarFill");
            barFillGO.transform.SetParent(barBgGO.transform, false);
            barFillGO.transform.localPosition = new Vector3(-0.5f, 0f, -0.05f);
            barFillGO.transform.localScale    = new Vector3(0.001f, 1f, 1f);
            var barFillSR = barFillGO.AddComponent<SpriteRenderer>();
            barFillSR.sprite = uiSprite;
            barFillSR.color  = new Color(0.25f, 0.85f, 0.35f, 1f);
            barFillSR.sortingOrder = 2;

            // FarmPlotUI
            var plotUI = plotGO.GetComponent<FarmPlotUI>() ?? plotGO.AddComponent<FarmPlotUI>();
            plotUI.label = tmp;

            EditorUtility.SetDirty(plotGO);
            Debug.Log("FarmPlot_" + i + " wired OK.");
        }

        // Physics2DRaycaster
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
        {
            cam.gameObject.AddComponent<Physics2DRaycaster>();
            Debug.Log("Physics2DRaycaster added.");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("WireFarmPlots complete!");
    }
}
