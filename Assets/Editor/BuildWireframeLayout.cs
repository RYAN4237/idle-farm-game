using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Rebuilds UI to match the wireframe:
/// - Top-left: transparent (desktop shows through)
/// - Top-right: Pomodoro timer panel
/// - Bottom: full-width farm bar
/// 
/// Window size target: 1280×720
/// Farm bar: bottom 30% = 216px
/// Right panel: right 28% = 358px, top 70% = 504px
public class BuildWireframeLayout
{
    // Proportions (matching wireframe)
    const float FARM_H  = 0.30f;   // farm bar height
    const float RIGHT_W = 0.28f;   // pomodoro panel width

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Camera: transparent background (shows desktop)
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            // Use pure black with 0 alpha → DWM makes it transparent
            // In editor we use sky blue so we can see; in build it'll be transparent
            cam.backgroundColor = new Color(0.50f, 0.78f, 0.95f, 0f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Clean up old elements
        foreach (var n in new[]{"BottomBar","BarBorder","GrassStrip","DivL","DivR",
            "TimerSection","ShopSection","FarmAreaLabel","OuterRing","ProgressRing",
            "StatusText","TimerText","DurationLabel","ButtonBar","CycleDots",
            "TopRightPanel","CropShopPanel","AutoFarmerBtn","PopupAnchor",
            "RightPanel","GrassMid","Clouds","TimerBG","FarmBG","PomoBG",
            "DesktopArea"})
            DestroyChild(canvas, n);

        // ══════════════════════════════════════════
        // 1. TRANSPARENT DESKTOP AREA (top-left)
        //    No background — camera shows through
        // ══════════════════════════════════════════
        // Nothing needed — camera renders transparent

        // ══════════════════════════════════════════
        // 2. POMODORO PANEL (top-right)
        // ══════════════════════════════════════════
        var pomoBG = MakeImg(canvas, "PomoBG",
            new Color(0.08f, 0.09f, 0.11f, 0.96f), 0,
            1f-RIGHT_W, FARM_H, 1f, 1f);

        // Top border accent
        MakeImg(canvas, "PomoTopBorder",
            new Color(0.20f, 0.85f, 0.70f, 0.8f), 1,
            1f-RIGHT_W, 1f-0.003f, 1f, 1f);

        // Left border
        MakeImg(canvas, "PomoLeftBorder",
            new Color(0.18f, 0.22f, 0.28f, 1f), 1,
            1f-RIGHT_W, FARM_H, 1f-RIGHT_W+0.002f, 1f);

        // Timer ring area (centered in pomo panel)
        var outerRing = MakeImg(canvas, "OuterRing",
            new Color(0.12f, 0.14f, 0.18f, 1f), 2,
            1f-RIGHT_W+0.01f, FARM_H+0.01f, 1f-0.01f, 1f-0.01f);
        // Keep it square via AspectRatioFitter
        var arf = outerRing.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        arf.aspectRatio = 1f;

        // Progress ring (fill bar at bottom of pomo panel)
        var progRing = MakeImg(canvas, "ProgressRing",
            new Color(0.20f, 0.85f, 0.70f, 0.9f), 3,
            1f-RIGHT_W+0.01f, FARM_H+0.01f, 1f-0.01f, FARM_H+0.038f);
        var progImg  = progRing.GetComponent<Image>();
        progImg.type       = Image.Type.Filled;
        progImg.fillMethod = Image.FillMethod.Horizontal;
        progImg.fillAmount = 1f;

        // Status text "Focus"
        var statusGO = MakeText(canvas, "StatusText", "Focus", 12f,
            new Color(0.5f,0.52f,0.56f,1f), 3,
            1f-RIGHT_W+0.01f, FARM_H + (1f-FARM_H)*0.74f,
            1f-0.01f,          1f - (1f-FARM_H)*0.04f);

        // Timer text "25:00"
        var timerTxtGO = MakeText(canvas, "TimerText", "25:00", 34f, Color.white, 3,
            1f-RIGHT_W+0.01f, FARM_H + (1f-FARM_H)*0.40f,
            1f-0.01f,          FARM_H + (1f-FARM_H)*0.74f);

        // Duration label
        var durGO = MakeText(canvas, "DurationLabel", "25 min", 9f,
            new Color(0.40f,0.42f,0.46f,1f), 3,
            1f-RIGHT_W+0.02f, FARM_H + (1f-FARM_H)*0.22f,
            1f-0.02f,          FARM_H + (1f-FARM_H)*0.38f);

        // Cycle dots
        var dotsGO  = new GameObject("CycleDots");
        dotsGO.transform.SetParent(canvas.transform, false);
        var dotsRect = dotsGO.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(1f-RIGHT_W+0.02f, FARM_H+(1f-FARM_H)*0.14f);
        dotsRect.anchorMax = new Vector2(1f-0.02f,          FARM_H+(1f-FARM_H)*0.22f);
        dotsRect.offsetMin = Vector2.zero; dotsRect.offsetMax = Vector2.zero;
        dotsRect.anchoredPosition = Vector2.zero; dotsRect.sizeDelta = Vector2.zero;
        var hLayout = dotsGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 12f; hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childForceExpandWidth = false; hLayout.childForceExpandHeight = false;
        var dotObjs = new GameObject[4];
        for (int d = 0; d < 4; d++)
        {
            var dot = new GameObject("Dot_" + (d+1));
            dot.transform.SetParent(dotsGO.transform, false);
            var dr = dot.AddComponent<RectTransform>();
            dr.sizeDelta = new Vector2(10f, 10f);
            dot.AddComponent<Image>().color = new Color(0.20f,0.85f,0.70f,1f);
            dotObjs[d] = dot;
            dot.SetActive(false);
        }

        // Buttons (Start Focus / Reset) in pomo panel
        var btnBarGO = new GameObject("ButtonBar");
        btnBarGO.transform.SetParent(canvas.transform, false);
        var bbRect = btnBarGO.AddComponent<RectTransform>();
        bbRect.anchorMin = new Vector2(1f-RIGHT_W+0.01f, FARM_H+0.01f);
        bbRect.anchorMax = new Vector2(1f-0.01f, FARM_H+0.09f);
        bbRect.offsetMin = Vector2.zero; bbRect.offsetMax = Vector2.zero;
        bbRect.anchoredPosition = Vector2.zero; bbRect.sizeDelta = Vector2.zero;
        btnBarGO.AddComponent<Image>().color = Color.clear;

        MakeButton(btnBarGO.transform, "StartPauseButton",
            new Color(0.15f,0.62f,0.48f,1f), "Start Focus", 12f, 0f,0f,0.57f,1f);
        MakeButton(btnBarGO.transform, "ResetButton",
            new Color(0.28f,0.30f,0.35f,1f), "Reset", 12f, 0.61f,0f,1f,1f);

        // ══════════════════════════════════════════
        // 3. FARM BAR (bottom, full width)
        // ══════════════════════════════════════════
        MakeImg(canvas, "FarmBG",
            new Color(0.08f, 0.09f, 0.11f, 0.97f), 0, 0f, 0f, 1f, FARM_H);
        // Top border of farm bar
        MakeImg(canvas, "FarmTopBorder",
            new Color(0.24f,0.46f,0.13f,1f), 1, 0f, FARM_H-0.004f, 1f, FARM_H+0.01f);
        // Grass strip
        MakeImg(canvas, "GrassStrip",
            new Color(0.24f,0.46f,0.13f,1f), 1, 0f, FARM_H, 1f, FARM_H+0.025f);
        // Divider between desktop area and pomo panel at top
        MakeImg(canvas, "PomoBottomBorder",
            new Color(0.18f,0.22f,0.28f,1f), 1, 1f-RIGHT_W, FARM_H-0.002f, 1f, FARM_H);

        // FP Stats (left side of farm bar)
        var statsGO = new GameObject("TopRightPanel");
        statsGO.transform.SetParent(canvas.transform, false);
        var sr = statsGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(0.01f, FARM_H*0.55f);
        sr.anchorMax = new Vector2(0.14f, FARM_H*0.98f);
        sr.offsetMin = Vector2.zero; sr.offsetMax = Vector2.zero;
        sr.anchoredPosition = Vector2.zero; sr.sizeDelta = Vector2.zero;
        statsGO.AddComponent<Image>().color = Color.clear;
        MakeStat(statsGO.transform,"FPLabel",        0f,0.74f,1f,1.00f,"FOCUS POINTS",9f,new Color(0.5f,0.5f,0.5f,1f));
        MakeStat(statsGO.transform,"FocusPointsText",0f,0.35f,1f,0.76f,"0",           22f,Color.white);
        MakeStat(statsGO.transform,"IncomeRateText", 0f,0.00f,1f,0.37f,"+1.0/s",      11f,new Color(0.2f,0.85f,0.7f,1f));

        // AutoFarmer button (farm bar)
        MakeButton(canvas.transform, "AutoFarmerBtn",
            new Color(0.15f,0.28f,0.45f,1f), "Auto-Farmer Lv1  200 FP", 9f,
            0.15f, FARM_H*0.60f, 0.32f, FARM_H*0.95f);

        // Crop shop panel (farm bar, right side)
        var shopGO = new GameObject("CropShopPanel");
        shopGO.transform.SetParent(canvas.transform, false);
        var spr = shopGO.AddComponent<RectTransform>();
        spr.anchorMin = new Vector2(0.33f, 0.01f);
        spr.anchorMax = new Vector2(1f-RIGHT_W-0.01f, FARM_H-0.01f);
        spr.offsetMin = new Vector2(2f,2f); spr.offsetMax = new Vector2(-2f,-2f);
        spr.anchoredPosition = Vector2.zero; spr.sizeDelta = Vector2.zero;
        shopGO.AddComponent<Image>().color = new Color(0.06f,0.08f,0.10f,0.8f);
        shopGO.AddComponent<CropShopUIController>();
        MakeStat(shopGO.transform,"ShopTitle",0f,0.85f,1f,1f,"CROPS",9f,new Color(0.5f,0.5f,0.5f,1f));

        // Popup anchor (center of screen)
        var popupGO = new GameObject("PopupAnchor");
        popupGO.transform.SetParent(canvas.transform, false);
        var pr = popupGO.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.5f,0.5f); pr.anchorMax = new Vector2(0.5f,0.5f);
        pr.anchoredPosition = Vector2.zero; pr.sizeDelta = new Vector2(300f,200f);

        // ══════════════════════════════════════════
        // Farm Plots: in the transparent area above farm bar
        // Keep clear of pomo panel (right 28%)
        // ══════════════════════════════════════════
        RepositionPlots();

        // ══════════════════════════════════════════
        // Wire UIManager
        // ══════════════════════════════════════════
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.timerText            = FindDeep<TextMeshProUGUI>(canvas.transform,"TimerText");
            uiMgr.statusText           = FindDeep<TextMeshProUGUI>(canvas.transform,"StatusText");
            uiMgr.focusPointsText      = FindDeep<TextMeshProUGUI>(canvas.transform,"FocusPointsText");
            uiMgr.incomeRateText       = FindDeep<TextMeshProUGUI>(canvas.transform,"IncomeRateText");
            uiMgr.startPauseButton     = FindDeep<Button>(canvas.transform,"StartPauseButton");
            uiMgr.startPauseButtonText = FindDeep<Button>(canvas.transform,"StartPauseButton")
                                            ?.GetComponentInChildren<TextMeshProUGUI>();
            uiMgr.resetButton          = FindDeep<Button>(canvas.transform,"ResetButton");
            uiMgr.progressRing         = FindDeep<Image>(canvas.transform,"ProgressRing");
            uiMgr.cycleDots            = dotObjs;
            EditorUtility.SetDirty(canvas);
        }

        // Wire FeedbackSystem
        var fb = UnityEngine.Object.FindFirstObjectByType<FeedbackSystem>();
        if (fb != null) { fb.popupParent = popupGO.transform; EditorUtility.SetDirty(fb.gameObject); }

        // Add WindowTransparency + WindowDragger to GameManager
        var gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            if (gm.GetComponent<WindowTransparency>() == null) gm.AddComponent<WindowTransparency>();
            if (gm.GetComponent<WindowDragger>()      == null) gm.AddComponent<WindowDragger>();
            EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildWireframeLayout complete + saved!");
    }

    static void RepositionPlots()
    {
        // Camera ortho=5, screen = 1280×720 (design)
        // Pomo panel = right 28% = x > 358px from right → world x < ~+4.4
        // Farm bar = bottom 30% → plots above y = -5+10*0.30 = -2.0
        // Use left 72% of screen, center x ≈ -1.4 world
        float cx = -1.8f, sp = 3.4f;
        float ry0 = 1.5f, ry1 = -0.6f; // back/front rows
        var pos = new Vector3[]
        {
            new Vector3(cx-sp, ry0,0), new Vector3(cx, ry0,0), new Vector3(cx+sp, ry0,0),
            new Vector3(cx-sp, ry1,0), new Vector3(cx, ry1,0), new Vector3(cx+sp, ry1,0),
        };
        for (int i=0;i<6;i++)
        {
            var go = GameObject.Find("FarmPlot_"+(i+1));
            if (go==null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(20f,20f,1f);
            EditorUtility.SetDirty(go);
        }
    }

    // ── Helpers ──────────────────────────────────────────
    static GameObject MakeImg(GameObject canvas, string name, Color col, int idx,
        float ax, float ay, float bx, float by)
    {
        var go=new GameObject(name); go.transform.SetParent(canvas.transform,false);
        go.transform.SetSiblingIndex(idx);
        var r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        var img=go.AddComponent<Image>(); img.color=col; img.raycastTarget=false;
        return go;
    }

    static GameObject MakeText(GameObject canvas, string name, string text,
        float size, Color col, int idx, float ax, float ay, float bx, float by)
    {
        var go=new GameObject(name); go.transform.SetParent(canvas.transform,false);
        go.transform.SetSiblingIndex(idx);
        var r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
        return go;
    }

    static void MakeButton(Transform parent, string name, Color col, string label,
        float fontSize, float ax, float ay, float bx, float by)
    {
        var go=new GameObject(name); go.transform.SetParent(parent,false);
        var r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=new Vector2(2f,2f); r.offsetMax=new Vector2(-2f,-2f);
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        var img=go.AddComponent<Image>(); img.color=col;
        var btn=go.AddComponent<Button>(); btn.targetGraphic=img;
        var cols=btn.colors;
        cols.highlightedColor=Color.Lerp(col,Color.white,0.25f);
        cols.pressedColor    =Color.Lerp(col,Color.black,0.25f);
        btn.colors=cols;
        var txtGO=new GameObject("Text"); txtGO.transform.SetParent(go.transform,false);
        var tr=txtGO.AddComponent<RectTransform>();
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var tmp=txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fontSize; tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static void MakeStat(Transform parent, string name, float ax, float ay,
        float bx, float by, string text, float size, Color col)
    {
        var go=new GameObject(name); go.transform.SetParent(parent,false);
        var r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static T FindDeep<T>(Transform root, string name) where T : Component
    {
        if (root.name==name) { var c=root.GetComponent<T>(); if(c!=null)return c; }
        foreach (Transform ch in root) { var f=FindDeep<T>(ch,name); if(f!=null)return f; }
        return null;
    }

    static void DestroyChild(GameObject parent, string name)
    {
        var t=parent.transform.Find(name);
        if (t!=null) UnityEngine.Object.DestroyImmediate(t.gameObject);
    }
}
