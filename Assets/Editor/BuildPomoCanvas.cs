using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Creates a completely separate Canvas for the Pomodoro widget.
/// This Canvas is independent of the farm UICanvas.
public class BuildPomoCanvas
{
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        // ── Delete any existing PomoCanvas ─────────────────────────────
        var existingPomo = GameObject.Find("PomoCanvas");
        if (existingPomo != null) Object.DestroyImmediate(existingPomo);

        // Also remove PomoWidget from UICanvas if present
        var uiCanvas = GameObject.Find("UICanvas");
        var pomoInUI = uiCanvas?.transform.Find("PomoWidget")?.gameObject;
        if (pomoInUI != null) Object.DestroyImmediate(pomoInUI);

        // ── Create PomoCanvas ──────────────────────────────────────────
        var pomoCanvasGO = new GameObject("PomoCanvas");
        var canvas = pomoCanvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // on top of everything

        var scaler = pomoCanvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        pomoCanvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem (only if one doesn't exist)
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ── Build PomoWidget inside PomoCanvas ─────────────────────────
        var widget = new GameObject("PomoWidget");
        widget.transform.SetParent(pomoCanvasGO.transform, false);
        var wRT = widget.AddComponent<RectTransform>();

        // Position: top-left corner, 170x220px
        wRT.anchorMin = new Vector2(0, 1);
        wRT.anchorMax = new Vector2(0, 1);
        wRT.pivot     = new Vector2(0, 1);
        wRT.sizeDelta = new Vector2(170, 225);
        wRT.anchoredPosition = new Vector2(8, -8);

        // Dark green background
        var wImg = widget.AddComponent<Image>();
        wImg.color = Hex("#161e14");
        var wOL = widget.AddComponent<Outline>();
        wOL.effectColor = Hex("#3a6a20"); wOL.effectDistance = new Vector2(2, -2);

        // PomoWidget script
        var pw = widget.AddComponent<PomoWidget>();

        // ── Header bar (drag area, 26px) ──────────────────────────────
        var header = GO("Header", widget.transform);
        RT(header, 0,1, 1,1, 0,-26, 0,0);
        Bg(header, Hex("#2a5a14"));
        // Phase label
        var phaseGO = GO("Phase", header.transform);
        RT(phaseGO, 0,0, 0.65f,1, 6,0,0,0);
        var phaseTMP = Lbl("T", phaseGO.transform, "FOCUS", 10, Hex("#b0f080"), true);
        pw.phaseLabel = phaseTMP;
        // Collapse button
        var colGO = GO("CollapseBtn", header.transform);
        RT(colGO, 0.68f,0.08f, 1f,0.92f, 0,0,-3,0);
        Bg(colGO, Hex("#1a3a0c"));
        var colBtn = colGO.AddComponent<Button>();
        Lbl("T", colGO.transform, "▲", 9, Hex("#90d060"), false);
        pw.collapseBtn = colBtn;

        // ── Body (collapsible) ────────────────────────────────────────
        var body = GO("Body", widget.transform);
        RT(body, 0,0, 1,1, 0,0, 0,-26);
        Bg(body, new Color(0,0,0,0));
        pw.bodyGO = body;

        // Progress ring background circle
        var ringBg = GO("RingBg", body.transform);
        RT(ringBg, 0.08f,0.44f, 0.92f,0.96f, 0,0,0,0);
        Bg(ringBg, Hex("#0c140c"));
        var rOL = ringBg.AddComponent<Outline>();
        rOL.effectColor = Hex("#2a5010"); rOL.effectDistance = new Vector2(1,-1);

        // Ring fill
        var ring = GO("Ring", ringBg.transform);
        RT(ring, 0.06f,0.06f, 0.94f,0.94f, 0,0,0,0);
        var ringImg = ring.AddComponent<Image>();
        ringImg.color = Hex("#18d870");
        ringImg.type = Image.Type.Filled;
        ringImg.fillMethod = Image.FillMethod.Radial360;
        ringImg.fillOrigin = (int)Image.Origin360.Top;
        ringImg.fillClockwise = true;
        ringImg.fillAmount = 1f;
        pw.progressRing = ringImg;

        // Timer text
        var timerGO = GO("TimerTxt", body.transform);
        RT(timerGO, 0.04f,0.50f, 0.96f,0.94f, 0,0,0,0);
        var timerTMP = Lbl("T", timerGO.transform, "25:00", 24, Color.white, true);
        pw.timerLabel = timerTMP;

        // FP display
        var fpGO = GO("FP", body.transform);
        RT(fpGO, 0,0.38f, 1,0.47f, 6,0,-6,0);
        Bg(fpGO, Hex("#0c180c"));
        var fpTMP = Lbl("T", fpGO.transform, "FP  0", 9, Hex("#70d030"), true);
        pw.fpLabel = fpTMP;

        // Buttons
        var btns = GO("Buttons", body.transform);
        RT(btns, 0,0, 1,0.40f, 4,4,-4,0);
        Bg(btns, new Color(0,0,0,0));

        var startGO = GO("StartBtn", btns.transform);
        RT(startGO, 0,0.1f, 0.52f,0.9f, 0,0,-2,0);
        Bg(startGO, Hex("#185c28"));
        var startOL = startGO.AddComponent<Outline>();
        startOL.effectColor=Hex("#0a2c14"); startOL.effectDistance=new Vector2(1,-1);
        var startBtn = startGO.AddComponent<Button>();
        Lbl("T", startGO.transform, "START", 9, Color.white, true);
        pw.startBtn = startBtn;

        var resetGO = GO("ResetBtn", btns.transform);
        RT(resetGO, 0.52f,0.1f, 1f,0.9f, 2,0,0,0);
        Bg(resetGO, Hex("#5c1810"));
        var resetOL = resetGO.AddComponent<Outline>();
        resetOL.effectColor=Hex("#2c0808"); resetOL.effectDistance=new Vector2(1,-1);
        var resetBtn = resetGO.AddComponent<Button>();
        Lbl("T", resetGO.transform, "RESET", 9, Color.white, true);
        pw.resetBtn = resetBtn;

        EditorUtility.SetDirty(pomoCanvasGO);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BuildPomoCanvas] PomoCanvas created as independent GameObject!");
    }

    static GameObject GO(string n, Transform p)
    { var g=new GameObject(n); g.transform.SetParent(p,false); g.AddComponent<RectTransform>(); return g; }
    static void Bg(GameObject g, Color c)
    { var i=g.GetComponent<Image>()??g.AddComponent<Image>(); i.color=c; }
    static void RT(GameObject g,float ax,float ay,float bx,float by,float l,float b,float r,float t)
    { var rt=g.GetComponent<RectTransform>(); rt.anchorMin=new Vector2(ax,ay); rt.anchorMax=new Vector2(bx,by); rt.offsetMin=new Vector2(l,b); rt.offsetMax=new Vector2(r,t); }
    static TextMeshProUGUI Lbl(string n,Transform p,string text,float sz,Color c,bool bold)
    { var g=GO(n,p); var rt=g.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero;
      var tm=g.AddComponent<TextMeshProUGUI>(); tm.text=text; tm.fontSize=sz; tm.color=c;
      tm.alignment=TextAlignmentOptions.Center; tm.fontStyle=bold?FontStyles.Bold:FontStyles.Normal;
      tm.enableWordWrapping=false; tm.overflowMode=TextOverflowModes.Truncate; return tm; }
}
