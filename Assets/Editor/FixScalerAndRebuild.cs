using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixScalerAndRebuild
{
    const float W = 1920f;
    const float H = 220f;

    // Section anchors
    const float fpX0   = 0f;
    const float fpX1   = 0.0833f;  // 160/1920
    const float farmX0 = 0.0844f;  // 162/1920
    const float farmX1 = 0.4583f;  // 880/1920
    const float afX0   = 0.4594f;  // 882/1920
    const float afX1   = 0.5365f;  // 1030/1920
    const float shopX0 = 0.5375f;  // 1032/1920
    const float shopX1 = 0.8177f;  // 1570/1920
    const float poX0   = 0.8188f;  // 1572/1920
    const float poX1   = 1.0f;

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── CanvasScaler: ScaleWithScreenSize, matchHeight=1 ──
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(W, H);
            scaler.matchWidthOrHeight  = 1f;
            EditorUtility.SetDirty(canvas);
        }

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.11f, 1f);
            cam.orthographicSize = 2.8f;
            EditorUtility.SetDirty(cam.gameObject);
        }

        // Wipe canvas
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);

        // ── Backgrounds ──
        BG(canvas,"FarmBG",   new Color(0.08f,0.09f,0.11f,1f), fpX0,  0f,   shopX1,1f);
        BG(canvas,"PomoBG",   new Color(0.10f,0.12f,0.15f,1f), poX0,  0f,   1f,    1f);
        BG(canvas,"GrassTop", new Color(0.24f,0.46f,0.13f,1f), fpX0,  0.90f,shopX1,1f);
        Div(canvas,"DivFP",   fpX1);
        Div(canvas,"DivAF",   afX0);
        Div(canvas,"DivShop", shopX0);
        Div(canvas,"DivPomo", poX0);

        // ── FP Stats ──
        var statsGO = Panel(canvas,"TopRightPanel", fpX0+0.002f,0.04f,fpX1-0.002f,0.96f);
        SubT(statsGO,"FPLabel",         0f,0.82f,1f,1f,   "FP",       9f, new Color(0.5f,0.5f,0.5f));
        SubT(statsGO,"FocusPointsText", 0f,0.44f,1f,0.84f,"0",        26f,Color.white);
        SubT(statsGO,"IncomeRateText",  0f,0.20f,1f,0.46f,"+1.0/s",   11f,new Color(0.2f,0.85f,0.7f));
        SubT(statsGO,"SessionCountText",0f,0.02f,1f,0.22f,"Sess: 0",   8f, new Color(0.4f,0.4f,0.4f));

        // ── Auto-Farmer ──
        var afGO = Btn(canvas,"AutoFarmerBtn",new Color(0.15f,0.28f,0.45f),
            "Auto\nFarmer\nLv1\n200FP",8f, afX0+0.003f,0.08f,afX1-0.003f,0.92f);
        afGO.GetComponent<Button>().onClick.AddListener(()=>AutoFarmer.Instance?.TryUpgrade());

        // ── Shop ──
        var shopGO = Panel(canvas,"CropShopPanel",shopX0+0.003f,0.02f,shopX1-0.003f,0.98f);
        shopGO.AddComponent<CropShopUIController>();
        SubT(shopGO,"ShopTitle",0f,0.88f,1f,1f,"CROPS",8f,new Color(0.45f,0.45f,0.48f));

        // ── Pomodoro ──
        T(canvas,"StatusText",  "Focus",  10f,new Color(0.45f,0.48f,0.52f),poX0+0.004f,0.80f,poX1-0.004f,0.97f);
        T(canvas,"TimerText",   "25:00",  30f,Color.white,                  poX0+0.004f,0.44f,poX1-0.004f,0.82f);
        T(canvas,"DurationLabel","25 min", 8f,new Color(0.4f,0.42f,0.46f), poX0+0.006f,0.28f,poX1-0.006f,0.44f);

        var progGO = BG(canvas,"ProgressRing",new Color(0.20f,0.85f,0.70f,0.9f),
            poX0+0.004f,0.20f,poX1-0.004f,0.28f);
        var pi=progGO.GetComponent<Image>();
        pi.type=Image.Type.Filled; pi.fillMethod=Image.FillMethod.Horizontal; pi.fillAmount=1f;

        // Cycle dots
        var dotsGO=Panel(canvas,"CycleDots",poX0+0.006f,0.10f,poX1-0.006f,0.20f);
        var hl=dotsGO.AddComponent<HorizontalLayoutGroup>();
        hl.spacing=8f; hl.childAlignment=TextAnchor.MiddleCenter;
        hl.childForceExpandWidth=false; hl.childForceExpandHeight=false;
        var dotObjs=new GameObject[4];
        for(int d=0;d<4;d++){
            var dot=new GameObject("Dot_"+(d+1));
            dot.transform.SetParent(dotsGO.transform,false);
            dot.AddComponent<RectTransform>().sizeDelta=new Vector2(8f,8f);
            dot.AddComponent<Image>().color=new Color(0.20f,0.85f,0.70f,0.8f);
            dotObjs[d]=dot; dot.SetActive(false);
        }

        // Buttons
        var bbGO=Panel(canvas,"ButtonBar",poX0+0.004f,0.03f,poX1-0.004f,0.14f);
        CBtn(bbGO.transform,"StartPauseButton",new Color(0.15f,0.62f,0.48f),"Start Focus",11f,0f,0f,0.56f,1f);
        CBtn(bbGO.transform,"ResetButton",     new Color(0.28f,0.30f,0.35f),"Reset",      11f,0.60f,0f,1f,1f);

        // Popup
        var popGO=Panel(canvas,"PopupAnchor",farmX0+0.05f,0.2f,farmX1-0.05f,0.8f);

        // Plots
        PositionPlots();

        // Wire UIManager
        var ui=canvas.GetComponent<UIManager>();
        if(ui!=null){
            ui.timerText            =D<TextMeshProUGUI>(canvas.transform,"TimerText");
            ui.statusText           =D<TextMeshProUGUI>(canvas.transform,"StatusText");
            ui.focusPointsText      =D<TextMeshProUGUI>(canvas.transform,"FocusPointsText");
            ui.incomeRateText       =D<TextMeshProUGUI>(canvas.transform,"IncomeRateText");
            ui.sessionCountText     =D<TextMeshProUGUI>(canvas.transform,"SessionCountText");
            ui.startPauseButton     =D<Button>(canvas.transform,"StartPauseButton");
            ui.startPauseButtonText =D<Button>(canvas.transform,"StartPauseButton")?.GetComponentInChildren<TextMeshProUGUI>();
            ui.resetButton          =D<Button>(canvas.transform,"ResetButton");
            ui.progressRing         =D<Image>(canvas.transform,"ProgressRing");
            ui.cycleDots            =dotObjs;
            EditorUtility.SetDirty(canvas);
        }

        var fb=Object.FindFirstObjectByType<FeedbackSystem>();
        if(fb!=null){fb.popupParent=popGO.transform;EditorUtility.SetDirty(fb.gameObject);}

        var gm=GameObject.Find("GameManager");
        if(gm!=null){
            var tw=gm.GetComponent<TransparentWindow>()??gm.AddComponent<TransparentWindow>();
            tw.barHeight=220; tw.bottomOffset=0;
            if(gm.GetComponent<WindowDragger>()==null)gm.AddComponent<WindowDragger>();
            EditorUtility.SetDirty(gm);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("FixScalerAndRebuild complete + saved!");
    }

    static void PositionPlots()
    {
        float fw=9.956f;
        float wx0=-fw/2+fw*farmX0, wx1=-fw/2+fw*farmX1;
        float cx=(wx0+wx1)*0.5f, sp=(wx1-wx0)/3.5f;
        var pos=new Vector3[]{
            new Vector3(cx-sp,0.85f,0),new Vector3(cx,0.85f,0),new Vector3(cx+sp,0.85f,0),
            new Vector3(cx-sp,-0.85f,0),new Vector3(cx,-0.85f,0),new Vector3(cx+sp,-0.85f,0),
        };
        for(int i=0;i<6;i++){
            var go=GameObject.Find("FarmPlot_"+(i+1));
            if(go==null)continue;
            go.transform.position=pos[i];
            go.transform.localScale=new Vector3(8f,8f,1f);
            var col=go.GetComponent<BoxCollider2D>();
            if(col!=null)col.size=new Vector2(0.16f,0.16f);
            var lbl=go.transform.Find("Label");
            if(lbl!=null){lbl.localScale=new Vector3(0.034f,0.034f,1f);lbl.localPosition=new Vector3(0f,0.004f,-0.1f);EditorUtility.SetDirty(lbl.gameObject);}
            EditorUtility.SetDirty(go);
        }
    }

    static void AR(GameObject go,float ax,float ay,float bx,float by){
        var r=go.GetComponent<RectTransform>();if(r==null)r=go.AddComponent<RectTransform>();
        r.anchorMin=new Vector2(ax,ay);r.anchorMax=new Vector2(bx,by);
        r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
        r.anchoredPosition=Vector2.zero;r.sizeDelta=Vector2.zero;EditorUtility.SetDirty(go);}

    static GameObject BG(GameObject c,string n,Color col,float ax,float ay,float bx,float by){
        var go=new GameObject(n);go.transform.SetParent(c.transform,false);AR(go,ax,ay,bx,by);
        var img=go.AddComponent<Image>();img.color=col;img.raycastTarget=false;return go;}

    static void Div(GameObject c,string n,float x){
        var go=new GameObject(n);go.transform.SetParent(c.transform,false);AR(go,x,0f,x+0.001f,1f);
        go.AddComponent<Image>().color=new Color(0.20f,0.25f,0.30f,1f);}

    static GameObject Panel(GameObject c,string n,float ax,float ay,float bx,float by){
        var go=new GameObject(n);go.transform.SetParent(c.transform,false);AR(go,ax,ay,bx,by);
        go.AddComponent<Image>().color=Color.clear;return go;}

    static void T(GameObject c,string n,string txt,float sz,Color col,float ax,float ay,float bx,float by){
        var go=new GameObject(n);go.transform.SetParent(c.transform,false);AR(go,ax,ay,bx,by);
        var tmp=go.AddComponent<TextMeshProUGUI>();tmp.text=txt;tmp.fontSize=sz;tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center;tmp.raycastTarget=false;}

    static void SubT(GameObject p,string n,float ax,float ay,float bx,float by,string txt,float sz,Color col){
        var go=new GameObject(n);go.transform.SetParent(p.transform,false);AR(go,ax,ay,bx,by);
        var tmp=go.AddComponent<TextMeshProUGUI>();tmp.text=txt;tmp.fontSize=sz;tmp.color=col;
        tmp.alignment=TextAlignmentOptions.Center;tmp.raycastTarget=false;}

    static GameObject Btn(GameObject c,string n,Color col,string lbl,float fs,float ax,float ay,float bx,float by){
        var go=new GameObject(n);go.transform.SetParent(c.transform,false);AR(go,ax,ay,bx,by);
        var img=go.AddComponent<Image>();img.color=col;
        var btn=go.AddComponent<Button>();btn.targetGraphic=img;
        var cs=btn.colors;cs.highlightedColor=Color.Lerp(col,Color.white,0.25f);cs.pressedColor=Color.Lerp(col,Color.black,0.25f);btn.colors=cs;
        BL(go.transform,lbl,fs);return go;}

    static void CBtn(Transform p,string n,Color col,string lbl,float fs,float ax,float ay,float bx,float by){
        var go=new GameObject(n);go.transform.SetParent(p,false);
        var r=go.AddComponent<RectTransform>();r.anchorMin=new Vector2(ax,ay);r.anchorMax=new Vector2(bx,by);
        r.offsetMin=new Vector2(2f,2f);r.offsetMax=new Vector2(-2f,-2f);r.anchoredPosition=Vector2.zero;r.sizeDelta=Vector2.zero;
        var img=go.AddComponent<Image>();img.color=col;var btn=go.AddComponent<Button>();btn.targetGraphic=img;
        var cs=btn.colors;cs.highlightedColor=Color.Lerp(col,Color.white,0.25f);cs.pressedColor=Color.Lerp(col,Color.black,0.25f);btn.colors=cs;
        BL(go.transform,lbl,fs);}

    static void BL(Transform p,string lbl,float fs){
        var t=new GameObject("Text");t.transform.SetParent(p,false);
        var r=t.AddComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;
        var tmp=t.AddComponent<TextMeshProUGUI>();tmp.text=lbl;tmp.fontSize=fs;tmp.color=Color.white;
        tmp.alignment=TextAlignmentOptions.Center;tmp.raycastTarget=false;}

    static T D<T>(Transform root,string name) where T:Component{
        if(root.name==name){var c=root.GetComponent<T>();if(c!=null)return c;}
        foreach(Transform ch in root){var f=D<T>(ch,name);if(f!=null)return f;}return null;}
}
