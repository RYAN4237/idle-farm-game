using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BuildBottomBar2
{
    const float POMO_W = 0.18f;
    const float SHOP_W = 0.20f;
    const float FP_W   = 0.10f;
    const float AF_W   = 0.08f;  // auto-farmer column width

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Wipe all children
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);

        // ── Backgrounds ──
        Img(canvas, "GrassTop",  new Color(0.24f,0.46f,0.13f,1f), 0f,0.90f,1f-POMO_W,1f);
        Img(canvas, "PomoBG",    new Color(0.10f,0.12f,0.15f,1f), 1f-POMO_W,0f,1f,1f);
        Img(canvas, "DivPomo",   new Color(0.22f,0.27f,0.34f,1f), 1f-POMO_W,0f,1f-POMO_W+0.002f,1f);
        Img(canvas, "DivFP",     new Color(0.15f,0.18f,0.22f,1f), FP_W,0f,FP_W+0.001f,1f);
        float shopX = 1f-POMO_W-SHOP_W;
        Img(canvas, "DivShop",   new Color(0.15f,0.18f,0.22f,1f), shopX,0f,shopX+0.001f,1f);
        float afX = shopX - AF_W;
        Img(canvas, "DivAF",     new Color(0.12f,0.15f,0.19f,1f), afX,0f,afX+0.001f,1f);

        // ── FP Stats (leftmost column) ──
        var statsGO = Panel(canvas, "TopRightPanel", 0.005f,0.05f,FP_W-0.005f,0.95f);
        Stat(statsGO, "FPLabel",         0f,0.80f,1f,1.00f, "FP",      9f,  new Color(0.5f,0.5f,0.5f));
        Stat(statsGO, "FocusPointsText", 0f,0.42f,1f,0.82f, "0",       22f, Color.white);
        Stat(statsGO, "IncomeRateText",  0f,0.20f,1f,0.44f, "+1.0/s",  10f, new Color(0.2f,0.85f,0.7f));
        Stat(statsGO, "SessionCountText",0f,0.02f,1f,0.22f, "Sess: 0", 8f,  new Color(0.4f,0.4f,0.4f));

        // ── Auto-Farmer button ──
        var afGO = Btn(canvas, "AutoFarmerBtn",
            new Color(0.15f,0.28f,0.45f,1f),
            "Auto\nFarmer\nLv1\n200FP", 8f,
            afX+0.004f, 0.08f, shopX-0.004f, 0.92f);

        // ── Crop Shop ──
        var shopGO = Panel(canvas, "CropShopPanel", shopX+0.004f,0.02f,1f-POMO_W-0.004f,0.98f);
        shopGO.AddComponent<CropShopUIController>();
        Stat(shopGO.transform, "ShopTitle", 0f,0.88f,1f,1f, "CROPS", 8f, new Color(0.45f,0.45f,0.48f));

        // ── Pomodoro section ──
        float px = 1f-POMO_W;
        TextEl(canvas, "StatusText",   "Focus",  10f, new Color(0.45f,0.48f,0.52f), px+0.005f,0.76f,1f-0.005f,0.93f);
        TextEl(canvas, "TimerText",    "25:00",  28f, Color.white,                   px+0.005f,0.44f,1f-0.005f,0.78f);
        TextEl(canvas, "DurationLabel","25 min",  8f, new Color(0.40f,0.42f,0.46f), px+0.01f, 0.28f,1f-0.01f, 0.44f);

        // Progress bar
        var progGO  = Img(canvas, "ProgressRing", new Color(0.20f,0.85f,0.70f,0.9f),
            px+0.005f, 0.19f, 1f-0.005f, 0.28f);
        var progImg = progGO.GetComponent<Image>();
        progImg.type = Image.Type.Filled; progImg.fillMethod = Image.FillMethod.Horizontal;
        progImg.fillAmount = 1f;

        // Cycle dots
        var dotsGO   = new GameObject("CycleDots");
        dotsGO.transform.SetParent(canvas.transform, false);
        Rect(dotsGO, px+0.01f, 0.10f, 1f-0.01f, 0.19f);
        var hLayout  = dotsGO.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 8f; hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childForceExpandWidth = false; hLayout.childForceExpandHeight = false;
        var dotObjs  = new GameObject[4];
        for (int d = 0; d < 4; d++)
        {
            var dot = new GameObject("Dot_"+(d+1));
            dot.transform.SetParent(dotsGO.transform, false);
            var dr = dot.AddComponent<RectTransform>(); dr.sizeDelta = new Vector2(8f,8f);
            dot.AddComponent<Image>().color = new Color(0.20f,0.85f,0.70f,0.8f);
            dotObjs[d] = dot; dot.SetActive(false);
        }

        // Buttons
        var barGO = new GameObject("ButtonBar");
        barGO.transform.SetParent(canvas.transform, false);
        Rect(barGO, px+0.005f, 0.03f, 1f-0.005f, 0.13f);
        barGO.AddComponent<Image>().color = Color.clear;
        BtnInParent(barGO.transform,"StartPauseButton",new Color(0.15f,0.62f,0.48f),"Start Focus",10f,0f,0f,0.56f,1f);
        BtnInParent(barGO.transform,"ResetButton",     new Color(0.28f,0.30f,0.35f),"Reset",      10f,0.60f,0f,1f,1f);

        // PopupAnchor
        var popGO = new GameObject("PopupAnchor");
        popGO.transform.SetParent(canvas.transform, false);
        var pr = popGO.AddComponent<RectTransform>();
        pr.anchorMin = new Vector2(0.35f,0.5f); pr.anchorMax = new Vector2(0.35f,0.5f);
        pr.anchoredPosition = Vector2.zero; pr.sizeDelta = new Vector2(200f,100f);

        // ── Farm plots ──
        PositionPlots();

        // ── Wire UIManager ──
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.timerText            = Deep<TextMeshProUGUI>(canvas.transform,"TimerText");
            uiMgr.statusText           = Deep<TextMeshProUGUI>(canvas.transform,"StatusText");
            uiMgr.focusPointsText      = Deep<TextMeshProUGUI>(canvas.transform,"FocusPointsText");
            uiMgr.incomeRateText       = Deep<TextMeshProUGUI>(canvas.transform,"IncomeRateText");
            uiMgr.sessionCountText     = Deep<TextMeshProUGUI>(canvas.transform,"SessionCountText");
            uiMgr.startPauseButton     = Deep<Button>(canvas.transform,"StartPauseButton");
            uiMgr.startPauseButtonText = Deep<Button>(canvas.transform,"StartPauseButton")
                                            ?.GetComponentInChildren<TextMeshProUGUI>();
            uiMgr.resetButton          = Deep<Button>(canvas.transform,"ResetButton");
            uiMgr.progressRing         = Deep<Image>(canvas.transform,"ProgressRing");
            uiMgr.cycleDots            = dotObjs;
            EditorUtility.SetDirty(canvas);
        }

        var fb = UnityEngine.Object.FindFirstObjectByType<FeedbackSystem>();
        if (fb != null) { fb.popupParent = popGO.transform; EditorUtility.SetDirty(fb.gameObject); }

        afGO.GetComponent<Button>().onClick.AddListener(() => AutoFarmer.Instance?.TryUpgrade());

        var gm = GameObject.Find("GameManager");
        if (gm != null)
        {
            var tw = gm.GetComponent<TransparentWindow>() ?? gm.AddComponent<TransparentWindow>();
            tw.barHeight = 220; tw.bottomOffset = 0;
            EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("BuildBottomBar2 complete + saved!");
    }

    static void PositionPlots()
    {
        // Farm area: FP_W(10%) to AF start (1-POMO_W-SHOP_W-AF_W = 0.52)
        // world width = 17.78, farm center x ≈ -8.89 + 17.78*(0.10+0.52)/2 = -8.89+5.51 = -3.38
        // Bar IS the game window: ortho=5, full height = 10 world units
        // 2 rows of plots centered vertically
        float cx = -3.2f, sp = 2.6f;
        var pos = new Vector3[]
        {
            new Vector3(cx-sp, 1.2f,0), new Vector3(cx,1.2f,0), new Vector3(cx+sp,1.2f,0),
            new Vector3(cx-sp,-1.2f,0), new Vector3(cx,-1.2f,0),new Vector3(cx+sp,-1.2f,0),
        };
        for (int i = 0; i < 6; i++)
        {
            var go = GameObject.Find("FarmPlot_"+(i+1));
            if (go == null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(12f,12f,1f);
            EditorUtility.SetDirty(go);
        }
    }

    // ── Minimal helpers ──────────────────────────────────
    static GameObject Img(GameObject canvas, string name, Color col,
        float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        Rect(go, ax, ay, bx, by);
        var img = go.AddComponent<Image>(); img.color = col; img.raycastTarget = false;
        return go;
    }

    static void Rect(GameObject go, float ax, float ay, float bx, float by)
    {
        var r = go.GetComponent<RectTransform>();
        if (r == null) r = go.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero;       r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
    }

    static GameObject Panel(GameObject canvas, string name,
        float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        Rect(go, ax, ay, bx, by);
        go.AddComponent<Image>().color = Color.clear;
        return go;
    }

    static void Stat(GameObject parent, string name,
        float ax, float ay, float bx, float by, string text, float size, Color col)
        => Stat(parent.transform, name, ax, ay, bx, by, text, size, col);

    static void Stat(Transform parent, string name,
        float ax, float ay, float bx, float by, string text, float size, Color col)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        Rect(go, ax, ay, bx, by);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
    }

    static void TextEl(GameObject canvas, string name, string text,
        float size, Color col, float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name); go.transform.SetParent(canvas.transform, false);
        Rect(go, ax, ay, bx, by);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center; tmp.raycastTarget = false;
    }

    static GameObject Btn(GameObject canvas, string name, Color col,
        string label, float fontSize, float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name); go.transform.SetParent(canvas.transform, false);
        Rect(go, ax, ay, bx, by);
        var img = go.AddComponent<Image>(); img.color = col;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var c   = btn.colors;
        c.highlightedColor = Color.Lerp(col,Color.white,0.25f);
        c.pressedColor     = Color.Lerp(col,Color.black,0.25f);
        btn.colors = c;
        var t = new GameObject("Text"); t.transform.SetParent(go.transform,false);
        Rect(t, 0f,0f,1f,1f);
        var tmp = t.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fontSize; tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
        return go;
    }

    static void BtnInParent(Transform parent, string name, Color col,
        string label, float fontSize, float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name); go.transform.SetParent(parent,false);
        var r  = go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=new Vector2(2f,2f); r.offsetMax=new Vector2(-2f,-2f);
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        var img=go.AddComponent<Image>(); img.color=col;
        var btn=go.AddComponent<Button>(); btn.targetGraphic=img;
        var c=btn.colors;
        c.highlightedColor=Color.Lerp(col,Color.white,0.25f);
        c.pressedColor    =Color.Lerp(col,Color.black,0.25f);
        btn.colors=c;
        var t=new GameObject("Text"); t.transform.SetParent(go.transform,false);
        Rect(t,0f,0f,1f,1f);
        var tmp=t.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fontSize; tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static T Deep<T>(Transform root, string name) where T : Component
    {
        if (root.name==name){var c=root.GetComponent<T>();if(c!=null)return c;}
        foreach(Transform ch in root){var f=Deep<T>(ch,name);if(f!=null)return f;}
        return null;
    }
}
