using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class BuildPomoWidget
{
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. 删除旧的散落番茄钟元素 ────────────────────────────────
        string[] toDelete = { "TimerText","ButtonBar","ProgressRing","PomoBG","DecorationPanel" };
        foreach (var name in toDelete)
        {
            var go = canvas.transform.Find(name)?.gameObject;
            if (go != null) { Object.DestroyImmediate(go); Debug.Log($"Deleted: {name}"); }
        }

        // ── 2. 创建 PomoWidget 悬浮窗 ────────────────────────────────
        // 已存在则先删除
        var oldWidget = canvas.transform.Find("PomoWidget");
        if (oldWidget != null) Object.DestroyImmediate(oldWidget.gameObject);

        var widget = new GameObject("PomoWidget");
        widget.transform.SetParent(canvas.transform, false);
        var wRT = widget.AddComponent<RectTransform>();

        // 位置：左上角，固定大小160x220
        wRT.anchorMin = new Vector2(0, 1);
        wRT.anchorMax = new Vector2(0, 1);
        wRT.pivot     = new Vector2(0, 1);
        wRT.sizeDelta = new Vector2(160, 210);
        wRT.anchoredPosition = new Vector2(8, -40); // 在TopBar下面

        // 背景
        var wImg = widget.AddComponent<Image>();
        wImg.color = Hex("#1a2a1a");
        var wOL = widget.AddComponent<Outline>();
        wOL.effectColor = Hex("#3a6a20"); wOL.effectDistance = new Vector2(2,-2);

        // PomoWidget 脚本
        var pw = widget.AddComponent<PomoWidget>();

        // ── Header bar (拖动区域) ─────────────────────────────────────
        var header = GO("Header", widget.transform);
        RT(header, 0,1, 1,1, 0,-26, 0,0);
        Bg(header, Hex("#3a6a20"));
        // Phase label
        var phaseGO = GO("Phase", header.transform);
        RT(phaseGO, 0,0, 0.6f,1, 6,0, 0,0);
        var phaseTMP = Lbl("T", phaseGO.transform, "FOCUS", 10, Hex("#c8ffb0"), true);
        pw.phaseLabel = phaseTMP;
        // Collapse btn
        var colGO = GO("CollapseBtn", header.transform);
        RT(colGO, 0.6f,0.05f, 1f,0.95f, 0,0, -3,0);
        Bg(colGO, Hex("#2a4a10"));
        var colBtn = colGO.AddComponent<Button>();
        var colTMP = Lbl("T", colGO.transform, "▲", 9, Hex("#a0e080"), false);
        pw.collapseBtn = colBtn;
        pw.bodyGO = null; // set later

        // ── Body (折叠对象) ────────────────────────────────────────────
        var body = GO("Body", widget.transform);
        RT(body, 0,0, 1,1, 0,0, 0,-26);
        Bg(body, new Color(0,0,0,0));
        pw.bodyGO = body;

        // Progress ring background
        var ringBgGO = GO("RingBg", body.transform);
        RT(ringBgGO, 0.1f,0.42f, 0.9f,0.97f, 0,0, 0,0);
        Bg(ringBgGO, Hex("#0a180a"));
        var ringBgOL = ringBgGO.AddComponent<Outline>();
        ringBgOL.effectColor = Hex("#2a5a10"); ringBgOL.effectDistance = new Vector2(1,-1);

        // Progress ring fill
        var ringGO = GO("Ring", ringBgGO.transform);
        RT(ringGO, 0.08f,0.08f, 0.92f,0.92f, 0,0, 0,0);
        var ringImg = ringGO.AddComponent<Image>();
        ringImg.color = Hex("#20e080");
        ringImg.type = Image.Type.Filled;
        ringImg.fillMethod = Image.FillMethod.Radial360;
        ringImg.fillOrigin = (int)Image.Origin360.Top;
        ringImg.fillClockwise = true;
        ringImg.fillAmount = 1f;
        pw.progressRing = ringImg;

        // Timer text (on top of ring)
        var timerGO = GO("Timer", body.transform);
        RT(timerGO, 0.05f,0.47f, 0.95f,0.92f, 0,0, 0,0);
        var timerTMP = Lbl("T", timerGO.transform, "25:00", 22, Color.white, true);
        pw.timerLabel = timerTMP;

        // FP display
        var fpGO = GO("FP", body.transform);
        RT(fpGO, 0,0.36f, 1,0.46f, 6,0, -6,0);
        Bg(fpGO, Hex("#0f1f0f"));
        var fpTMP = Lbl("T", fpGO.transform, "FP: 0", 9, Hex("#80e040"), true);
        pw.fpLabel = fpTMP;

        // Buttons row
        var btnsGO = GO("Buttons", body.transform);
        RT(btnsGO, 0,0, 1,0.38f, 4,4, -4,0);
        Bg(btnsGO, new Color(0,0,0,0));

        // START button
        var startGO = GO("StartBtn", btnsGO.transform);
        RT(startGO, 0,0.1f, 0.52f,0.9f, 0,0, -2,0);
        Bg(startGO, Hex("#1a6a30"));
        var startBtn = startGO.AddComponent<Button>();
        var startOL = startGO.AddComponent<Outline>();
        startOL.effectColor = Hex("#0a3a18"); startOL.effectDistance = new Vector2(1,-1);
        Lbl("T", startGO.transform, "START", 9, Color.white, true);
        pw.startBtn = startBtn;

        // RESET button
        var resetGO = GO("ResetBtn", btnsGO.transform);
        RT(resetGO, 0.52f,0.1f, 1f,0.9f, 2,0, 0,0);
        Bg(resetGO, Hex("#6a2010"));
        var resetBtn2 = resetGO.AddComponent<Button>();
        var resetOL = resetGO.AddComponent<Outline>();
        resetOL.effectColor = Hex("#3a1008"); resetOL.effectDistance = new Vector2(1,-1);
        Lbl("T", resetGO.transform, "RESET", 9, Color.white, true);
        pw.resetBtn = resetBtn2;

        // ── 3. 把FP显示连接到TopBar/FPDisplay/Value ──────────────────
        // PomoWidget的fpLabel需要在Update里读ResourceSystem
        // 在 PomoWidget脚本里的Update添加FP更新逻辑（脚本已写好）

        // ── 4. 在TopBar保留FP显示 ────────────────────────────────────
        // TopBar/FPDisplay 保持不变，它从ResourceSystem读数据

        EditorUtility.SetDirty(widget);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BuildPomoWidget] Done! Pomo widget created at top-left.");
    }

    // ── helpers ──────────────────────────────────────────────────────
    static GameObject GO(string n, Transform p)
    { var g = new GameObject(n); g.transform.SetParent(p,false); g.AddComponent<RectTransform>(); return g; }

    static void Bg(GameObject g, Color c)
    { var i = g.GetComponent<Image>() ?? g.AddComponent<Image>(); i.color = c; }

    static void RT(GameObject g, float ax,float ay,float bx,float by,
                   float l,float b,float r,float t)
    {
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax,ay); rt.anchorMax = new Vector2(bx,by);
        rt.offsetMin = new Vector2(l,b);   rt.offsetMax  = new Vector2(r,t);
    }

    static TextMeshProUGUI Lbl(string n, Transform p, string text,
                                float sz, Color c, bool bold)
    {
        var g  = GO(n,p);
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tm = g.AddComponent<TextMeshProUGUI>();
        tm.text = text; tm.fontSize = sz; tm.color = c;
        tm.alignment = TextAlignmentOptions.Center;
        tm.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tm.enableWordWrapping = false;
        tm.overflowMode = TextOverflowModes.Truncate;
        return tm;
    }
}
