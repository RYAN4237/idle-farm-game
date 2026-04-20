using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Builds the complete bottom bar UI from scratch.
/// Left: Timer | Center: Farm labels | Right: FP + Shop + AutoFarmer
public class BuildBarFromScratch
{
    const float BAR   = 0.26f;
    const float LEFT  = 0.20f;
    const float RIGHT = 0.22f;

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.50f, 0.78f, 0.95f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Remove any leftover panels
        foreach (var n in new[]{"BottomBar","BarBorder","GrassStrip","DivL","DivR",
            "TimerSection","ShopSection","FarmAreaLabel","CenterContainer","ButtonBar",
            "CycleDots","TopRightPanel","CropShopPanel","AutoFarmerBtn","PopupAnchor",
            "RightPanel","GrassMid","Clouds"})
            Destroy(canvas, n);

        // ── Background panels ──
        MakeImg(canvas, "BottomBar",  new Color(0.08f,0.09f,0.11f,0.97f), 0, 0f,0f,1f,BAR);
        MakeImg(canvas, "GrassStrip", new Color(0.24f,0.46f,0.13f,1f),    0, 0f,BAR,1f,BAR+0.035f);
        MakeImg(canvas, "BarBorder",  new Color(0.28f,0.34f,0.42f,1f),    1, 0f,BAR-0.003f,1f,BAR);
        MakeImg(canvas, "DivL",       new Color(0.22f,0.27f,0.34f,1f),    2, LEFT-0.002f,0f,LEFT,BAR);
        MakeImg(canvas, "DivR",       new Color(0.22f,0.27f,0.34f,1f),    2, 1f-RIGHT,0f,1f-RIGHT+0.002f,BAR);

        // ════════════════════════════════
        // LEFT SECTION — Timer
        // ════════════════════════════════
        var timerBg = MakeImg(canvas,"TimerBG", new Color(0,0,0,0), 3, 0f,0f,LEFT,BAR);

        // Outer ring (background circle)
        var outerRing = MakeImg(canvas,"OuterRing", new Color(0.18f,0.20f,0.24f,1f), 4, 0f,0f,LEFT,BAR);
        outerRing.GetComponent<Image>().type = Image.Type.Simple;
        // Make it circular-looking by using a proper sprite later; for now it's square
        SquareInRect(outerRing, 0.05f, 0.04f, 0.95f, 0.96f);

        // Progress ring (filled radial)
        var progRingGO = MakeImg(canvas,"ProgressRing", new Color(0.20f,0.85f,0.70f,0.95f), 5, 0f,0f,LEFT,BAR);
        SquareInRect(progRingGO, 0.05f, 0.04f, 0.95f, 0.96f);
        var progImg = progRingGO.GetComponent<Image>();
        progImg.type       = Image.Type.Filled;
        progImg.fillMethod = Image.FillMethod.Radial360;
        progImg.fillAmount = 1f;
        progImg.fillOrigin = 2;

        // Status text ("Focus" / "Break")
        var statusGO  = MakeTMPU(canvas, "StatusText", "Focus", 11f, new Color(0.6f,0.65f,0.70f,1f), 6);
        AnchorInRect(statusGO, 0.05f, 0.64f, 0.95f, 0.80f, 0f, 0f, LEFT, BAR);

        // Timer text "25:00"
        var timerTxtGO = MakeTMPU(canvas, "TimerText", "25:00", 28f, Color.white, 6);
        AnchorInRect(timerTxtGO, 0.05f, 0.35f, 0.95f, 0.64f, 0f, 0f, LEFT, BAR);

        // Duration label
        var durGO = MakeTMPU(canvas, "DurationLabel", "25 min", 9f, new Color(0.45f,0.45f,0.50f,1f), 6);
        AnchorInRect(durGO, 0.10f, 0.12f, 0.90f, 0.27f, 0f, 0f, LEFT, BAR);

        // Cycle dots row
        var dotsGO = new GameObject("CycleDots");
        dotsGO.transform.SetParent(canvas.transform, false);
        var dotsRect = dotsGO.AddComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.01f, 0f);
        dotsRect.anchorMax = new Vector2(LEFT-0.01f, 0f);
        dotsRect.pivot     = new Vector2(0.5f, 0f);
        dotsRect.sizeDelta = new Vector2(0f, 14f);
        dotsRect.anchoredPosition = new Vector2(0f, 44f);
        var hLayout = dotsGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 10f; hLayout.childAlignment = TextAnchor.MiddleCenter;
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
            dot.SetActive(false); // hidden by default
        }

        // ButtonBar
        var barGO  = new GameObject("ButtonBar");
        barGO.transform.SetParent(canvas.transform, false);
        var barRect = barGO.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.01f, 0f);
        barRect.anchorMax = new Vector2(LEFT-0.01f, 0f);
        barRect.pivot     = new Vector2(0.5f, 0f);
        barRect.sizeDelta = new Vector2(0f, 36f);
        barRect.anchoredPosition = new Vector2(0f, 4f);
        barGO.AddComponent<Image>().color = Color.clear;

        var startBtnGO  = MakeButton(barGO.transform, "StartPauseButton",
            new Color(0.15f,0.62f,0.48f,1f), "Start Focus", 11f,
            0f,0f,0.57f,1f);
        var resetBtnGO  = MakeButton(barGO.transform, "ResetButton",
            new Color(0.28f,0.30f,0.35f,1f), "Reset", 11f,
            0.61f,0f,1f,1f);

        // ════════════════════════════════
        // RIGHT SECTION — Stats + Shop
        // ════════════════════════════════
        float x0 = 1f - RIGHT;

        // FP Stats panel
        var statsGO = new GameObject("TopRightPanel");
        statsGO.transform.SetParent(canvas.transform, false);
        var sr = statsGO.AddComponent<RectTransform>();
        sr.anchorMin = new Vector2(x0, BAR*0.66f); sr.anchorMax = new Vector2(1f, BAR);
        sr.offsetMin = new Vector2(4f,0f);          sr.offsetMax = new Vector2(-4f,-3f);
        sr.anchoredPosition = Vector2.zero;          sr.sizeDelta = Vector2.zero;
        statsGO.AddComponent<Image>().color = Color.clear;

        MakeStat(statsGO.transform,"FPLabel",        0f,0.74f,1f,1.00f, "FOCUS POINTS", 9f,  new Color(0.5f,0.5f,0.5f,1f));
        MakeStat(statsGO.transform,"FocusPointsText",0f,0.35f,1f,0.76f, "0",            20f, Color.white);
        MakeStat(statsGO.transform,"IncomeRateText", 0f,0.00f,1f,0.37f, "+1.0/s",       10f, new Color(0.2f,0.85f,0.7f,1f));

        // AutoFarmer button
        var afGO = MakeButton(canvas.transform, "AutoFarmerBtn",
            new Color(0.15f,0.28f,0.45f,1f), "Auto-Farmer Lv1  200 FP", 9f,
            x0, BAR*0.52f, 1f, BAR*0.64f);

        // CropShopPanel
        var shopPanelGO = new GameObject("CropShopPanel");
        shopPanelGO.transform.SetParent(canvas.transform, false);
        var spr = shopPanelGO.AddComponent<RectTransform>();
        spr.anchorMin = new Vector2(x0, 0f); spr.anchorMax = new Vector2(1f, BAR*0.50f);
        spr.offsetMin = new Vector2(3f,3f);   spr.offsetMax = new Vector2(-3f,-3f);
        spr.anchoredPosition = Vector2.zero;   spr.sizeDelta = Vector2.zero;
        shopPanelGO.AddComponent<Image>().color = new Color(0.06f,0.08f,0.10f,0.8f);
        shopPanelGO.AddComponent<CropShopUIController>();

        // CROPS label
        var cropsLbl = MakeTMPU(shopPanelGO, "ShopTitle", "CROPS", 9f, new Color(0.5f,0.5f,0.5f,1f), 0);
        var clRect   = cropsLbl.GetComponent<RectTransform>();
        clRect.anchorMin = new Vector2(0f,0.88f); clRect.anchorMax = new Vector2(1f,1f);
        clRect.offsetMin = Vector2.zero; clRect.offsetMax = Vector2.zero;

        // PopupAnchor
        var popupGO  = new GameObject("PopupAnchor");
        popupGO.transform.SetParent(canvas.transform, false);
        var pr = popupGO.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.5f,0.5f); pr.anchorMax = new Vector2(0.5f,0.5f);
        pr.anchoredPosition = Vector2.zero;      pr.sizeDelta = new Vector2(300f,200f);

        // ════════════════════════════════
        // Wire UIManager
        // ════════════════════════════════
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.timerText            = Find<TextMeshProUGUI>(canvas.transform, "TimerText");
            uiMgr.statusText           = Find<TextMeshProUGUI>(canvas.transform, "StatusText");
            uiMgr.focusPointsText      = Find<TextMeshProUGUI>(canvas.transform, "FocusPointsText");
            uiMgr.incomeRateText       = Find<TextMeshProUGUI>(canvas.transform, "IncomeRateText");
            uiMgr.startPauseButton     = Find<Button>(canvas.transform, "StartPauseButton");
            uiMgr.startPauseButtonText = Find<Button>(canvas.transform,"StartPauseButton")
                                            ?.GetComponentInChildren<TextMeshProUGUI>();
            uiMgr.resetButton          = Find<Button>(canvas.transform, "ResetButton");
            uiMgr.cycleDots            = dotObjs;
            uiMgr.progressRing         = null;
            EditorUtility.SetDirty(canvas);
        }

        // Wire FeedbackSystem popupParent
        var feedback = Object.FindFirstObjectByType<FeedbackSystem>();
        if (feedback != null)
        {
            feedback.popupParent = popupGO.transform;
            EditorUtility.SetDirty(feedback.gameObject);
        }

        // Wire AutoFarmer button listener
        var afBtn = Find<Button>(canvas.transform, "AutoFarmerBtn");
        if (afBtn != null)
            afBtn.onClick.AddListener(() => {
                AutoFarmer.Instance?.TryUpgrade();
            });

        // Farm plots
        RepositionPlots();

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildBarFromScratch complete + saved!");
    }

    static void RepositionPlots()
    {
        float cx=0f, sp=3.6f;
        var pos = new Vector3[]{
            new Vector3(cx-sp,1.4f,0f), new Vector3(cx,1.4f,0f), new Vector3(cx+sp,1.4f,0f),
            new Vector3(cx-sp,-1.0f,0f),new Vector3(cx,-1.0f,0f),new Vector3(cx+sp,-1.0f,0f),
        };
        for (int i=0;i<6;i++){
            var go=GameObject.Find("FarmPlot_"+(i+1));
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

    static void SquareInRect(GameObject go, float ax, float ay, float bx, float by)
    {
        var r=go.GetComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
    }

    static GameObject MakeTMPU(GameObject parent, string name, string text, float size, Color col, int idx)
    {
        var go=new GameObject(name); go.transform.SetParent(parent.transform,false);
        if (idx>=0) go.transform.SetSiblingIndex(idx);
        go.AddComponent<RectTransform>();
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
        return go;
    }

    static void AnchorInRect(GameObject go, float ax, float ay, float bx, float by,
        float parentAX, float parentAY, float parentBX, float parentBY)
    {
        // Convert local anchors inside parent area to canvas anchors
        float w=parentBX-parentAX, h=parentBY-parentAY;
        var r=go.GetComponent<RectTransform>();
        r.anchorMin=new Vector2(parentAX+ax*w, parentAY+ay*h);
        r.anchorMax=new Vector2(parentAX+bx*w, parentAY+by*h);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
    }

    static GameObject MakeButton(Transform parent, string name, Color col, string label,
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
        cols.highlightedColor=Color.Lerp(col,Color.white,0.2f);
        cols.pressedColor    =Color.Lerp(col,Color.black,0.2f);
        btn.colors=cols;
        var txtGO=new GameObject("Text"); txtGO.transform.SetParent(go.transform,false);
        var tr=txtGO.AddComponent<RectTransform>();
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var tmp=txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fontSize; tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
        return go;
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

    static T Find<T>(Transform root, string name) where T : Component
    {
        if (root.name==name) return root.GetComponent<T>();
        foreach (Transform c in root) { var f=Find<T>(c,name); if(f!=null) return f; }
        return null;
    }

    static void Destroy(GameObject parent, string name)
    {
        var t=parent.transform.Find(name);
        if (t!=null) Object.DestroyImmediate(t.gameObject);
    }
}
