using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Rebuilds the entire UI as a single bottom bar.
/// The Unity window IS the bar — no transparent desktop area needed.
/// 
/// Layout (full window width × 200px height):
///  [FP | Farm Plots ... | CropShop | AutoFarmer | Pomo Timer]
///  Left→                                               ←Right
public class BuildBottomBar
{
    // Proportions of the bar
    const float POMO_W   = 0.18f;  // pomodoro section: right 18%
    const float SHOP_W   = 0.20f;  // crop shop: 20% left of pomo
    const float FP_W     = 0.10f;  // FP display: left 10%

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Camera: solid dark background (the bar background)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Clean everything
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);

        // ══════════════════════════════════════════
        // BACKGROUND & DIVIDERS
        // ══════════════════════════════════════════

        // Top edge grass accent
        MakeImg(canvas, "GrassAccent",
            new Color(0.24f, 0.46f, 0.13f, 1f), 0,
            0f, 0.88f, 1f-POMO_W, 1f);

        // Pomo panel background (right section)
        MakeImg(canvas, "PomoBG",
            new Color(0.10f, 0.12f, 0.15f, 1f), 0,
            1f-POMO_W, 0f, 1f, 1f);

        // Divider between farm and pomo
        MakeImg(canvas, "DivPomo",
            new Color(0.22f, 0.27f, 0.34f, 1f), 1,
            1f-POMO_W-0.002f, 0f, 1f-POMO_W, 1f);

        // Divider between FP and farm
        MakeImg(canvas, "DivFP",
            new Color(0.15f, 0.18f, 0.22f, 1f), 1,
            FP_W-0.001f, 0f, FP_W, 1f);

        // Shop background
        MakeImg(canvas, "ShopBG",
            new Color(0.07f, 0.08f, 0.10f, 1f), 0,
            1f-POMO_W-SHOP_W, 0.02f, 1f-POMO_W-0.002f, 0.98f);

        // ══════════════════════════════════════════
        // LEFT: FOCUS POINTS DISPLAY
        // ══════════════════════════════════════════
        var statsGO = new GameObject("TopRightPanel");
        statsGO.transform.SetParent(canvas.transform, false);
        SetRect(statsGO, 0.005f, 0.02f, FP_W-0.01f, 0.98f);
        statsGO.AddComponent<Image>().color = Color.clear;
        MakeStat(statsGO.transform, "FPLabel",         0f,0.78f,1f,1.00f, "FP",      9f,  new Color(0.5f,0.5f,0.5f,1f));
        MakeStat(statsGO.transform, "FocusPointsText", 0f,0.40f,1f,0.80f, "0",       24f, Color.white);
        MakeStat(statsGO.transform, "IncomeRateText",  0f,0.18f,1f,0.42f, "+1.0/s",  10f, new Color(0.2f,0.85f,0.7f,1f));
        MakeStat(statsGO.transform, "SessionCountText",0f,0.02f,1f,0.20f, "Sess: 0", 8f,  new Color(0.4f,0.4f,0.4f,1f));

        // ══════════════════════════════════════════
        // CENTER-LEFT: FARM PLOTS (world space — plots positioned by code)
        // Just a label to show farm area
        // ══════════════════════════════════════════
        // Farm area = FP_W to (1 - POMO_W - SHOP_W)
        // Plots are world-space objects, not UI

        // ══════════════════════════════════════════
        // CENTER-RIGHT: CROP SHOP
        // ══════════════════════════════════════════
        var shopGO = new GameObject("CropShopPanel");
        shopGO.transform.SetParent(canvas.transform, false);
        SetRect(shopGO, 1f-POMO_W-SHOP_W+0.005f, 0.02f, 1f-POMO_W-0.008f, 0.98f);
        shopGO.AddComponent<Image>().color = Color.clear;
        shopGO.AddComponent<CropShopUIController>();
        MakeStat(shopGO.transform, "ShopTitle", 0f,0.88f,1f,1f, "CROPS", 8f, new Color(0.45f,0.45f,0.48f,1f));

        // Auto-Farmer button just left of shop
        var afGO = new GameObject("AutoFarmerBtn");
        afGO.transform.SetParent(canvas.transform, false);
        float afX0 = 1f - POMO_W - SHOP_W - 0.10f;
        float afX1 = 1f - POMO_W - SHOP_W - 0.005f;
        SetRect(afGO, afX0, 0.15f, afX1, 0.85f);
        var afImg = afGO.AddComponent<Image>(); afImg.color = new Color(0.15f,0.28f,0.45f,1f);
        var afBtn = afGO.AddComponent<Button>(); afBtn.targetGraphic = afImg;
        var afCols = afBtn.colors;
        afCols.highlightedColor = new Color(0.22f,0.38f,0.58f,1f);
        afCols.pressedColor     = new Color(0.10f,0.20f,0.32f,1f);
        afBtn.colors = afCols;
        var afTxt = new GameObject("Text");
        afTxt.transform.SetParent(afGO.transform, false);
        var afR = afTxt.AddComponent<RectTransform>();
        afR.anchorMin=Vector2.zero; afR.anchorMax=Vector2.one;
        afR.offsetMin=new Vector2(3f,2f); afR.offsetMax=new Vector2(-3f,-2f);
        var afTMP = afTxt.AddComponent<TextMeshProUGUI>();
        afTMP.text = "Auto\nFarmer\nLv1\n200FP";
        afTMP.fontSize = 8f; afTMP.color = new Color(0.8f,0.9f,1f,1f);
        afTMP.alignment = TextAlignmentOptions.Center;
        afTMP.raycastTarget = false;

        // ══════════════════════════════════════════
        // RIGHT: POMODORO TIMER
        // ══════════════════════════════════════════
        float px0 = 1f - POMO_W;

        // Timer display
        MakeText(canvas, "StatusText", "Focus", 10f,
            new Color(0.45f,0.48f,0.52f,1f), 3,
            px0+0.005f, 0.72f, 1f-0.005f, 0.92f);

        MakeText(canvas, "TimerText", "25:00", 30f, Color.white, 3,
            px0+0.005f, 0.40f, 1f-0.005f, 0.74f);

        MakeText(canvas, "DurationLabel", "25 min", 8f,
            new Color(0.40f,0.42f,0.46f,1f), 3,
            px0+0.01f, 0.26f, 1f-0.01f, 0.40f);

        // Progress bar (thin horizontal under timer)
        var progGO = MakeImg(canvas, "ProgressRing",
            new Color(0.20f, 0.85f, 0.70f, 0.9f), 2,
            px0+0.005f, 0.20f, 1f-0.005f, 0.27f);
        var progImg = progGO.GetComponent<Image>();
        progImg.type       = Image.Type.Filled;
        progImg.fillMethod = Image.FillMethod.Horizontal;
        progImg.fillAmount = 1f;
        progImg.fillOrigin = 0;

        // Cycle dots
        var dotsGO = new GameObject("CycleDots");
        dotsGO.transform.SetParent(canvas.transform, false);
        var dotsRect = dotsGO.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(px0+0.01f, 0.10f);
        dotsRect.anchorMax = new Vector2(1f-0.01f,  0.20f);
        dotsRect.offsetMin = Vector2.zero; dotsRect.offsetMax = Vector2.zero;
        dotsRect.anchoredPosition = Vector2.zero; dotsRect.sizeDelta = Vector2.zero;
        var hLayout = dotsGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 8f; hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childForceExpandWidth = false; hLayout.childForceExpandHeight = false;
        var dotObjs = new GameObject[4];
        for (int d = 0; d < 4; d++)
        {
            var dot = new GameObject("Dot_" + (d+1));
            dot.transform.SetParent(dotsGO.transform, false);
            var dr = dot.AddComponent<RectTransform>(); dr.sizeDelta = new Vector2(8f, 8f);
            dot.AddComponent<Image>().color = new Color(0.20f,0.85f,0.70f,0.8f);
            dotObjs[d] = dot;
            dot.SetActive(false);
        }

        // Buttons
        var btnBarGO = new GameObject("ButtonBar");
        btnBarGO.transform.SetParent(canvas.transform, false);
        SetRect(btnBarGO, px0+0.005f, 0.03f, 1f-0.005f, 0.14f);
        btnBarGO.AddComponent<Image>().color = Color.clear;

        MakeButtonInParent(btnBarGO.transform, "StartPauseButton",
            new Color(0.15f,0.62f,0.48f,1f), "Start Focus", 10f, 0f,0f,0.56f,1f);
        MakeButtonInParent(btnBarGO.transform, "ResetButton",
            new Color(0.28f,0.30f,0.35f,1f), "Reset", 10f, 0.60f,0f,1f,1f);

        // Popup anchor
        var popupGO = new GameObject("PopupAnchor");
        popupGO.transform.SetParent(canvas.transform, false);
        var pr = popupGO.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.4f,0.5f); pr.anchorMax = new Vector2(0.4f,0.5f);
        pr.anchoredPosition = Vector2.zero; pr.sizeDelta = new Vector2(200f,100f);

        // ══════════════════════════════════════════
        // FARM PLOTS: world space, inside bar
        // Camera ortho: bar is ~200px of a 720px logical height
        // Ortho size = 5 → 200/720 * 10 = 2.78 world units tall
        // Center plots vertically in bar
        // ══════════════════════════════════════════
        RepositionPlots();

        // Wire UIManager
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.timerText            = FindDeep<TextMeshProUGUI>(canvas.transform, "TimerText");
            uiMgr.statusText           = FindDeep<TextMeshProUGUI>(canvas.transform, "StatusText");
            uiMgr.focusPointsText      = FindDeep<TextMeshProUGUI>(canvas.transform, "FocusPointsText");
            uiMgr.incomeRateText       = FindDeep<TextMeshProUGUI>(canvas.transform, "IncomeRateText");
            uiMgr.sessionCountText     = FindDeep<TextMeshProUGUI>(canvas.transform, "SessionCountText");
            uiMgr.startPauseButton     = FindDeep<Button>(canvas.transform, "StartPauseButton");
            uiMgr.startPauseButtonText = FindDeep<Button>(canvas.transform, "StartPauseButton")
                                            ?.GetComponentInChildren<TextMeshProUGUI>();
            uiMgr.resetButton          = FindDeep<Button>(canvas.transform, "ResetButton");
            uiMgr.progressRing         = FindDeep<Image>(canvas.transform, "ProgressRing");
            uiMgr.cycleDots            = dotObjs;
            EditorUtility.SetDirty(canvas);
        }

        // Wire FeedbackSystem
        var fb = UnityEngine.Object.FindFirstObjectByType<FeedbackSystem>();
        if (fb != null) { fb.popupParent = popupGO.transform; EditorUtility.SetDirty(fb.gameObject); }

        // Wire AutoFarmer button
        afBtn.onClick.AddListener(() => AutoFarmer.Instance?.TryUpgrade());

        // Ensure TransparentWindow on GameManager
        var gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            var tw = gm.GetComponent<TransparentWindow>() ?? gm.AddComponent<TransparentWindow>();
            tw.barHeight    = 220;
            tw.bottomOffset = 0;
            EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildBottomBar complete + saved!");
    }

    // ── Plot positioning ──────────────────────────────────
    static void RepositionPlots()
    {
        // Bar is full width. Farm area: FP_W to (1 - POMO_W - SHOP_W - 0.10)
        // In 1920px wide: FP = 192px, farm ends at 1920*(1-0.18-0.20-0.10) = 1920*0.52 = 998px
        // Camera ortho=5, 16:9 → world width = 17.78
        // FP section = 10% → world x start = -8.89 + 1.78 = -7.11
        // Farm end = 52% → world x end = -8.89 + 9.24 = 0.35
        // Farm center x = (-7.11 + 0.35) / 2 = -3.38

        // Bar height in world: camera ortho=5 → full height = 10 world units
        // Bar is only the game window height. Camera fills the window.
        // So world y range = -5 to +5, plots should be centered at y=0

        float cx = -3.0f;
        float sp = 2.8f;

        var positions = new Vector3[]
        {
            new Vector3(cx - sp,  1.1f, 0f),
            new Vector3(cx,       1.1f, 0f),
            new Vector3(cx + sp,  1.1f, 0f),
            new Vector3(cx - sp, -1.1f, 0f),
            new Vector3(cx,      -1.1f, 0f),
            new Vector3(cx + sp, -1.1f, 0f),
        };

        // With 2 rows we need plots to fit in 200px height
        // Use smaller scale: 12 (12*0.16 = 1.92 world units)
        for (int i = 0; i < 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i + 1));
            if (go == null) continue;
            go.transform.position   = positions[i];
            go.transform.localScale = new Vector3(12f, 12f, 1f);
            EditorUtility.SetDirty(go);
        }
    }

    // ── Helpers ──────────────────────────────────────────
    static GameObject MakeImg(GameObject canvas, string name, Color col, int idx,
        float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetSiblingIndex(idx);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = false;
        return go;
    }

    static void SetRect(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
    }

    static GameObject MakeText(GameObject canvas, string name, string text,
        float size, Color col, int idx, float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetSiblingIndex(idx);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
        return go;
    }

    static void MakeButtonInParent(Transform parent, string name, Color col,
        string label, float fontSize, float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = new Vector2(2f,2f); r.offsetMax = new Vector2(-2f,-2f);
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var img = go.AddComponent<Image>(); img.color = col;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var cols = btn.colors;
        cols.highlightedColor = Color.Lerp(col, Color.white, 0.25f);
        cols.pressedColor     = Color.Lerp(col, Color.black, 0.25f);
        btn.colors = cols;
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = fontSize; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
    }

    static void MakeStat(Transform parent, string name,
        float ax, float ay, float bx, float by,
        string text, float size, Color col)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
    }

    static T FindDeep<T>(Transform root, string name) where T : Component
    {
        if (root.name == name) { var c = root.GetComponent<T>(); if (c != null) return c; }
        foreach (Transform ch in root) { var f = FindDeep<T>(ch, name); if (f != null) return f; }
        return null;
    }
}
