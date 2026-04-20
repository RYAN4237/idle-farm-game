using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class RebuildDesktopBar
{
    const float BAR  = 0.26f;   // bar height = 26% of screen
    const float LEFT = 0.20f;   // timer section width
    const float RIGHT= 0.22f;   // shop section width

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── Camera ──
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.50f, 0.78f, 0.95f, 1f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        // ── Clean slate: remove generated panels ──
        foreach (var n in new[]{"BottomBar","BarBorder","GrassStrip","TimerSection",
                                 "ShopSection","FarmAreaLabel","GrassMid","Clouds",
                                 "PanelDivider","RightPanel","TopBackground","BackgroundPanel"})
            Destroy(canvas, n);

        // ── Bottom bar ──
        Bar(canvas, "BottomBar",  new Color(0.08f,0.09f,0.11f,0.97f), 0, 0f,0f,1f,BAR);
        Bar(canvas, "BarBorder",  new Color(0.28f,0.34f,0.42f,1f),    1, 0f,BAR-0.003f,1f,BAR);
        Bar(canvas, "GrassStrip", new Color(0.24f,0.46f,0.13f,1f),    0, 0f,BAR,1f,BAR+0.035f);

        // Left divider line
        Bar(canvas, "DivL", new Color(0.22f,0.27f,0.34f,1f), 2, LEFT-0.002f, 0f, LEFT, BAR);
        // Right divider line
        Bar(canvas, "DivR", new Color(0.22f,0.27f,0.34f,1f), 2, 1f-RIGHT, 0f, 1f-RIGHT+0.002f, BAR);

        // ── TIMER SECTION (left 20%) ──
        SetupTimerSection(canvas);

        // ── SHOP SECTION (right 22%) ──
        SetupShopSection(canvas);

        // ── Farm plots: 2 rows clearly above bar ──
        RepositionPlots();

        // ── UIManager: re-wire references ──
        RewireUIManager(canvas);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("RebuildDesktopBar complete + saved!");
    }

    // ─────────────────────────────────────────────────────
    static void SetupTimerSection(GameObject canvas)
    {
        // CenterContainer
        var center = FindAny(canvas, "CenterContainer");
        if (center != null)
        {
            center.SetParent(canvas.transform, false);
            var r = center.GetComponent<RectTransform>();
            // Anchor to left 20%, bottom bar
            r.anchorMin        = new Vector2(0f, 0f);
            r.anchorMax        = new Vector2(LEFT, BAR);
            r.offsetMin        = Vector2.zero;
            r.offsetMax        = Vector2.zero;
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(center.gameObject);

            // Fix children: ring fills container, text positioned inside
            FixChild(center, "OuterRing",   Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            FixChild(center, "ProgressRing",Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            FixChild(center, "StatusText",  new Vector2(0.1f,0.62f), new Vector2(0.9f,0.80f), Vector2.zero, Vector2.zero);
            FixChild(center, "TimerText",   new Vector2(0.05f,0.32f), new Vector2(0.95f,0.62f), Vector2.zero, Vector2.zero);
            FixChild(center, "DurationLabel",new Vector2(0.1f,0.10f), new Vector2(0.9f,0.28f), Vector2.zero, Vector2.zero);
        }

        // ButtonBar below ring inside same left zone
        var buttonBar = FindAny(canvas, "ButtonBar");
        if (buttonBar != null)
        {
            buttonBar.SetParent(canvas.transform, false);
            var r = buttonBar.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.01f, 0f);
            r.anchorMax        = new Vector2(LEFT - 0.01f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(0f, 36f);
            r.anchoredPosition = new Vector2(0f, 4f);
            EditorUtility.SetDirty(buttonBar.gameObject);

            FixButtonInBar(buttonBar, "StartPauseButton", 0f, 0f, 0.58f, 1f, new Color(0.15f,0.62f,0.48f,1f), "Start Focus");
            FixButtonInBar(buttonBar, "ResetButton",      0.62f,0f,1f,  1f, new Color(0.28f,0.30f,0.35f,1f), "Reset");
        }

        // CycleDots
        var dots = FindAny(canvas, "CycleDots");
        if (dots != null)
        {
            dots.SetParent(canvas.transform, false);
            var r = dots.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(0.5f * LEFT * 0f, 0f);
            r.anchorMax        = new Vector2(LEFT, 0f);
            r.anchorMin        = new Vector2(0.01f, 0f);
            r.anchorMax        = new Vector2(LEFT - 0.01f, 0f);
            r.pivot            = new Vector2(0.5f, 0f);
            r.sizeDelta        = new Vector2(0f, 14f);
            r.anchoredPosition = new Vector2(0f, 44f);
            EditorUtility.SetDirty(dots.gameObject);
        }
    }

    static void SetupShopSection(GameObject canvas)
    {
        float x0 = 1f - RIGHT;

        // Stats (FP display)
        var stats = FindAny(canvas, "TopRightPanel");
        if (stats != null)
        {
            stats.SetParent(canvas.transform, false);
            var r = stats.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(x0, BAR * 0.68f);
            r.anchorMax        = new Vector2(1f, BAR);
            r.offsetMin        = new Vector2(4f, 0f);
            r.offsetMax        = new Vector2(-4f, -3f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(stats.gameObject);

            SetStat(stats, "FPLabel",         0f,0.74f,1f,1.00f, 9f,  new Color(0.5f,0.5f,0.5f,1f));
            SetStat(stats, "FocusPointsText", 0f,0.35f,1f,0.76f, 20f, Color.white);
            SetStat(stats, "IncomeRateText",  0f,0.00f,1f,0.37f, 10f, new Color(0.2f,0.85f,0.7f,1f));
            var sess = stats.Find("SessionCountText");
            if (sess != null) sess.gameObject.SetActive(false);
        }

        // CropShopPanel
        var shop = FindAny(canvas, "CropShopPanel");
        if (shop != null)
        {
            shop.SetParent(canvas.transform, false);
            var r = shop.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(x0, 0f);
            r.anchorMax        = new Vector2(1f, BAR * 0.66f);
            r.offsetMin        = new Vector2(3f, 3f);
            r.offsetMax        = new Vector2(-3f, -3f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;
            EditorUtility.SetDirty(shop.gameObject);
        }

        // AutoFarmer button
        var afBtn = FindAny(canvas, "AutoFarmerBtn");
        if (afBtn != null)
        {
            afBtn.SetParent(canvas.transform, false);
            var r = afBtn.GetComponent<RectTransform>();
            r.anchorMin        = new Vector2(x0, BAR * 0.66f);
            r.anchorMax        = new Vector2(1f, BAR * 0.68f + 0.04f);
            r.offsetMin        = new Vector2(3f, 0f);
            r.offsetMax        = new Vector2(-3f, 0f);
            r.anchoredPosition = Vector2.zero;
            r.sizeDelta        = Vector2.zero;

            var img = afBtn.GetComponent<Image>() ?? afBtn.gameObject.AddComponent<Image>();
            img.color = new Color(0.15f, 0.28f, 0.45f, 1f);
            var btn = afBtn.GetComponent<Button>();
            if (btn != null) btn.targetGraphic = img;
            EditorUtility.SetDirty(afBtn.gameObject);
        }
    }

    static void RepositionPlots()
    {
        // Bar top ≈ world y = -5 + 10*0.26 = -2.4
        // Place plots: front row y=-1.0 (grass level), back row y=+1.4
        float cx = 0f, sp = 3.6f;
        var pos = new Vector3[]
        {
            new Vector3(cx-sp,  1.4f, 0f), new Vector3(cx, 1.4f, 0f), new Vector3(cx+sp,  1.4f, 0f),
            new Vector3(cx-sp, -1.0f, 0f), new Vector3(cx,-1.0f, 0f), new Vector3(cx+sp, -1.0f, 0f),
        };
        for (int i = 0; i < 6; i++)
        {
            var go = GameObject.Find("FarmPlot_" + (i+1));
            if (go == null) continue;
            go.transform.position   = pos[i];
            go.transform.localScale = new Vector3(20f, 20f, 1f);
            EditorUtility.SetDirty(go);
        }
    }

    static void RewireUIManager(GameObject canvas)
    {
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr == null) return;

        uiMgr.timerText           = FindTMP<TextMeshProUGUI>(canvas, "TimerText");
        uiMgr.statusText          = FindTMP<TextMeshProUGUI>(canvas, "StatusText");
        uiMgr.focusPointsText     = FindTMP<TextMeshProUGUI>(canvas, "FocusPointsText");
        uiMgr.incomeRateText      = FindTMP<TextMeshProUGUI>(canvas, "IncomeRateText");

        var startBtn = FindChildDeep(canvas.transform, "StartPauseButton");
        if (startBtn != null)
        {
            uiMgr.startPauseButton     = startBtn.GetComponent<Button>();
            uiMgr.startPauseButtonText = startBtn.GetComponentInChildren<TextMeshProUGUI>();
        }

        var resetBtn = FindChildDeep(canvas.transform, "ResetButton");
        if (resetBtn != null) uiMgr.resetButton = resetBtn.GetComponent<Button>();

        // CycleDots
        var dotsParent = FindChildDeep(canvas.transform, "CycleDots");
        if (dotsParent != null)
        {
            var dots = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < dotsParent.childCount; i++)
                dots.Add(dotsParent.GetChild(i).gameObject);
            uiMgr.cycleDots = dots.ToArray();
        }

        EditorUtility.SetDirty(canvas);
        Debug.Log("UIManager re-wired.");
    }

    // ── Helpers ──────────────────────────────────────────
    static void Bar(GameObject canvas, string name, Color color, int idx,
                    float ax, float ay, float bx, float by)
    {
        var go = new GameObject(name);
        go.transform.SetParent(canvas.transform, false);
        go.transform.SetSiblingIndex(idx);
        go.AddComponent<RectTransform>();
        var img = go.AddComponent<Image>();
        img.color = color; img.raycastTarget = false;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static void FixChild(Transform parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var t = parent.Find(name); if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.anchoredPosition = pos; r.sizeDelta = size;
        r.localPosition = new Vector3(r.localPosition.x, r.localPosition.y, 0f);
        EditorUtility.SetDirty(t.gameObject);
    }

    static void FixButtonInBar(Transform bar, string name,
        float ax, float ay, float bx, float by, Color col, string label)
    {
        var t = bar.Find(name); if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = new Vector2(2f,2f); r.offsetMax = new Vector2(-2f,-2f);
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var img = t.GetComponent<Image>() ?? t.gameObject.AddComponent<Image>();
        img.color = col;
        var btn = t.GetComponent<Button>(); if (btn != null) btn.targetGraphic = img;
        var txt = t.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null) { txt.text = label; txt.fontSize = 11f; txt.color = Color.white; }
        EditorUtility.SetDirty(t.gameObject);
    }

    static void SetStat(Transform parent, string name,
        float ax, float ay, float bx, float by, float size, Color col)
    {
        var t = parent.Find(name); if (t == null) return;
        var r = t.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(ax,ay); r.anchorMax = new Vector2(bx,by);
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        r.anchoredPosition = Vector2.zero; r.sizeDelta = Vector2.zero;
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) { tmp.fontSize = size; tmp.color = col; tmp.alignment = TMPro.TextAlignmentOptions.Center; }
        EditorUtility.SetDirty(t.gameObject);
    }

    static Transform FindAny(GameObject canvas, string name)
    {
        // Search entire canvas hierarchy
        return FindChildDeep(canvas.transform, name);
    }

    static Transform FindChildDeep(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) return child;
            var found = FindChildDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static T FindTMP<T>(GameObject canvas, string name) where T : Component
    {
        var t = FindChildDeep(canvas.transform, name);
        return t?.GetComponent<T>();
    }

    static void Destroy(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) Object.DestroyImmediate(t.gameObject);
    }
}
