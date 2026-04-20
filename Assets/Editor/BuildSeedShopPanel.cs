using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Rebuilds the ExpandablePanel to look like a pixel-art farm seed shop.
public class BuildSeedShopPanel
{
    // ── Pixel-art colour palette ──────────────────────────────────────
    static Color PanelBG    = Hex("#c8b89a");
    static Color PanelBorder= Hex("#6b4c2a");
    static Color TitleBG    = Hex("#8fbc5a");
    static Color TitleText  = Hex("#1a3a00");
    static Color CellBG     = Hex("#e8dcc8");
    static Color CellBorder = Hex("#8b7355");
    static Color CellHoverBG= Hex("#d4f0a0");
    static Color CatBtnBG   = Hex("#d4a843");
    static Color CatBorder  = Hex("#8b6914");
    static Color BottomBG   = Hex("#8b7355");
    static Color InfoBG     = Hex("#7a9a3c");
    static Color InfoText   = Hex("#e8f8c0");
    static Color TextDark   = Hex("#3a2810");
    static Color GoldText   = Hex("#ffe060");
    static Color CostText   = Hex("#8b6914");
    static Color CountBG    = Hex("#8fbc5a");
    static Color WhiteText  = Hex("#fff8e0");

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString(h, out Color c);
        return c;
    }

    static Sprite WhiteSprite()
    {
        var tex = new Texture2D(4, 4);
        for (int i = 0; i < 16; i++) tex.SetPixel(i%4, i/4, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,4,4), new Vector2(0.5f,0.5f));
    }

    // ── Helpers ───────────────────────────────────────────────────────
    static Image MakeImage(GameObject go, Color col, Sprite spr = null)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = col;
        if (spr != null) img.sprite = spr;
        return img;
    }

    static RectTransform RT(GameObject go) => go.GetComponent<RectTransform>();

    static void SetAnchors(GameObject go, Vector2 min, Vector2 max,
                           Vector2 offsetMin, Vector2 offsetMax)
    {
        var rt = RT(go);
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
    }

    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static TextMeshProUGUI MakeTMP(GameObject go, string text, float size,
                                   Color col, TextAlignmentOptions align = TextAlignmentOptions.Center,
                                   bool bold = false)
    {
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = align; tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Truncate;
        return tmp;
    }

    // ── Entry point ───────────────────────────────────────────────────
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var panelGO = canvas.transform.Find("ExpandablePanel")?.gameObject;
        if (panelGO == null) { Debug.LogError("ExpandablePanel not found"); return; }

        // Clear existing children
        for (int i = panelGO.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(panelGO.transform.GetChild(i).gameObject);

        var ws   = WhiteSprite();
        var panelImg = MakeImage(panelGO, PanelBG, ws);
        // Outline via Outline component
        var outline = panelGO.GetComponent<Outline>() ?? panelGO.AddComponent<Outline>();
        outline.effectColor = PanelBorder;
        outline.effectDistance = new Vector2(2, -2);

        // ── Position panel (slides in from right) ─────────────────────
        var panelRT = RT(panelGO);
        panelRT.anchorMin = new Vector2(1, 0.05f);
        panelRT.anchorMax = new Vector2(1, 0.95f);
        panelRT.pivot     = new Vector2(1, 0.5f);
        panelRT.sizeDelta = new Vector2(280, 0);
        panelRT.anchoredPosition = new Vector2(0, 0); // shown pos; hidden = +300

        // Padding container
        var pad = MakeGO("Content", panelGO.transform);
        SetAnchors(pad, Vector2.zero, Vector2.one, new Vector2(5,5), new Vector2(-5,-5));
        var padVLG = pad.AddComponent<VerticalLayoutGroup>();
        padVLG.spacing = 4; padVLG.childForceExpandWidth = true;
        padVLG.childForceExpandHeight = false;
        padVLG.padding = new RectOffset(4,4,4,4);

        // ── Title bar ─────────────────────────────────────────────────
        var titleGO = MakeGO("TitleBar", pad.transform);
        MakeImage(titleGO, TitleBG, ws);
        var titleOL = titleGO.AddComponent<Outline>();
        titleOL.effectColor = Hex("#4a7c20"); titleOL.effectDistance = new Vector2(1,-1);
        var titleLE = titleGO.AddComponent<LayoutElement>(); titleLE.preferredHeight = 26;

        var titleTxt = MakeGO("TitleText", titleGO.transform);
        SetAnchors(titleTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        MakeTMP(titleTxt, "SEEDS SHOP", 12, TitleText, TextAlignmentOptions.Center, true);

        // ── Middle row: category bar + grid ───────────────────────────
        var middleGO = MakeGO("Middle", pad.transform);
        var midLE    = middleGO.AddComponent<LayoutElement>(); midLE.flexibleHeight = 1;
        var midHLG   = middleGO.AddComponent<HorizontalLayoutGroup>();
        midHLG.spacing = 4; midHLG.childForceExpandHeight = true;
        midHLG.childForceExpandWidth = false;

        // Category sidebar
        var catBar = MakeGO("CategoryBar", middleGO.transform);
        var catLE  = catBar.AddComponent<LayoutElement>(); catLE.preferredWidth = 30;
        var catVLG = catBar.AddComponent<VerticalLayoutGroup>();
        catVLG.spacing = 3; catVLG.childForceExpandWidth = true;
        catVLG.childForceExpandHeight = false;
        catVLG.childAlignment = TextAnchor.UpperCenter;

        string[] catLabels = { "S", "F", "V", "*" };
        Color[]  catCols   = { Hex("#8fbc5a"), CatBtnBG, CatBtnBG, CatBtnBG };
        for (int i = 0; i < catLabels.Length; i++)
        {
            var btn  = MakeGO("Cat_" + catLabels[i], catBar.transform);
            MakeImage(btn, catCols[i], ws);
            var bOL  = btn.AddComponent<Outline>();
            bOL.effectColor = CatBorder; bOL.effectDistance = new Vector2(1,-1);
            var bLE  = btn.AddComponent<LayoutElement>();
            bLE.preferredHeight = 30; bLE.preferredWidth = 30;
            btn.AddComponent<Button>();

            var bTxt = MakeGO("T", btn.transform);
            SetAnchors(bTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            MakeTMP(bTxt, catLabels[i], 11, TextDark, TextAlignmentOptions.Center, true);
        }

        // Seed grid container
        var gridWrap = MakeGO("GridWrap", middleGO.transform);
        var gwLE     = gridWrap.AddComponent<LayoutElement>(); gwLE.flexibleWidth = 1;
        var gwVLG    = gridWrap.AddComponent<VerticalLayoutGroup>();
        gwVLG.spacing = 3; gwVLG.childForceExpandWidth = true;
        gwVLG.childForceExpandHeight = false;

        var gridGO = MakeGO("SeedGrid", gridWrap.transform);
        var gridLE = gridGO.AddComponent<LayoutElement>(); gridLE.flexibleHeight = 1;
        var gridGLG= gridGO.AddComponent<GridLayoutGroup>();
        gridGLG.cellSize    = new Vector2(54, 56);
        gridGLG.spacing     = new Vector2(3, 3);
        gridGLG.constraint  = GridLayoutGroup.Constraint.FixedColumnCount;
        gridGLG.constraintCount = 4;
        gridGLG.childAlignment = TextAnchor.UpperLeft;

        // Seed data
        var seeds = new (string name, string icon, int cost, int count, bool locked)[]
        {
            ("Wheat",    "W",  10,  99, false),
            ("Corn",     "C",  20,  12, false),
            ("Carrot",   "Ca", 15,   8, false),
            ("Tomato",   "T",  25,   5, false),
            ("Potato",   "P",  18,   3, false),
            ("Pumpkin",  "Pu", 40,   2, false),
            ("Strawb.",  "S",  50,   1, false),
            ("Waterml",  "W2", 80,   0, false),
            ("Sunflwr",  "Su", 60,  99, false),
            ("Rose",     "R", 120,   4, false),
            ("Mushroom", "M", 200,   0, true),
            ("Dragon",   "D", 500,   0, true),
        };

        foreach (var (name, icon, cost, count, locked) in seeds)
        {
            var cell = MakeGO("Cell_" + name, gridGO.transform);
            MakeImage(cell, locked ? Hex("#b0a090") : CellBG, ws);
            var cOL = cell.AddComponent<Outline>();
            cOL.effectColor = CellBorder; cOL.effectDistance = new Vector2(1,-1);
            if (!locked) cell.AddComponent<Button>();

            // Count badge (top-right)
            var badge = MakeGO("Count", cell.transform);
            var badgeRT = RT(badge);
            badgeRT.anchorMin = new Vector2(1,1);
            badgeRT.anchorMax = new Vector2(1,1);
            badgeRT.pivot     = new Vector2(1,1);
            badgeRT.sizeDelta = new Vector2(22, 12);
            badgeRT.anchoredPosition = new Vector2(-1, -1);
            MakeImage(badge, locked ? Hex("#888077") : CountBG, ws);
            var badgeTxt = MakeGO("T", badge.transform);
            SetAnchors(badgeTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            MakeTMP(badgeTxt, count.ToString(), 8, TitleText, TextAlignmentOptions.Center, true);

            // Icon (big letter placeholder — replace with actual sprites later)
            var iconGO = MakeGO("Icon", cell.transform);
            var iconRT = RT(iconGO);
            iconRT.anchorMin = new Vector2(0.1f, 0.35f);
            iconRT.anchorMax = new Vector2(0.9f, 0.85f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;
            MakeTMP(iconGO, icon, locked ? 10f : 14f,
                locked ? Hex("#888077") : Hex("#4a7c20"),
                TextAlignmentOptions.Center, true);

            // Name
            var nameGO = MakeGO("Name", cell.transform);
            var nameRT = RT(nameGO);
            nameRT.anchorMin = new Vector2(0, 0.18f);
            nameRT.anchorMax = new Vector2(1, 0.40f);
            nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;
            MakeTMP(nameGO, name, 7, locked ? Hex("#888077") : TextDark,
                TextAlignmentOptions.Center, true);

            // Cost
            var costGO = MakeGO("Cost", cell.transform);
            var costRT = RT(costGO);
            costRT.anchorMin = new Vector2(0, 0f);
            costRT.anchorMax = new Vector2(1, 0.20f);
            costRT.offsetMin = costRT.offsetMax = Vector2.zero;
            MakeTMP(costGO, $"$ {cost}", 7, locked ? Hex("#888077") : CostText,
                TextAlignmentOptions.Center, false);

            // Lock overlay
            if (locked)
            {
                var lockGO  = MakeGO("Lock", cell.transform);
                SetAnchors(lockGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                MakeImage(lockGO, new Color(0,0,0,0.3f), ws);
                var lockTxt = MakeGO("T", lockGO.transform);
                SetAnchors(lockTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                MakeTMP(lockTxt, "LOCK", 10, WhiteText, TextAlignmentOptions.Center, true);
            }
        }

        // Info bar
        var infoBar = MakeGO("InfoBar", gridWrap.transform);
        MakeImage(infoBar, InfoBG, ws);
        var iLE = infoBar.AddComponent<LayoutElement>(); iLE.preferredHeight = 20;
        var infoOL = infoBar.AddComponent<Outline>();
        infoOL.effectColor = Hex("#4a7c20"); infoOL.effectDistance = new Vector2(1,-1);
        var infoTxt = MakeGO("T", infoBar.transform);
        SetAnchors(infoTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        MakeTMP(infoTxt, "Select a seed to plant", 9, InfoText, TextAlignmentOptions.Center);

        // ── Bottom resource bar ────────────────────────────────────────
        var bottomGO = MakeGO("BottomBar", pad.transform);
        MakeImage(bottomGO, BottomBG, ws);
        var bLE2    = bottomGO.AddComponent<LayoutElement>(); bLE2.preferredHeight = 24;
        var bOL2    = bottomGO.AddComponent<Outline>();
        bOL2.effectColor = PanelBorder; bOL2.effectDistance = new Vector2(1,-1);
        var bHLG    = bottomGO.AddComponent<HorizontalLayoutGroup>();
        bHLG.padding = new RectOffset(6,6,3,3);
        bHLG.spacing = 8;
        bHLG.childForceExpandHeight = true;
        bHLG.childAlignment = TextAnchor.MiddleLeft;

        void AddResource(Transform parent, string label, string val)
        {
            var rGO = MakeGO("Res_"+label, parent);
            var rLE = rGO.AddComponent<LayoutElement>(); rLE.flexibleWidth = 1;
            MakeTMP(rGO, $"{label} {val}", 9, GoldText, TextAlignmentOptions.Left, true);
        }

        AddResource(bottomGO.transform, "$",  "4100");
        AddResource(bottomGO.transform, "♦", "3");
        AddResource(bottomGO.transform, "⚡", "88");

        // Close button
        var closeBtnGO = MakeGO("CloseBtn", bottomGO.transform);
        MakeImage(closeBtnGO, Hex("#c0392b"), ws);
        var cOL3 = closeBtnGO.AddComponent<Outline>();
        cOL3.effectColor = Hex("#7b1f14"); cOL3.effectDistance = new Vector2(1,-1);
        var cLE3 = closeBtnGO.AddComponent<LayoutElement>();
        cLE3.preferredWidth = 40; cLE3.preferredHeight = 18;
        closeBtnGO.AddComponent<Button>();
        var closeTxt = MakeGO("T", closeBtnGO.transform);
        SetAnchors(closeTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        MakeTMP(closeTxt, "X", 10, WhiteText, TextAlignmentOptions.Center, true);

        EditorUtility.SetDirty(panelGO);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[BuildSeedShopPanel] Done! Pixel-art seed shop panel built.");
    }
}
