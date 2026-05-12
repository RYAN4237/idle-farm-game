using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;

/// Rebuilds the bottom widget UI to match the reference UI.png layout:
/// [DAY/TIME/FP info] | [SEEDS / FISHING RODS tabbed panel] | [SAVE / QUIT / SOUND]
public class RebuildUILayout
{
    static Color PanelBG   = new Color(0.22f, 0.18f, 0.12f, 0.95f);
    static Color TabActive = new Color(0.45f, 0.35f, 0.18f, 1f);
    static Color TabInact  = new Color(0.30f, 0.22f, 0.10f, 1f);
    static Color BorderCol = new Color(0.55f, 0.42f, 0.20f, 1f);
    static Color TextLight = new Color(0.95f, 0.90f, 0.75f, 1f);
    static Color TextDim   = new Color(0.65f, 0.60f, 0.45f, 1f);
    static Color SlotBG    = new Color(0.15f, 0.12f, 0.08f, 1f);
    static Color BtnGreen  = new Color(0.20f, 0.55f, 0.25f, 1f);
    static Color BtnRed    = new Color(0.65f, 0.18f, 0.18f, 1f);

    [MenuItem("Tools/Rebuild UI Layout")]
    public static void Run()
    {
        var canvasGO = GameObject.Find("UICanvas");
        if (canvasGO == null) { Debug.LogError("UICanvas not found"); return; }

        foreach (var n in new[] { "BottomBar", "HUD", "ExpandablePanel", "SettingsPopup", "MainStrip" })
        {
            var old = canvasGO.transform.Find(n);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        }

        var strip = MakeRect(canvasGO.transform, "MainStrip",
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        strip.gameObject.AddComponent<Image>().color = new Color(0,0,0,0);

        // ── LEFT: Info panel (220px) ──────────────────────────────────
        float leftW = 220f, rightW = 160f;
        var left = MakeRect(strip, "InfoPanel",
            new Vector2(0,0), new Vector2(0,1),
            new Vector2(8,4), new Vector2(leftW-4,-4));
        AddBG(left, PanelBG, BorderCol);

        var dayTMP  = MakeLabel(left, "DayLabel",  new Vector2(0,0.58f), Vector2.one,
            new Vector2(8,2), new Vector2(-8,-2), "DAY 1, SPRING 1", 13, TextLight, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
        var timeTMP = MakeLabel(left, "TimeLabel", new Vector2(0,0.3f),  new Vector2(1,0.58f),
            new Vector2(8,2), new Vector2(-8,-2), "8:00 AM", 12, TextDim, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        var fpTMP   = MakeLabel(left, "FPRow",     Vector2.zero, new Vector2(1,0.3f),
            new Vector2(8,4), new Vector2(-8,-4), "★  0 FP", 14, new Color(1f,0.85f,0.2f,1f), FontStyles.Bold, TextAlignmentOptions.MidlineLeft);

        var fpCtrl = left.gameObject.AddComponent<InfoPanelController>();
        fpCtrl.dayLabel = dayTMP; fpCtrl.timeLabel = timeTMP; fpCtrl.fpLabel = fpTMP;

        // ── CENTER: Backpack tabbed panel ─────────────────────────────
        var center = MakeRect(strip, "BackpackPanel",
            new Vector2(0,0), new Vector2(1,1),
            new Vector2(leftW+12,4), new Vector2(-(rightW+12),-4));
        AddBG(center, PanelBG, BorderCol);

        // Tab buttons (top 28px)
        var tabRow = MakeRect(center, "TabRow",
            new Vector2(0,1), Vector2.one, new Vector2(2,-30), new Vector2(-2,-2));

        var stGO  = MakeRect(tabRow, "SeedsTab",  new Vector2(0,0),    new Vector2(0.42f,1), Vector2.zero, Vector2.zero);
        var stImg = stGO.gameObject.AddComponent<Image>(); stImg.color = TabActive;
        var stBtn = stGO.gameObject.AddComponent<Button>();
        MakeLabelInline(stGO, "SEEDS", 11, TextLight);

        var ftGO  = MakeRect(tabRow, "FishTab",   new Vector2(0.43f,0),new Vector2(0.82f,1), Vector2.zero, Vector2.zero);
        var ftImg = ftGO.gameObject.AddComponent<Image>(); ftImg.color = TabInact;
        var ftBtn = ftGO.gameObject.AddComponent<Button>();
        MakeLabelInline(ftGO, "FISHING RODS", 10, TextDim);

        // Content area
        var content = MakeRect(center, "ContentArea",
            Vector2.zero, new Vector2(1,1), new Vector2(2,26), new Vector2(-2,-32));

        var seedsPanel = MakeRect(content, "SeedsPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        seedsPanel.gameObject.AddComponent<Image>().color = new Color(0,0,0,0);
        BuildGrid(seedsPanel, 8, 1,
            new[]{ "Wheat\nSeeds","Corn\nSeeds","Tomato\nSeeds","Pumpkin\nSeeds","Apple\nSaplings","Carrot\nSeeds","Pumpkin\nSaplings","Berry\nSeeds" },
            new[]{ "Free","50FP","120FP","250FP","300FP","80FP","400FP","180FP" },
            new Color(0.4f,0.7f,0.3f,0.6f));

        var fishPanel  = MakeRect(content, "FishPanel",  Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fishPanel.gameObject.AddComponent<Image>().color = new Color(0,0,0,0);
        BuildGrid(fishPanel, 8, 1,
            new[]{ "Basic\nRod","Silver\nLure","Floatwood\nRod","Advanced\nRod","Special\nRod","Glowing\nBait","Carbon\nRod","Glowing\nBall" },
            new[]{ "Free","200FP","350FP","500FP","750FP","900FP","1100FP","1200FP" },
            new Color(0.3f,0.5f,0.8f,0.6f));
        fishPanel.gameObject.SetActive(false);

        // Bottom buttons
        var btnRow = MakeRect(center, "BtnRow",
            Vector2.zero, new Vector2(1,0), new Vector2(4,2), new Vector2(-4,26));
        MakeButton(btnRow, "SelectBtn", "SELECT", new Vector2(0,0), new Vector2(0.38f,1), BtnGreen, 10);
        MakeButton(btnRow, "CloseBtn",  "CLOSE",  new Vector2(0.62f,0), new Vector2(1,1), BtnRed,   10);

        // Tab controller
        var tabCtrl = center.gameObject.AddComponent<BackpackTabController>();
        tabCtrl.seedsTabBtn = stBtn; tabCtrl.fishTabBtn  = ftBtn;
        tabCtrl.seedsPanel  = seedsPanel.gameObject; tabCtrl.fishPanel = fishPanel.gameObject;
        tabCtrl.seedsTabImg = stImg; tabCtrl.fishTabImg  = ftImg;

        // ── RIGHT: Action panel (160px) ───────────────────────────────
        var right = MakeRect(strip, "ActionPanel",
            new Vector2(1,0), Vector2.one,
            new Vector2(-(rightW-4),4), new Vector2(-8,-4));
        AddBG(right, PanelBG, BorderCol);

        MakeButton(right, "SaveBtn", "SAVE", new Vector2(0,0.68f), new Vector2(1,1), BtnGreen, 13);
        var quitBtn = MakeButton(right, "QuitBtn", "QUIT", new Vector2(0,0.38f), new Vector2(1,0.66f), BtnRed, 13);

        // Sound label
        var sndLabel = MakeRect(right, "SoundLabel",
            new Vector2(0,0.22f), new Vector2(1,0.38f), new Vector2(4,0), new Vector2(-4,0));
        var sndTMP = sndLabel.gameObject.AddComponent<TextMeshProUGUI>();
        sndTMP.text = "SOUND"; sndTMP.fontSize = 10; sndTMP.color = TextDim;
        sndTMP.alignment = TextAlignmentOptions.Center; sndTMP.raycastTarget = false;

        // Volume slider
        BuildSlider(right, "VolumeSlider",
            Vector2.zero, new Vector2(1,0.22f), new Vector2(8,6), new Vector2(-8,-6));

        // Gear icon (top-right corner)
        var gear = MakeRect(right, "GearIcon",
            new Vector2(0.72f,0.86f), Vector2.one, new Vector2(2,2), new Vector2(-4,-4));
        var gearImg = gear.gameObject.AddComponent<Image>();
        var gearSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/KenneyIcons/gear.png");
        if (gearSprite != null) { gearImg.sprite = gearSprite; gearImg.color = TextDim; }
        else gearImg.color = new Color(0.6f,0.6f,0.6f,0.8f);
        gearImg.preserveAspect = true;
        var gearBtn = gear.gameObject.AddComponent<Button>();

        // Settings controller on quit button
        var settingsCtrl = right.gameObject.AddComponent<SettingsMenuController>();
        settingsCtrl.quitBtn  = quitBtn.GetComponent<Button>();
        settingsCtrl.closeBtn = null;
        settingsCtrl.popup    = null;

        EditorUtility.SetDirty(canvasGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[RebuildUILayout] Done!");
    }

    // ── helpers ──────────────────────────────────────────────────────

    static RectTransform MakeRect(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax, Vector2 offMin, Vector2 offMax)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "RebuildUI");
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = offMin; r.offsetMax = offMax;
        return r;
    }

    static void AddBG(RectTransform rt, Color bg, Color border)
    {
        rt.gameObject.AddComponent<Image>().color = bg;
        var ol = rt.gameObject.AddComponent<Outline>();
        ol.effectColor = border; ol.effectDistance = new Vector2(2,-2);
    }

    static TextMeshProUGUI MakeLabel(RectTransform parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax,
        string text, float size, Color color, FontStyles style, TextAlignmentOptions align)
    {
        var r = MakeRect(parent, name, aMin, aMax, oMin, oMax);
        var t = r.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.fontStyle = style; t.alignment = align; t.raycastTarget = false;
        return t;
    }

    static void MakeLabelInline(RectTransform parent, string text, float size, Color color)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = color;
        t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false;
    }

    static Button MakeButton(Transform parent, string name, string label,
        Vector2 aMin, Vector2 aMax, Color bg, float size)
    {
        var r = MakeRect(parent, name, aMin, aMax, new Vector2(3,3), new Vector2(-3,-3));
        r.gameObject.AddComponent<Image>().color = bg;
        var btn = r.gameObject.AddComponent<Button>();
        var c = btn.colors; c.highlightedColor = Color.Lerp(bg,Color.white,0.2f);
        c.pressedColor = Color.Lerp(bg,Color.black,0.2f); btn.colors = c;
        MakeLabelInline(r, label, size, Color.white);
        return btn;
    }

    static void BuildGrid(RectTransform parent, int cols, int rows,
        string[] names, string[] costs, Color iconTint)
    {
        int total = cols * rows;
        for (int i = 0; i < total; i++)
        {
            int col = i % cols, row = i / cols;
            float xMin = (float)col/cols, xMax=(float)(col+1)/cols;
            float yMax = 1f-(float)row/rows, yMin=1f-(float)(row+1)/rows;

            var slot = MakeRect(parent,$"Slot_{i}",
                new Vector2(xMin,yMin),new Vector2(xMax,yMax),
                new Vector2(2,2),new Vector2(-2,-2));
            slot.gameObject.AddComponent<Image>().color = SlotBG;

            // Lock overlay
            var lk = new GameObject("Lock"); lk.transform.SetParent(slot,false);
            var lkR = lk.AddComponent<RectTransform>();
            lkR.anchorMin=Vector2.zero;lkR.anchorMax=Vector2.one;
            lkR.offsetMin=Vector2.zero;lkR.offsetMax=Vector2.zero;
            lk.AddComponent<Image>().color = new Color(0,0,0,0.6f);
            lk.AddComponent<Button>();
            lk.SetActive(i > 0);

            // Icon placeholder
            var icon = MakeRect(slot,"Icon",
                new Vector2(0.15f,0.35f),new Vector2(0.85f,0.95f),Vector2.zero,Vector2.zero);
            icon.gameObject.AddComponent<Image>().color = iconTint;

            // Name label
            var lbl = MakeRect(slot,"Label",
                Vector2.zero,new Vector2(1,0.38f),new Vector2(1,1),new Vector2(-1,-1));
            var lt = lbl.gameObject.AddComponent<TextMeshProUGUI>();
            lt.text = i < names.Length ? names[i] : $"Item {i}";
            lt.fontSize=7;lt.color=TextLight;lt.alignment=TextAlignmentOptions.Center;lt.raycastTarget=false;

            // Cost badge
            var badge = MakeRect(slot,"Cost",
                new Vector2(0,0.8f),new Vector2(0.65f,1f),new Vector2(1,1),new Vector2(-1,-2));
            var bt = badge.gameObject.AddComponent<TextMeshProUGUI>();
            bt.text = i < costs.Length ? costs[i] : "???";
            bt.fontSize=7;bt.color=new Color(1f,0.85f,0.2f,1f);
            bt.alignment=TextAlignmentOptions.MidlineLeft;bt.raycastTarget=false;
        }
    }

    static void BuildSlider(Transform parent, string name,
        Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
    {
        var r = MakeRect(parent, name, aMin, aMax, oMin, oMax);
        var slider = r.gameObject.AddComponent<Slider>();
        slider.minValue=0; slider.maxValue=1; slider.value=0.8f;

        var bg = new GameObject("BG"); bg.transform.SetParent(r,false);
        var bgR = bg.AddComponent<RectTransform>();
        bgR.anchorMin=Vector2.zero;bgR.anchorMax=Vector2.one;
        bgR.offsetMin=Vector2.zero;bgR.offsetMax=Vector2.zero;
        bg.AddComponent<Image>().color=new Color(0.1f,0.08f,0.05f,1f);

        var fa = new GameObject("FillArea"); fa.transform.SetParent(r,false);
        var faR = fa.AddComponent<RectTransform>();
        faR.anchorMin=new Vector2(0,0.2f);faR.anchorMax=new Vector2(1,0.8f);
        faR.offsetMin=Vector2.zero;faR.offsetMax=new Vector2(-5,0);
        var fill = new GameObject("Fill"); fill.transform.SetParent(fa.transform,false);
        var fillR = fill.AddComponent<RectTransform>();
        fillR.anchorMin=Vector2.zero;fillR.anchorMax=Vector2.one;
        fillR.offsetMin=Vector2.zero;fillR.offsetMax=Vector2.zero;
        fill.AddComponent<Image>().color=new Color(0.55f,0.42f,0.18f,1f);
        slider.fillRect=fillR;

        var ha = new GameObject("HandleArea"); ha.transform.SetParent(r,false);
        var haR = ha.AddComponent<RectTransform>();
        haR.anchorMin=Vector2.zero;haR.anchorMax=Vector2.one;
        haR.offsetMin=Vector2.zero;haR.offsetMax=Vector2.zero;
        var handle = new GameObject("Handle"); handle.transform.SetParent(ha.transform,false);
        var hR = handle.AddComponent<RectTransform>(); hR.sizeDelta=new Vector2(14,0);
        handle.AddComponent<Image>().color=new Color(0.85f,0.65f,0.25f,1f);
        slider.handleRect=hR;
        slider.targetGraphic=handle.GetComponent<Image>();
        slider.onValueChanged.AddListener(v=>AudioListener.volume=v);
    }
}
