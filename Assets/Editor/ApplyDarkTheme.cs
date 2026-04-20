using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ApplyDarkTheme
{
    public static void Execute()
    {
        TMP_FontAsset tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (tmpFont == null) tmpFont = TMP_Settings.defaultFontAsset;
        Material tmpMat = tmpFont != null ? tmpFont.material : null;

        var gm = GameObject.Find("GameManager");
        var canvas = GameObject.Find("UICanvas");
        if (gm == null || canvas == null) { Debug.LogError("[DarkTheme] Missing GM or UICanvas"); return; }

        // Nuke all canvas children
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(canvas.transform.GetChild(i).gameObject);

        // ==== Background ====
        var bgGO = new GameObject("BackgroundPanel", typeof(RectTransform));
        bgGO.transform.SetParent(canvas.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.09f, 0.11f, 0.13f, 1f);
        bgImg.raycastTarget = false;
        Stretch(bgGO);

        // ==== Center container ====
        var centerGO = new GameObject("CenterContainer", typeof(RectTransform));
        centerGO.transform.SetParent(canvas.transform, false);
        var centerRect = centerGO.GetComponent<RectTransform>();
        centerRect.anchorMin = new Vector2(0.5f, 0.5f);
        centerRect.anchorMax = new Vector2(0.5f, 0.5f);
        centerRect.anchoredPosition = Vector2.zero;
        centerRect.sizeDelta = new Vector2(600, 600);

        // Outer ring (faint, always visible)
        var outerRingGO = new GameObject("OuterRing", typeof(RectTransform));
        outerRingGO.transform.SetParent(centerGO.transform, false);
        var outerImg = outerRingGO.AddComponent<Image>();
        outerImg.sprite = MakeRingSprite(256, 126, 124);
        outerImg.color = new Color(1f, 1f, 1f, 0.08f);
        outerImg.raycastTarget = false;
        Stretch(outerRingGO);

        // Progress ring
        var progressRingGO = new GameObject("ProgressRing", typeof(RectTransform));
        progressRingGO.transform.SetParent(centerGO.transform, false);
        var progImg = progressRingGO.AddComponent<Image>();
        progImg.sprite = MakeRingSprite(256, 126, 114);
        progImg.color = new Color(0.2f, 0.85f, 0.7f, 0.95f);
        progImg.type = Image.Type.Filled;
        progImg.fillMethod = Image.FillMethod.Radial360;
        progImg.fillOrigin = (int)Image.Origin360.Top;
        progImg.fillClockwise = true;
        progImg.fillAmount = 1f;
        progImg.raycastTarget = false;
        Stretch(progressRingGO);

        // Status text
        var statusGO = MakeTMP("StatusText", centerGO.transform, "Ready", 24, tmpFont, tmpMat,
            new Color(0.6f, 0.65f, 0.7f, 1f));
        PlaceCenter(statusGO, new Vector2(0, 90), new Vector2(400, 40));

        // Timer text
        var timerGO = MakeTMP("TimerText", centerGO.transform, "25:00", 120, tmpFont, tmpMat, Color.white);
        PlaceCenter(timerGO, new Vector2(0, 10), new Vector2(500, 140));
        timerGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Duration drag label - Image parent (hit area) + TMP child (text)
        // Unity forbids two Graphic components on the same GameObject, so we split them.
        var durationGO = new GameObject("DurationLabel", typeof(RectTransform));
        durationGO.transform.SetParent(centerGO.transform, false);
        PlaceCenter(durationGO, new Vector2(0, -70), new Vector2(240, 45));
        var durImg = durationGO.AddComponent<Image>();
        durImg.color = new Color(1f, 1f, 1f, 0.05f);
        durImg.raycastTarget = true;

        var durationTextGO = MakeTMP("Text", durationGO.transform, "\u2190 25 min \u2192", 22, tmpFont, tmpMat,
            new Color(0.55f, 0.6f, 0.65f, 1f));
        Stretch(durationTextGO);
        durationTextGO.GetComponent<TextMeshProUGUI>().raycastTarget = false;

        var dragger = durationGO.AddComponent<DurationDragger>();
        dragger.label = durationTextGO.GetComponent<TextMeshProUGUI>();

        // ==== Button bar (bottom) ====
        var btnBarGO = new GameObject("ButtonBar", typeof(RectTransform));
        btnBarGO.transform.SetParent(canvas.transform, false);
        var btnBarRect = btnBarGO.GetComponent<RectTransform>();
        btnBarRect.anchorMin = new Vector2(0.5f, 0f);
        btnBarRect.anchorMax = new Vector2(0.5f, 0f);
        btnBarRect.anchoredPosition = new Vector2(0, 100);
        btnBarRect.sizeDelta = new Vector2(400, 70);
        var barLayout = btnBarGO.AddComponent<HorizontalLayoutGroup>();
        barLayout.childAlignment = TextAnchor.MiddleCenter;
        barLayout.spacing = 20f;
        barLayout.childControlWidth = false;
        barLayout.childControlHeight = false;

        var startBtn = MakePillBtn("StartPauseButton", btnBarGO.transform, "\u25b6  Start",
            new Color(0.2f, 0.85f, 0.7f, 1f), new Color(0.09f, 0.11f, 0.13f, 1f), 180, 60, tmpFont, tmpMat);
        var resetBtn = MakePillBtn("ResetButton", btnBarGO.transform, "Reset",
            new Color(1f, 1f, 1f, 0.08f), Color.white, 120, 60, tmpFont, tmpMat);

        // ==== Top-right info panel ====
        var topRightGO = new GameObject("TopRightPanel", typeof(RectTransform));
        topRightGO.transform.SetParent(canvas.transform, false);
        var trRect = topRightGO.GetComponent<RectTransform>();
        trRect.anchorMin = new Vector2(1f, 1f);
        trRect.anchorMax = new Vector2(1f, 1f);
        trRect.pivot = new Vector2(1f, 1f);
        trRect.anchoredPosition = new Vector2(-40, -30);
        trRect.sizeDelta = new Vector2(260, 140);
        var trLayout = topRightGO.AddComponent<VerticalLayoutGroup>();
        trLayout.childAlignment = TextAnchor.UpperRight;
        trLayout.spacing = 4f;
        trLayout.childControlWidth = true;
        trLayout.childControlHeight = false;
        trLayout.childForceExpandWidth = true;

        var fpLabelGO = MakeTMP("FPLabel", topRightGO.transform, "FOCUS POINTS", 12, tmpFont, tmpMat,
            new Color(0.5f, 0.55f, 0.6f, 1f));
        var fpLTmp = fpLabelGO.GetComponent<TextMeshProUGUI>();
        fpLTmp.alignment = TextAlignmentOptions.Right;
        fpLTmp.characterSpacing = 8f;
        fpLabelGO.AddComponent<LayoutElement>().preferredHeight = 18;

        var fpValueGO = MakeTMP("FocusPointsText", topRightGO.transform, "0", 48, tmpFont, tmpMat, Color.white);
        var fpVTmp = fpValueGO.GetComponent<TextMeshProUGUI>();
        fpVTmp.alignment = TextAlignmentOptions.Right;
        fpVTmp.fontStyle = FontStyles.Bold;
        fpValueGO.AddComponent<LayoutElement>().preferredHeight = 55;

        var incomeGO = MakeTMP("IncomeRateText", topRightGO.transform, "+1.0/s", 16, tmpFont, tmpMat,
            new Color(0.2f, 0.85f, 0.7f, 1f));
        incomeGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        incomeGO.AddComponent<LayoutElement>().preferredHeight = 22;

        var sessGO = MakeTMP("SessionCountText", topRightGO.transform, "Sessions: 0", 14, tmpFont, tmpMat,
            new Color(0.45f, 0.5f, 0.55f, 1f));
        sessGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        sessGO.AddComponent<LayoutElement>().preferredHeight = 20;

        // ==== Popup anchor ====
        var anchorGO = new GameObject("PopupAnchor", typeof(RectTransform));
        anchorGO.transform.SetParent(canvas.transform, false);
        var anchorRect = anchorGO.GetComponent<RectTransform>();
        anchorRect.anchorMin = new Vector2(0.5f, 0.5f);
        anchorRect.anchorMax = new Vector2(0.5f, 0.5f);
        anchorRect.anchoredPosition = new Vector2(0, 220);
        anchorRect.sizeDelta = new Vector2(400, 100);

        // ==== Wire UIManager ====
        var ui = canvas.GetComponent<UIManager>();
        if (ui == null) ui = canvas.AddComponent<UIManager>();
        ui.timerText = timerGO.GetComponent<TextMeshProUGUI>();
        ui.startPauseButton = startBtn.GetComponent<Button>();
        ui.startPauseButtonText = startBtn.GetComponentInChildren<TextMeshProUGUI>();
        ui.resetButton = resetBtn.GetComponent<Button>();
        ui.focusPointsText = fpValueGO.GetComponent<TextMeshProUGUI>();
        ui.incomeRateText = incomeGO.GetComponent<TextMeshProUGUI>();
        ui.sessionCountText = sessGO.GetComponent<TextMeshProUGUI>();
        ui.statusText = statusGO.GetComponent<TextMeshProUGUI>();
        ui.backgroundPanel = bgImg;
        ui.durationLabelText = durationTextGO.GetComponent<TextMeshProUGUI>();
        ui.decreaseDurationBtn = null;
        ui.increaseDurationBtn = null;
        ui.idleColor = new Color(0.09f, 0.11f, 0.13f, 1f);
        ui.workingColor = new Color(0.09f, 0.11f, 0.13f, 1f);
        ui.completedColor = new Color(0.09f, 0.11f, 0.13f, 1f);

        // ==== Wire FeedbackSystem ====
        var fb = gm.GetComponent<FeedbackSystem>();
        if (fb == null) fb = gm.AddComponent<FeedbackSystem>();
        fb.popupParent = anchorGO.transform;

        // ==== Progress ring controller ====
        var ringCtrl = canvas.GetComponent<ProgressRingController>();
        if (ringCtrl == null) ringCtrl = canvas.AddComponent<ProgressRingController>();
        ringCtrl.progressRing = progImg;

        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(gm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[DarkTheme] DONE!");
    }

    static GameObject MakePillBtn(string name, Transform parent, string label, Color bgColor, Color textColor,
        float w, float h, TMP_FontAsset font, Material mat)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        go.AddComponent<Button>();
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(w, h);

        var tgo = MakeTMP("Text", go.transform, label, 22, font, mat, textColor);
        tgo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero; tr.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject MakeTMP(string n, Transform p, string text, float size, TMP_FontAsset font, Material mat, Color color)
    {
        var go = new GameObject(n, typeof(RectTransform));
        go.transform.SetParent(p, false);
        go.AddComponent<CanvasRenderer>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        if (mat != null) tmp.fontSharedMaterial = mat;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return go;
    }

    static void Stretch(GameObject go)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    static void PlaceCenter(GameObject go, Vector2 pos, Vector2 size)
    {
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
    }

    static Sprite MakeRingSprite(int size, int outerRadius, int innerRadius)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(size * 0.5f - 0.5f, size * 0.5f - 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= outerRadius && dist >= innerRadius)
                {
                    float edge = Mathf.Min(outerRadius - dist, dist - innerRadius);
                    float alpha = Mathf.Clamp01(edge);
                    tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
