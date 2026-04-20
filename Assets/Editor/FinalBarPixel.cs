using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Ultimate fix: ConstantPixelSize + absolute pixel coordinates.
/// Reference: bar = 1920 x 220 pixels.
/// All UI placed in pixel coordinates directly.
public class FinalBarPixel
{
    // Absolute pixel sizes (design for 1920x220 bar)
    const float W = 1920f;
    const float H = 220f;

    // Section pixel boundaries (x)
    const float FP_X0   = 0f;
    const float FP_X1   = 160f;    // FP display: 160px
    const float FARM_X0 = 162f;    // Farm area start
    const float FARM_X1 = 880f;    // Farm area end
    const float AF_X0   = 882f;    // Auto-Farmer
    const float AF_X1   = 1030f;
    const float SHOP_X0 = 1032f;   // Crop Shop
    const float SHOP_X1 = 1570f;
    const float POMO_X0 = 1572f;   // Pomodoro
    const float POMO_X1 = 1920f;

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── CanvasScaler: Constant Pixel Size, scale=1 ──
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            EditorUtility.SetDirty(canvas);
        }

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.08f, 0.09f, 0.11f, 1f);
            cam.orthographicSize = 2.8f;
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Wipe canvas
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);

        // ── Backgrounds ──
        PxImg(canvas, "FarmBG",    new Color(0.08f,0.09f,0.11f,1f), FP_X0,0,SHOP_X1,H);
        PxImg(canvas, "PomoBG",    new Color(0.10f,0.12f,0.15f,1f), POMO_X0,0,W,H);
        PxImg(canvas, "GrassTop",  new Color(0.24f,0.46f,0.13f,1f), FP_X0,H-18f,SHOP_X1,H);
        PxImg(canvas, "DivFP",     new Color(0.20f,0.25f,0.30f,1f), FP_X1,0,FP_X1+2f,H);
        PxImg(canvas, "DivAF",     new Color(0.20f,0.25f,0.30f,1f), AF_X0,0,AF_X0+2f,H);
        PxImg(canvas, "DivShop",   new Color(0.20f,0.25f,0.30f,1f), SHOP_X0,0,SHOP_X0+2f,H);
        PxImg(canvas, "DivPomo",   new Color(0.20f,0.25f,0.30f,1f), POMO_X0,0,POMO_X0+2f,H);

        // ── FP Display ──
        PxTxt(canvas,"FPLabel",        "FP",      9f, new Color(0.5f,0.5f,0.5f),  FP_X0+4,H*0.78f,FP_X1-4,H);
        PxTxt(canvas,"FocusPointsText","0",        26f,Color.white,                FP_X0+4,H*0.38f,FP_X1-4,H*0.82f);
        PxTxt(canvas,"IncomeRateText", "+1.0/s",  11f, new Color(0.2f,0.85f,0.7f),FP_X0+4,H*0.16f,FP_X1-4,H*0.40f);
        PxTxt(canvas,"SessionCountText","Sess: 0", 8f, new Color(0.4f,0.4f,0.4f), FP_X0+4,0,       FP_X1-4,H*0.18f);

        // ── Auto-Farmer button ──
        var afGO = PxBtn(canvas, "AutoFarmerBtn",
            new Color(0.15f,0.28f,0.45f,1f),
            "Auto\nFarmer\nLv1\n200FP", 8f,
            AF_X0+6, 14f, AF_X1-6, H-14f);
        afGO.GetComponent<Button>().onClick.AddListener(
            () => AutoFarmer.Instance?.TryUpgrade());

        // ── Crop Shop ──
        var shopGO = new GameObject("CropShopPanel");
        shopGO.transform.SetParent(canvas.transform, false);
        PxRect(shopGO, SHOP_X0+4, 4, SHOP_X1-4, H-4);
        shopGO.AddComponent<Image>().color = Color.clear;
        shopGO.AddComponent<CropShopUIController>();
        // Shop title
        var stGO = new GameObject("ShopTitle");
        stGO.transform.SetParent(shopGO.transform, false);
        // local rect within shop panel
        var stR = stGO.AddComponent<RectTransform>();
        stR.anchorMin = new Vector2(0f,0.88f); stR.anchorMax = new Vector2(1f,1f);
        stR.offsetMin = Vector2.zero; stR.offsetMax = Vector2.zero;
        stR.anchoredPosition = Vector2.zero; stR.sizeDelta = Vector2.zero;
        var stT = stGO.AddComponent<TextMeshProUGUI>();
        stT.text="CROPS"; stT.fontSize=8f; stT.color=new Color(0.45f,0.45f,0.48f);
        stT.alignment=TextAlignmentOptions.Center; stT.raycastTarget=false;

        // ── Pomodoro ──
        PxTxt(canvas,"StatusText",   "Focus",  10f,new Color(0.45f,0.48f,0.52f),POMO_X0+8,H*0.78f,W-8,H*0.96f);
        PxTxt(canvas,"TimerText",    "25:00",  30f,Color.white,                  POMO_X0+8,H*0.44f,W-8,H*0.82f);
        PxTxt(canvas,"DurationLabel","25 min",  8f,new Color(0.4f,0.42f,0.46f), POMO_X0+10,H*0.28f,W-10,H*0.44f);

        // Progress bar
        var progGO = PxImg(canvas,"ProgressRing",
            new Color(0.20f,0.85f,0.70f,0.9f),
            POMO_X0+8, H*0.20f, W-8, H*0.28f);
        var pi = progGO.GetComponent<Image>();
        pi.type=Image.Type.Filled; pi.fillMethod=Image.FillMethod.Horizontal;
        pi.fillAmount=1f; pi.fillOrigin=0;

        // Cycle dots
        var dotsGO = new GameObject("CycleDots");
        dotsGO.transform.SetParent(canvas.transform, false);
        PxRect(dotsGO, POMO_X0+8, H*0.10f, W-8, H*0.20f);
        var hl = dotsGO.AddComponent<HorizontalLayoutGroup>();
        hl.spacing=8f; hl.childAlignment=TextAnchor.MiddleCenter;
        hl.childForceExpandWidth=false; hl.childForceExpandHeight=false;
        var dotObjs = new GameObject[4];
        for(int d=0;d<4;d++)
        {
            var dot=new GameObject("Dot_"+(d+1));
            dot.transform.SetParent(dotsGO.transform,false);
            dot.AddComponent<RectTransform>().sizeDelta=new Vector2(8f,8f);
            dot.AddComponent<Image>().color=new Color(0.20f,0.85f,0.70f,0.8f);
            dotObjs[d]=dot; dot.SetActive(false);
        }

        // Buttons
        var bbGO = new GameObject("ButtonBar");
        bbGO.transform.SetParent(canvas.transform, false);
        PxRect(bbGO, POMO_X0+6, 6, W-6, H*0.14f);
        bbGO.AddComponent<Image>().color = Color.clear;
        PxBtnChild(bbGO.transform,"StartPauseButton",
            new Color(0.15f,0.62f,0.48f),"Start Focus",11f, 0f,0f,0.56f,1f);
        PxBtnChild(bbGO.transform,"ResetButton",
            new Color(0.28f,0.30f,0.35f),"Reset",      11f, 0.60f,0f,1f,1f);

        // Popup anchor
        var popGO = new GameObject("PopupAnchor");
        popGO.transform.SetParent(canvas.transform, false);
        PxRect(popGO, FARM_X0+100, 20, FARM_X1-100, H-20);

        // ── Farm Plots ──
        PositionPlots();

        // ── Wire UIManager ──
        var ui = canvas.GetComponent<UIManager>();
        if(ui!=null)
        {
            ui.timerText            = D<TextMeshProUGUI>(canvas.transform,"TimerText");
            ui.statusText           = D<TextMeshProUGUI>(canvas.transform,"StatusText");
            ui.focusPointsText      = D<TextMeshProUGUI>(canvas.transform,"FocusPointsText");
            ui.incomeRateText       = D<TextMeshProUGUI>(canvas.transform,"IncomeRateText");
            ui.sessionCountText     = D<TextMeshProUGUI>(canvas.transform,"SessionCountText");
            ui.startPauseButton     = D<Button>(canvas.transform,"StartPauseButton");
            ui.startPauseButtonText = D<Button>(canvas.transform,"StartPauseButton")
                                        ?.GetComponentInChildren<TextMeshProUGUI>();
            ui.resetButton          = D<Button>(canvas.transform,"ResetButton");
            ui.progressRing         = D<Image>(canvas.transform,"ProgressRing");
            ui.cycleDots            = dotObjs;
            EditorUtility.SetDirty(canvas);
        }

        var fb = Object.FindFirstObjectByType<FeedbackSystem>();
        if(fb!=null){fb.popupParent=popGO.transform;EditorUtility.SetDirty(fb.gameObject);}

        var gm = GameObject.Find("GameManager");
        if(gm!=null)
        {
            var tw = gm.GetComponent<TransparentWindow>()??gm.AddComponent<TransparentWindow>();
            tw.barHeight=220; tw.bottomOffset=0;
            if(gm.GetComponent<WindowDragger>()==null) gm.AddComponent<WindowDragger>();
            EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FinalBarPixel complete + saved!");
    }

    static void PositionPlots()
    {
        // Farm area: FARM_X0(162) ~ FARM_X1(880) = 718px
        // Camera ortho=2.8, 16:9 → world w=9.956
        // Farm in world: -4.978 + 9.956*(162/1920) to -4.978 + 9.956*(880/1920)
        float fw = 9.956f;
        float fx0 = -fw/2 + fw*(FARM_X0/W);
        float fx1 = -fw/2 + fw*(FARM_X1/W);
        float cx  = (fx0+fx1)*0.5f;
        float sp  = (fx1-fx0)/3.6f;
        float ry0 =  0.85f, ry1 = -0.85f;

        var pos = new Vector3[]{
            new Vector3(cx-sp,ry0,0),new Vector3(cx,ry0,0),new Vector3(cx+sp,ry0,0),
            new Vector3(cx-sp,ry1,0),new Vector3(cx,ry1,0),new Vector3(cx+sp,ry1,0),
        };

        for(int i=0;i<6;i++)
        {
            var go = GameObject.Find("FarmPlot_"+(i+1));
            if(go==null)continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(8f,8f,1f);
            var col=go.GetComponent<BoxCollider2D>();
            if(col!=null)col.size=new Vector2(0.16f,0.16f);
            var label=go.transform.Find("Label");
            if(label!=null)
            {
                label.localScale   =new Vector3(0.034f,0.034f,1f);
                label.localPosition=new Vector3(0f,0.004f,-0.1f);
                var tmp=label.GetComponent<TMPro.TextMeshPro>();
                if(tmp!=null)tmp.fontSize=10f;
                EditorUtility.SetDirty(label.gameObject);
            }
            EditorUtility.SetDirty(go);
        }
    }

    // ── Pixel-coordinate helpers ──────────────────────────
    // Canvas uses top-left origin in pixel mode, but Unity UI uses bottom-left.
    // We pass (x0,y0,x1,y1) where y0<y1 and y=0 is bottom of bar.

    static GameObject PxImg(GameObject c, string name, Color col,
        float x0,float y0,float x1,float y1)
    {
        var go=new GameObject(name); go.transform.SetParent(c.transform,false);
        PxRect(go,x0,y0,x1,y1);
        var img=go.AddComponent<Image>(); img.color=col; img.raycastTarget=false;
        return go;
    }

    static void PxRect(GameObject go, float x0,float y0,float x1,float y1)
    {
        var r=go.GetComponent<RectTransform>();
        if(r==null)r=go.AddComponent<RectTransform>();
        // In ConstantPixelSize, Canvas = screen size (1920x220 in build).
        // Use anchor 0,0 + offsetMin/Max to place in pixels from bottom-left.
        r.anchorMin=Vector2.zero; r.anchorMax=Vector2.zero;
        r.pivot    =Vector2.zero;
        r.anchoredPosition=new Vector2(x0,y0);
        r.sizeDelta       =new Vector2(x1-x0,y1-y0);
        EditorUtility.SetDirty(go);
    }

    static void PxTxt(GameObject c,string name,string text,float size,Color col,
        float x0,float y0,float x1,float y1)
    {
        var go=new GameObject(name); go.transform.SetParent(c.transform,false);
        PxRect(go,x0,y0,x1,y1);
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static GameObject PxBtn(GameObject c,string name,Color col,string label,float fs,
        float x0,float y0,float x1,float y1)
    {
        var go=new GameObject(name); go.transform.SetParent(c.transform,false);
        PxRect(go,x0,y0,x1,y1);
        var img=go.AddComponent<Image>(); img.color=col;
        var btn=go.AddComponent<Button>(); btn.targetGraphic=img;
        var cs=btn.colors;
        cs.highlightedColor=Color.Lerp(col,Color.white,0.25f);
        cs.pressedColor    =Color.Lerp(col,Color.black,0.25f);
        btn.colors=cs;
        var t=new GameObject("Text"); t.transform.SetParent(go.transform,false);
        var tr=t.AddComponent<RectTransform>();
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var tmp=t.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fs; tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
        return go;
    }

    static void PxBtnChild(Transform p,string name,Color col,string label,float fs,
        float ax,float ay,float bx,float by)
    {
        var go=new GameObject(name); go.transform.SetParent(p,false);
        var r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=new Vector2(2f,2f); r.offsetMax=new Vector2(-2f,-2f);
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        var img=go.AddComponent<Image>(); img.color=col;
        var btn=go.AddComponent<Button>(); btn.targetGraphic=img;
        var cs=btn.colors;
        cs.highlightedColor=Color.Lerp(col,Color.white,0.25f);
        cs.pressedColor    =Color.Lerp(col,Color.black,0.25f);
        btn.colors=cs;
        var t=new GameObject("Text"); t.transform.SetParent(go.transform,false);
        var tr=t.AddComponent<RectTransform>();
        tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var tmp=t.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fs; tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static T D<T>(Transform root,string name) where T:Component
    {
        if(root.name==name){var c=root.GetComponent<T>();if(c!=null)return c;}
        foreach(Transform ch in root){var f=D<T>(ch,name);if(f!=null)return f;}
        return null;
    }
}
