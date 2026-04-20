using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Final complete rebuild — fixes Canvas Scaler and all button click areas.
/// Reference resolution = 1920×220 (matches the actual bar window size)
public class FinalBarBuild
{
    const float POMO_W = 0.18f;
    const float SHOP_W = 0.20f;
    const float FP_W   = 0.10f;
    const float AF_W   = 0.08f;

    // Reference resolution matching our bar window
    const float REF_W = 1920f;
    const float REF_H = 220f;

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── Fix CanvasScaler to match bar resolution ──
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(REF_W, REF_H);
            scaler.matchWidthOrHeight  = 0.5f; // match both
            scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            EditorUtility.SetDirty(canvas);
        }

        // Camera
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            cam.orthographicSize = 2.8f;
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Wipe all children
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            UnityEngine.Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);

        // ── Backgrounds ──
        Img(canvas, "GrassTop",  new Color(0.24f,0.46f,0.13f,1f), 0f,0.88f,1f-POMO_W,1f);
        Img(canvas, "PomoBG",    new Color(0.10f,0.12f,0.15f,1f), 1f-POMO_W,0f,1f,1f);
        Img(canvas, "DivPomo",   new Color(0.22f,0.27f,0.34f,1f), 1f-POMO_W,0f,1f-POMO_W+0.002f,1f);
        Img(canvas, "DivFP",     new Color(0.15f,0.18f,0.22f,1f), FP_W,0f,FP_W+0.001f,1f);
        float shopX = 1f-POMO_W-SHOP_W;
        Img(canvas, "DivShop",   new Color(0.15f,0.18f,0.22f,1f), shopX,0f,shopX+0.001f,1f);
        float afX = shopX-AF_W;
        Img(canvas, "DivAF",     new Color(0.12f,0.15f,0.19f,1f), afX,0f,afX+0.001f,1f);

        // ── FP Stats ──
        var statsGO = new GameObject("TopRightPanel");
        statsGO.transform.SetParent(canvas.transform, false);
        R(statsGO, 0.005f,0.05f,FP_W-0.005f,0.95f);
        statsGO.AddComponent<Image>().color = Color.clear;
        Stat(statsGO.transform,"FPLabel",         0f,0.82f,1f,1.00f,"FP",      9f, new Color(0.5f,0.5f,0.5f));
        Stat(statsGO.transform,"FocusPointsText", 0f,0.44f,1f,0.84f,"0",       26f,Color.white);
        Stat(statsGO.transform,"IncomeRateText",  0f,0.20f,1f,0.46f,"+1.0/s",  11f,new Color(0.2f,0.85f,0.7f));
        Stat(statsGO.transform,"SessionCountText",0f,0.02f,1f,0.22f,"Sess: 0",  8f,new Color(0.4f,0.4f,0.4f));

        // ── Auto-Farmer button ──
        var afGO = MakeBtn(canvas.transform,"AutoFarmerBtn",
            new Color(0.15f,0.28f,0.45f,1f),
            "Auto\nFarmer\nLv1\n200FP", 8f,
            afX+0.004f, 0.08f, shopX-0.004f, 0.92f);

        // ── Crop Shop ──
        var shopGO = new GameObject("CropShopPanel");
        shopGO.transform.SetParent(canvas.transform, false);
        R(shopGO, shopX+0.004f,0.02f,1f-POMO_W-0.004f,0.98f);
        shopGO.AddComponent<Image>().color = Color.clear;
        shopGO.AddComponent<CropShopUIController>();
        Stat(shopGO.transform,"ShopTitle",0f,0.88f,1f,1f,"CROPS",8f,new Color(0.45f,0.45f,0.48f));

        // ── Pomodoro ──
        float px = 1f-POMO_W;
        TxtEl(canvas,"StatusText",  "Focus",  10f,new Color(0.45f,0.48f,0.52f),px+0.005f,0.78f,1f-0.005f,0.96f);
        TxtEl(canvas,"TimerText",   "25:00",  30f,Color.white,                  px+0.005f,0.44f,1f-0.005f,0.80f);
        TxtEl(canvas,"DurationLabel","25 min",  8f,new Color(0.4f,0.42f,0.46f),px+0.01f, 0.28f,1f-0.01f, 0.44f);

        var progGO = Img(canvas,"ProgressRing",new Color(0.20f,0.85f,0.70f,0.9f),
            px+0.005f,0.20f,1f-0.005f,0.28f);
        var pi = progGO.GetComponent<Image>();
        pi.type=Image.Type.Filled; pi.fillMethod=Image.FillMethod.Horizontal; pi.fillAmount=1f;

        // Cycle dots
        var dotsGO = new GameObject("CycleDots");
        dotsGO.transform.SetParent(canvas.transform,false);
        R(dotsGO, px+0.01f,0.10f,1f-0.01f,0.20f);
        var hl = dotsGO.AddComponent<HorizontalLayoutGroup>();
        hl.spacing=8f; hl.childAlignment=TextAnchor.MiddleCenter;
        hl.childForceExpandWidth=false; hl.childForceExpandHeight=false;
        var dotObjs = new GameObject[4];
        for(int d=0;d<4;d++){
            var dot=new GameObject("Dot_"+(d+1));
            dot.transform.SetParent(dotsGO.transform,false);
            dot.AddComponent<RectTransform>().sizeDelta=new Vector2(8f,8f);
            dot.AddComponent<Image>().color=new Color(0.20f,0.85f,0.70f,0.8f);
            dotObjs[d]=dot; dot.SetActive(false);
        }

        // Buttons inside pomo section
        var bbGO = new GameObject("ButtonBar");
        bbGO.transform.SetParent(canvas.transform,false);
        R(bbGO, px+0.005f,0.03f,1f-0.005f,0.14f);
        bbGO.AddComponent<Image>().color=Color.clear;
        MakeBtnInParent(bbGO.transform,"StartPauseButton",new Color(0.15f,0.62f,0.48f),"Start Focus",11f,0f,0f,0.56f,1f);
        MakeBtnInParent(bbGO.transform,"ResetButton",     new Color(0.28f,0.30f,0.35f),"Reset",      11f,0.60f,0f,1f,1f);

        // PopupAnchor
        var popGO=new GameObject("PopupAnchor");
        popGO.transform.SetParent(canvas.transform,false);
        R(popGO,0.35f,0.3f,0.65f,0.7f);

        // ── Farm plots ──
        PositionPlots();

        // ── Wire UIManager ──
        var ui = canvas.GetComponent<UIManager>();
        if(ui!=null){
            ui.timerText            = D<TextMeshProUGUI>(canvas.transform,"TimerText");
            ui.statusText           = D<TextMeshProUGUI>(canvas.transform,"StatusText");
            ui.focusPointsText      = D<TextMeshProUGUI>(canvas.transform,"FocusPointsText");
            ui.incomeRateText       = D<TextMeshProUGUI>(canvas.transform,"IncomeRateText");
            ui.sessionCountText     = D<TextMeshProUGUI>(canvas.transform,"SessionCountText");
            ui.startPauseButton     = D<Button>(canvas.transform,"StartPauseButton");
            ui.startPauseButtonText = D<Button>(canvas.transform,"StartPauseButton")?.GetComponentInChildren<TextMeshProUGUI>();
            ui.resetButton          = D<Button>(canvas.transform,"ResetButton");
            ui.progressRing         = D<Image>(canvas.transform,"ProgressRing");
            ui.cycleDots            = dotObjs;
            EditorUtility.SetDirty(canvas);
        }

        var fb=UnityEngine.Object.FindFirstObjectByType<FeedbackSystem>();
        if(fb!=null){fb.popupParent=popGO.transform;EditorUtility.SetDirty(fb.gameObject);}

        afGO.GetComponent<Button>().onClick.AddListener(()=>AutoFarmer.Instance?.TryUpgrade());

        var gm=GameObject.Find("GameManager");
        if(gm!=null){
            var tw=gm.GetComponent<TransparentWindow>()??gm.AddComponent<TransparentWindow>();
            tw.barHeight=220; tw.bottomOffset=0; EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FinalBarBuild complete + saved!");
    }

    static void PositionPlots()
    {
        // ortho=2.8, 1920x220 window → aspect = 1920/220 = 8.727
        // world width = 2 * 2.8 * 8.727 = 48.87... no, aspect is screen ratio
        // Unity camera uses actual screen ratio, not reference resolution
        // Editor Game View at 731x411 → aspect=1.778 (16:9)
        // world width = 2 * 2.8 * 1.778 = 9.956
        // Farm area = 10% to 52% → x: -4.978+0.996 to -4.978+5.177 = -3.982 to 0.199
        float camHalfW = 2.8f * (16f/9f);
        float farmX0 = -camHalfW + camHalfW*2*0.10f;
        float farmX1 = -camHalfW + camHalfW*2*0.52f;
        float cx = (farmX0+farmX1)*0.5f;
        float w  = farmX1-farmX0;
        float sp = w/3.2f;

        var pos = new Vector3[]{
            new Vector3(cx-sp, 0.9f,0), new Vector3(cx,0.9f,0), new Vector3(cx+sp,0.9f,0),
            new Vector3(cx-sp,-0.9f,0), new Vector3(cx,-0.9f,0),new Vector3(cx+sp,-0.9f,0),
        };

        for(int i=0;i<6;i++){
            var go=GameObject.Find("FarmPlot_"+(i+1));
            if(go==null)continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(8f,8f,1f);
            var col=go.GetComponent<BoxCollider2D>();
            if(col!=null)col.size=new Vector2(0.16f,0.16f);

            var label=go.transform.Find("Label");
            if(label!=null){
                label.localScale   =new Vector3(0.034f,0.034f,1f);
                label.localPosition=new Vector3(0f,0.004f,-0.1f);
                var tmp=label.GetComponent<TMPro.TextMeshPro>();
                if(tmp!=null)tmp.fontSize=10f;
                EditorUtility.SetDirty(label.gameObject);
            }
            EditorUtility.SetDirty(go);
        }
    }

    // ── Helpers ──────────────────────────────────────────
    static GameObject Img(GameObject c, string n, Color col,
        float ax,float ay,float bx,float by)
    {
        var go=new GameObject(n); go.transform.SetParent(c.transform,false);
        R(go,ax,ay,bx,by);
        var img=go.AddComponent<Image>(); img.color=col; img.raycastTarget=false;
        return go;
    }

    static void R(GameObject go, float ax,float ay,float bx,float by)
    {
        var r=go.GetComponent<RectTransform>();
        if(r==null)r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay); r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero; r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero; r.sizeDelta=Vector2.zero;
        EditorUtility.SetDirty(go);
    }

    static void Stat(Transform p,string n,float ax,float ay,float bx,float by,
        string text,float size,Color col)
    {
        var go=new GameObject(n); go.transform.SetParent(p,false);
        R(go,ax,ay,bx,by);
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TMPro.TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static void TxtEl(GameObject c,string n,string text,float size,Color col,
        float ax,float ay,float bx,float by)
    {
        var go=new GameObject(n); go.transform.SetParent(c.transform,false);
        R(go,ax,ay,bx,by);
        var tmp=go.AddComponent<TextMeshProUGUI>();
        tmp.text=text; tmp.fontSize=size; tmp.color=col;
        tmp.alignment=TMPro.TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static GameObject MakeBtn(Transform p,string n,Color col,string label,float fs,
        float ax,float ay,float bx,float by)
    {
        var go=new GameObject(n); go.transform.SetParent(p,false);
        R(go,ax,ay,bx,by);
        var img=go.AddComponent<Image>(); img.color=col;
        var btn=go.AddComponent<Button>(); btn.targetGraphic=img;
        var cs=btn.colors;
        cs.highlightedColor=Color.Lerp(col,Color.white,0.25f);
        cs.pressedColor    =Color.Lerp(col,Color.black,0.25f);
        btn.colors=cs;
        var t=new GameObject("Text"); t.transform.SetParent(go.transform,false);
        R(t,0f,0f,1f,1f);
        var tmp=t.AddComponent<TextMeshProUGUI>();
        tmp.text=label; tmp.fontSize=fs; tmp.color=Color.white;
        tmp.alignment=TMPro.TextAlignmentOptions.Center; tmp.raycastTarget=false;
        return go;
    }

    static void MakeBtnInParent(Transform p,string n,Color col,string label,float fs,
        float ax,float ay,float bx,float by)
    {
        var go=new GameObject(n); go.transform.SetParent(p,false);
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
        tmp.alignment=TMPro.TextAlignmentOptions.Center; tmp.raycastTarget=false;
    }

    static T D<T>(Transform root,string name) where T:Component
    {
        if(root.name==name){var c=root.GetComponent<T>();if(c!=null)return c;}
        foreach(Transform ch in root){var f=D<T>(ch,name);if(f!=null)return f;}
        return null;
    }
}
