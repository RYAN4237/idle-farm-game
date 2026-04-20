using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Expands farm to 12 plots (2 rows x 6 cols) and sets up scrolling.
/// Also adds scroll indicator arrows on the farm area.
public class BuildScrollableFarm
{
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        var farmContainer = GameObject.Find("FarmContainer");
        if (farmContainer == null) { Debug.LogError("FarmContainer not found"); return; }

        var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // ── 1. Add FarmMapScroller to GameManager ──
        var gm = GameObject.Find("GameManager");
        if (gm != null && gm.GetComponent<FarmMapScroller>() == null)
        {
            var scroller = gm.AddComponent<FarmMapScroller>();
            scroller.mapMinX = -10f;
            scroller.mapMaxX =  4f;
            scroller.scrollSpeed   = 8f;
            scroller.snapSmoothing = 6f;
            EditorUtility.SetDirty(gm);
            Debug.Log("FarmMapScroller added to GameManager.");
        }

        // ── 2. Layout: 2 rows x 6 cols ──
        // Each plot: scale=8, world size=1.28 units, gap=0.4 → step=1.68
        float scale  = 8f;
        float step   = 1.72f;
        float ry0    =  0.85f;   // back row y
        float ry1    = -0.85f;   // front row y
        int   cols   = 6;

        // Center the 6 cols so col 0,1,2 are in view at start
        // cam starts at x=0, visible range ≈ -4.98 to +4.98 (ortho=2.8, 16:9)
        // 6 cols from x=-4.5 step 1.72: -4.5, -2.78, -1.06, +0.66, +2.38, +4.10
        float startX = -4.3f;

        var plotPositions = new Vector3[12];
        for (int c = 0; c < cols; c++)
        {
            float x = startX + c * step;
            plotPositions[c]       = new Vector3(x, ry0, 0f); // back row
            plotPositions[c + cols]= new Vector3(x, ry1, 0f); // front row
        }

        // ── 3. Move / create plots 1-12 ──
        for (int i = 0; i < 12; i++)
        {
            int plotNum = i + 1;
            var go      = GameObject.Find("FarmPlot_" + plotNum);

            if (go == null)
            {
                // Create new plot
                go = CreatePlot(plotNum, farmContainer, uiSprite);
            }

            go.transform.position   = plotPositions[i];
            go.transform.localScale = new Vector3(scale, scale, 1f);

            // Fix collider
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null) col.size = new Vector2(0.16f, 0.16f);

            // Fix label
            FixLabel(go, scale);

            // Lock plots 7-12 by default
            var plot = go.GetComponent<FarmPlot>();
            if (plot != null && plotNum > 6)
            {
                plot.isLocked = true;
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(0.18f, 0.18f, 0.18f, 1f);
            }

            EditorUtility.SetDirty(go);
        }

        // ── 4. Update CropShop unlock costs for 7-12 ──
        var cropShop = gm?.GetComponent<CropShop>();
        if (cropShop != null)
        {
            cropShop.plotUnlockCosts = new float[]
            {
                0f, 0f, 0f,         // plots 1-3: free
                80f, 150f, 300f,    // plots 4-6
                500f, 800f, 1200f,  // plots 7-9
                1800f, 2500f, 3500f // plots 10-12
            };
            EditorUtility.SetDirty(gm);
        }

        // ── 5. Add scroll hint arrows in UI ──
        AddScrollArrows(canvas);

        // ── 6. Extend FarmBG transparency to cover wider area ──
        // (already transparent, nothing to do)

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildScrollableFarm complete! 12 plots, scrollable map.");
    }

    static GameObject CreatePlot(int plotNum, GameObject farmContainer, Sprite uiSprite)
    {
        var go = new GameObject("FarmPlot_" + plotNum);
        go.transform.SetParent(farmContainer.transform);

        var sr      = go.AddComponent<SpriteRenderer>();
        sr.sprite   = uiSprite;
        sr.drawMode = SpriteDrawMode.Simple;
        sr.color    = new Color(0.42f, 0.30f, 0.16f, 1f);
        sr.sortingOrder = 0;

        var col = go.AddComponent<BoxCollider2D>();
        col.size= new Vector2(0.16f, 0.16f);

        var plot = go.AddComponent<FarmPlot>();
        plot.growthDuration = 10f;
        plot.plantCost      = 10f;
        plot.harvestReward  = 25f;
        plot.emptyColor     = new Color(0.42f, 0.30f, 0.16f, 1f);
        plot.growingColor   = new Color(0.20f, 0.58f, 0.20f, 1f);
        plot.readyColor     = new Color(0.30f, 1.00f, 0.30f, 1f);

        // Shadow
        var shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(go.transform, false);
        shadowGO.transform.localPosition = new Vector3(0.005f, -0.008f, 0.05f);
        shadowGO.transform.localScale    = new Vector3(1.08f, 1.08f, 1f);
        var shadowSR = shadowGO.AddComponent<SpriteRenderer>();
        shadowSR.sprite = uiSprite; shadowSR.drawMode = SpriteDrawMode.Simple;
        shadowSR.color  = new Color(0.18f, 0.12f, 0.06f, 0.7f);
        shadowSR.sortingOrder = -1;

        // Label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var tmp = labelGO.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "Plant\n(10 FP)"; tmp.fontSize = 10f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.92f, 0.65f, 1f);
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
        tmp.sortingOrder = 3;
        tmp.rectTransform.sizeDelta = new Vector2(6f, 4f);

        // ProgressBarBG
        var barBG = new GameObject("ProgressBarBG");
        barBG.transform.SetParent(go.transform, false);
        barBG.transform.localPosition = new Vector3(0f, -0.050f, -0.05f);
        barBG.transform.localScale    = new Vector3(0.012f, 0.006f, 1f);
        var barBGSR = barBG.AddComponent<SpriteRenderer>();
        barBGSR.sprite = uiSprite; barBGSR.drawMode = SpriteDrawMode.Simple;
        barBGSR.color  = new Color(0.08f, 0.08f, 0.08f, 0.7f); barBGSR.sortingOrder = 1;

        var barFill = new GameObject("ProgressBarFill");
        barFill.transform.SetParent(barBG.transform, false);
        barFill.transform.localPosition = new Vector3(-0.5f, 0f, -0.05f);
        barFill.transform.localScale    = new Vector3(0.001f, 1f, 1f);
        var barFillSR = barFill.AddComponent<SpriteRenderer>();
        barFillSR.sprite = uiSprite; barFillSR.drawMode = SpriteDrawMode.Simple;
        barFillSR.color  = new Color(0.25f, 0.85f, 0.35f, 1f); barFillSR.sortingOrder = 2;

        // CropIcon
        var iconGO = new GameObject("CropIcon");
        iconGO.transform.SetParent(go.transform, false);
        iconGO.transform.localPosition = new Vector3(0f, 0.035f, -0.15f);
        iconGO.transform.localScale    = new Vector3(0.25f, 0.25f, 1f);
        var iconSR = iconGO.AddComponent<SpriteRenderer>();
        iconSR.sprite = uiSprite; iconSR.drawMode = SpriteDrawMode.Simple;
        iconSR.color  = Color.clear; iconSR.sortingOrder = 4;

        // FarmPlotUI
        var plotUI = go.AddComponent<FarmPlotUI>();
        plotUI.label = tmp;

        Debug.Log($"FarmPlot_{plotNum} created.");
        return go;
    }

    static void FixLabel(GameObject go, float scale)
    {
        var label = go.transform.Find("Label");
        if (label == null) return;
        label.localScale    = new Vector3(0.034f, 0.034f, 1f);
        label.localPosition = new Vector3(0f, 0.004f, -0.1f);
        var tmp = label.GetComponent<TMPro.TextMeshPro>();
        if (tmp != null) tmp.fontSize = 10f;
        EditorUtility.SetDirty(label.gameObject);
    }

    static void AddScrollArrows(GameObject canvas)
    {
        if (canvas == null) return;

        // Remove old arrows
        var old = canvas.transform.Find("ScrollArrows");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var arrowsGO = new GameObject("ScrollArrows");
        arrowsGO.transform.SetParent(canvas.transform, false);
        var r = arrowsGO.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0.085f, 0f); r.anchorMax = new Vector2(0.46f, 1f);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;

        // Left arrow button
        var leftGO = MakeArrowBtn(arrowsGO.transform, "LeftArrow", "◀", 0f, 0.3f, 0.08f, 0.7f);
        var leftBtn = leftGO.GetComponent<Button>();
        leftBtn.onClick.AddListener(() => {
            if (FarmMapScroller.Instance != null)
                FarmMapScroller.Instance.ScrollTo(
                    Camera.main.transform.position.x - 3.5f);
        });

        // Right arrow button
        var rightGO = MakeArrowBtn(arrowsGO.transform, "RightArrow", "▶", 0.92f, 0.3f, 1f, 0.7f);
        var rightBtn = rightGO.GetComponent<Button>();
        rightBtn.onClick.AddListener(() => {
            if (FarmMapScroller.Instance != null)
                FarmMapScroller.Instance.ScrollTo(
                    Camera.main.transform.position.x + 3.5f);
        });

        EditorUtility.SetDirty(arrowsGO);
    }

    static GameObject MakeArrowBtn(Transform parent, string name, string label,
        float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var r  = go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.10f, 0.12f, 0.16f, 0.75f);
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var cs  = btn.colors;
        cs.highlightedColor = new Color(0.18f, 0.22f, 0.30f, 0.9f);
        cs.pressedColor     = new Color(0.06f, 0.08f, 0.10f, 0.9f);
        btn.colors = cs;

        var txtGO = new GameObject("Text"); txtGO.transform.SetParent(go.transform, false);
        var tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 14f; tmp.color = new Color(0.7f, 0.75f, 0.8f, 1f);
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;

        return go;
    }
}
