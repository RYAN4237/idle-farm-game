using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixPanelLayout
{
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        var old = canvas.transform.Find("ExpandablePanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var panelGO = Make("ExpandablePanel", canvas.transform);
        var panelRT = panelGO.GetComponent<RectTransform>();
        panelRT.anchorMin       = new Vector2(1, 0.05f);
        panelRT.anchorMax       = new Vector2(1, 0.95f);
        panelRT.pivot           = new Vector2(1, 0.5f);
        panelRT.sizeDelta       = new Vector2(280, 0);
        panelRT.anchoredPosition= new Vector2(290, 0);
        SetImg(panelGO, Hex("#c8b89a"));

        var vlg = panelGO.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(5,5,5,5);
        vlg.spacing = 4;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;

        // Title
        var titleGO = Make("TitleBar", panelGO.transform);
        SetImg(titleGO, Hex("#5a9e2f"));
        SetLE(titleGO, prefH: 28);
        var titleTxt = MakeTxt("TitleText", titleGO.transform, "SEEDS SHOP", 12, Color.white, true);
        Stretch(titleTxt);

        // Middle
        var midGO = Make("Middle", panelGO.transform);
        SetImg(midGO, new Color(0,0,0,0));
        SetLE(midGO, flexH: true);
        var hlg = midGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;

        // Category bar
        var catGO = Make("CatBar", midGO.transform);
        SetImg(catGO, new Color(0,0,0,0));
        SetLE(catGO, prefW: 28);
        var cvlg = catGO.AddComponent<VerticalLayoutGroup>();
        cvlg.spacing = 3; cvlg.childForceExpandWidth = true; cvlg.childForceExpandHeight = false;
        cvlg.childAlignment = TextAnchor.UpperCenter;

        string[] catLabels = { "S", "F", "V", "*" };
        Color[]  catColors = { Hex("#5a9e2f"), Hex("#d4a843"), Hex("#d4a843"), Hex("#d4a843") };
        for (int ci = 0; ci < catLabels.Length; ci++)
        {
            var btn = Make("Cat_"+catLabels[ci], catGO.transform);
            SetImg(btn, catColors[ci]);
            SetLE(btn, prefW: 26, prefH: 26);
            btn.AddComponent<Button>();
            var bt = MakeTxt("T", btn.transform, catLabels[ci], 10, Hex("#1a3a00"), true);
            Stretch(bt);
        }

        // Grid wrap
        var gridWrapGO = Make("GridWrap", midGO.transform);
        SetImg(gridWrapGO, new Color(0,0,0,0));
        SetLE(gridWrapGO, flexW: true);
        var gvlg = gridWrapGO.AddComponent<VerticalLayoutGroup>();
        gvlg.spacing = 3; gvlg.childForceExpandWidth = true; gvlg.childForceExpandHeight = false;

        // Seed grid
        var gridGO = Make("SeedGrid", gridWrapGO.transform);
        SetImg(gridGO, new Color(0,0,0,0));
        SetLE(gridGO, flexH: true);
        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(76, 74); glg.spacing = new Vector2(3, 3);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3; glg.childAlignment = TextAnchor.UpperLeft;
        glg.padding = new RectOffset(1,1,1,1);

        // Seeds
        var seeds = new (string n, string a, int cost, int count, bool locked)[]
        {
            ("Wheat","W",10,99,false),("Corn","C",20,12,false),("Carrot","Ca",15,8,false),
            ("Tomato","T",25,5,false),("Potato","P",18,3,false),("Pumpkin","Pu",40,2,false),
            ("Strawb.","S2",50,1,false),("Waterml","W2",80,0,false),("Sunflwr","Su",60,99,false),
            ("Rose","R",120,4,false),("Mushroom","M",200,0,true),("Dragon","D",500,0,true),
        };

        foreach (var (name, abbr, cost, count, locked) in seeds)
        {
            var cell = Make("Cell_"+name, gridGO.transform);
            SetImg(cell, locked ? Hex("#a09080") : Hex("#ddd0b8"));
            var cOL = cell.AddComponent<Outline>();
            cOL.effectColor = Hex("#8b7355"); cOL.effectDistance = new Vector2(1.5f,-1.5f);

            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName = name; scb.seedCost = cost; scb.isLocked = locked;

            // Badge
            var badge = Make("Badge", cell.transform);
            var brt = badge.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1,1); brt.anchorMax = new Vector2(1,1);
            brt.pivot = new Vector2(1,1); brt.sizeDelta = new Vector2(24,14);
            brt.anchoredPosition = new Vector2(-2,-2);
            SetImg(badge, locked ? Hex("#888070") : Hex("#5a9e2f"));
            var bt2 = MakeTxt("T", badge.transform, count.ToString(), 8, Color.white, true);
            Stretch(bt2);

            // Icon text
            var iconGO = Make("Icon", cell.transform);
            var irt = iconGO.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.05f,0.38f); irt.anchorMax = new Vector2(0.95f,0.82f);
            irt.offsetMin = irt.offsetMax = Vector2.zero;
            var iconTxt = MakeTxt("T", iconGO.transform, abbr, locked?11f:16f,
                locked ? Hex("#888070") : Hex("#2a6010"), true);
            Stretch(iconTxt);

            // Name
            var nameGO = Make("Name", cell.transform);
            var nrt = nameGO.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0,0.20f); nrt.anchorMax = new Vector2(1,0.42f);
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            var nameTxt = MakeTxt("T", nameGO.transform, name, 7,
                locked ? Hex("#888070") : Hex("#3a2810"), true);
            Stretch(nameTxt);

            // Cost
            var costGO = Make("Cost", cell.transform);
            var costRT = costGO.GetComponent<RectTransform>();
            costRT.anchorMin = new Vector2(0,0.01f); costRT.anchorMax = new Vector2(1,0.22f);
            costRT.offsetMin = costRT.offsetMax = Vector2.zero;
            var costTxt = MakeTxt("T", costGO.transform, $"${cost}", 7,
                locked ? Hex("#888070") : Hex("#8b5c14"), false);
            Stretch(costTxt);

            // Lock overlay
            if (locked)
            {
                var lockGO = Make("Lock", cell.transform);
                var lrt = lockGO.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                SetImg(lockGO, new Color(0,0,0,0.25f));
                var lockTxt = MakeTxt("T", lockGO.transform, "LOCK", 9, Color.white, true);
                Stretch(lockTxt);
            }
        }

        // Info bar
        var infoGO = Make("InfoBar", gridWrapGO.transform);
        SetImg(infoGO, Hex("#4a8020"));
        SetLE(infoGO, prefH: 20);
        var infoTxt = MakeTxt("InfoTxt", infoGO.transform,
            "Select a seed  •  click grid to plant", 8, Hex("#d8f8a0"), false);
        Stretch(infoTxt);

        // Bottom bar
        var btmGO = Make("BottomBar", panelGO.transform);
        SetImg(btmGO, Hex("#6b4c2a"));
        SetLE(btmGO, prefH: 26);
        var bhlg = btmGO.AddComponent<HorizontalLayoutGroup>();
        bhlg.padding = new RectOffset(6,4,3,3); bhlg.spacing = 6;
        bhlg.childForceExpandHeight = true; bhlg.childAlignment = TextAnchor.MiddleLeft;

        void Res(string lbl, string val)
        {
            var rGO = Make("Res_"+lbl, btmGO.transform);
            SetLE(rGO, flexW: true);
            var rt2 = MakeTxt("T", rGO.transform, lbl+val, 9, Hex("#ffe060"), true);
            Stretch(rt2);
        }
        Res("$ ","4100"); Res("♦ ","3"); Res("⚡ ","88");

        var clsGO = Make("CloseBtn", btmGO.transform);
        SetImg(clsGO, Hex("#b02020"));
        SetLE(clsGO, prefW: 34, prefH: 20);
        clsGO.AddComponent<Button>();
        var clsTxt = MakeTxt("T", clsGO.transform, "X", 11, Color.white, true);
        Stretch(clsTxt);

        // Wire UIManager
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        uiMgr.expandablePanel = panelRT;
        uiMgr.panelTitle = titleTxt.GetComponent<TextMeshProUGUI>();
        uiMgr.closeButton = clsGO.GetComponent<Button>();
        var ib = canvas.transform.Find("RightIconBar");
        if (ib != null)
        {
            var sb = ib.Find("SeedButton");   if (sb)  uiMgr.seedButton    = sb.GetComponent<Button>();
            var bb = ib.Find("BuildButton");  if (bb)  uiMgr.buildButton   = bb.GetComponent<Button>();
            var ub = ib.Find("UpgradeButton");if (ub)  uiMgr.upgradeButton = ub.GetComponent<Button>();
        }
        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixPanelLayout] Done!");
    }

    static GameObject Make(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }
    static void SetImg(GameObject go, Color c)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>(); img.color = c;
    }
    static void SetLE(GameObject go, float prefW=0, float prefH=0, bool flexH=false, bool flexW=false)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        if (prefW > 0) le.preferredWidth  = prefW;
        if (prefH > 0) le.preferredHeight = prefH;
        if (flexH) le.flexibleHeight = 1;
        if (flexW) le.flexibleWidth  = 1;
    }
    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
    static void Stretch(Component c) => Stretch(c.gameObject);
    static GameObject MakeTxt(string name, Transform parent, string text, float size, Color col, bool bold)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = col;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.enableWordWrapping = false; tmp.overflowMode = TextOverflowModes.Truncate;
        return go;
    }
}
