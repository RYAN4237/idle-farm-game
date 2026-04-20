using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Final clean rebuild - no LayoutGroups on CatBar, manual RectTransform positioning
public class RebuildPanelFinal
{
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Remove old panel
        var old = canvas.transform.Find("ExpandablePanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // ── Root panel ───────────────────────────────────────────────
        var panel = NewGO("ExpandablePanel", canvas.transform);
        var pRT   = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(1,0.05f); pRT.anchorMax = new Vector2(1,0.95f);
        pRT.pivot     = new Vector2(1,0.5f);
        pRT.sizeDelta = new Vector2(280,0);
        pRT.anchoredPosition = new Vector2(290,0);   // hidden
        Img(panel, Hex("#c8b89a"));

        // ── Title (top 32px) ──────────────────────────────────────────
        var title = NewGO("TitleBar", panel.transform);
        SetRT(title, 0,1, 1,1, 0,-32, 0,0);   // top strip 32px
        Img(title, Hex("#5a9e2f"));
        var titleTxt = Txt("TitleText", title.transform, "SEEDS SHOP", 12, Color.white, true);
        Fill(titleTxt);

        // ── Bottom bar (bottom 30px) ───────────────────────────────────
        var btm = NewGO("BottomBar", panel.transform);
        SetRT(btm, 0,0, 1,0, 0,0, 0,30);    // bottom strip 30px
        Img(btm, Hex("#6b4c2a"));

        // Resources in bottom
        var r1 = Txt("Res1", btm.transform, "$ 4100", 9, Hex("#ffe060"), true);
        SetRT(r1, 0,0, 0.33f,1, 4,4, 0,-4);
        var r2 = Txt("Res2", btm.transform, "♦ 3",    9, Hex("#ffe060"), true);
        SetRT(r2, 0.33f,0, 0.66f,1, 0,4, 0,-4);
        var r3 = Txt("Res3", btm.transform, "⚡ 88",  9, Hex("#ffe060"), true);
        SetRT(r3, 0.66f,0, 0.85f,1, 0,4, 0,-4);

        var cls = NewGO("CloseBtn", btm.transform);
        SetRT(cls, 0.85f,0.1f, 1f,0.9f, 2,2, -2,-2);
        Img(cls, Hex("#b02020"));
        cls.AddComponent<Button>();
        var clsTxt = Txt("T", cls.transform, "X", 11, Color.white, true);
        Fill(clsTxt);

        // ── Cat bar (left 32px, between title and bottom) ─────────────
        var cat = NewGO("CatBar", panel.transform);
        SetRT(cat, 0,0, 0,1, 5,32, 37,-32);  // left strip, inside padding
        Img(cat, new Color(0,0,0,0));

        string[] catL = {"S","F","V","*"};
        Color[]  catC = {Hex("#5a9e2f"), Hex("#d4a843"), Hex("#d4a843"), Hex("#d4a843")};
        for (int i = 0; i < 4; i++)
        {
            float yMax = 1f - i * 0.25f;
            float yMin = yMax - 0.22f;
            var b = NewGO("Cat_"+catL[i], cat.transform);
            SetRT(b, 0,yMin, 1,yMax, 2,2, -2,-2);
            Img(b, catC[i]);
            b.AddComponent<Button>();
            var bT = Txt("T", b.transform, catL[i], 10, Hex("#1a3a00"), true);
            Fill(bT);
        }

        // ── Grid area (right of catbar, between title and info+bottom) ─
        var gridArea = NewGO("GridArea", panel.transform);
        SetRT(gridArea, 0,0, 1,1, 42,30, -5,-32); // right of catbar, above bottom
        Img(gridArea, new Color(0,0,0,0));

        // Info bar (bottom 22px of grid area)
        var info = NewGO("InfoBar", gridArea.transform);
        SetRT(info, 0,0, 1,0, 0,0, 0,22);
        Img(info, Hex("#4a8020"));
        var infoTxt = Txt("InfoTxt", info.transform,
            "Select a seed  •  click grid to plant", 8, Hex("#d8f8a0"), false);
        Fill(infoTxt);

        // Seed grid (rest of grid area above info)
        var grid = NewGO("SeedGrid", gridArea.transform);
        SetRT(grid, 0,0, 1,1, 0,22, 0,0);
        Img(grid, new Color(0,0,0,0));
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(74,72);
        glg.spacing         = new Vector2(3,3);
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.childAlignment  = TextAnchor.UpperLeft;
        glg.padding         = new RectOffset(2,2,2,2);

        // Seeds
        var seeds = new (string n, string a, int cost, int cnt, bool lk)[]
        {
            ("Wheat","W",10,99,false),("Corn","C",20,12,false),("Carrot","Ca",15,8,false),
            ("Tomato","T",25,5,false),("Potato","P",18,3,false),("Pumpkin","Pu",40,2,false),
            ("Strawb.","S2",50,1,false),("Waterml","W2",80,0,false),("Sunflwr","Su",60,99,false),
            ("Rose","R",120,4,false),("Mushroom","M",200,0,true),("Dragon","D",500,0,true),
        };

        foreach (var (name, abbr, cost, cnt, lk) in seeds)
        {
            var cell = NewGO("Cell_"+name, grid.transform);
            Img(cell, lk ? Hex("#a09080") : Hex("#ddd0b8"));
            var ol = cell.AddComponent<Outline>();
            ol.effectColor = Hex("#8b7355"); ol.effectDistance = new Vector2(1.5f,-1.5f);
            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName = name; scb.seedCost = cost; scb.isLocked = lk;

            // Badge top-right
            var bdg = NewGO("Badge", cell.transform);
            var brt = bdg.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1,1); brt.anchorMax = new Vector2(1,1);
            brt.pivot = new Vector2(1,1); brt.sizeDelta = new Vector2(22,13);
            brt.anchoredPosition = new Vector2(-1,-1);
            Img(bdg, lk ? Hex("#888070") : Hex("#5a9e2f"));
            Fill(Txt("T", bdg.transform, cnt.ToString(), 8, Color.white, true));

            // Icon (center-upper)
            var ico = NewGO("Icon", cell.transform);
            var irt = ico.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.05f,0.40f); irt.anchorMax = new Vector2(0.95f,0.84f);
            irt.offsetMin = irt.offsetMax = Vector2.zero;
            Fill(Txt("T", ico.transform, abbr, lk?10f:16f,
                lk ? Hex("#888070") : Hex("#2a6010"), true));

            // Name
            var nm = NewGO("Name", cell.transform);
            var nrt = nm.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0,0.21f); nrt.anchorMax = new Vector2(1,0.42f);
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            Fill(Txt("T", nm.transform, name, 7,
                lk ? Hex("#888070") : Hex("#3a2810"), true));

            // Cost
            var cs2 = NewGO("Cost", cell.transform);
            var crt = cs2.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0,0.02f); crt.anchorMax = new Vector2(1,0.23f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            Fill(Txt("T", cs2.transform, $"${cost}", 7,
                lk ? Hex("#888070") : Hex("#8b5c14"), false));

            if (lk)
            {
                var lkGO = NewGO("Lock", cell.transform);
                var lrt = lkGO.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                Img(lkGO, new Color(0,0,0,0.25f));
                Fill(Txt("T", lkGO.transform, "LOCK", 9, Color.white, true));
            }
        }

        // ── Wire UIManager ────────────────────────────────────────────
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        uiMgr.expandablePanel = pRT;
        uiMgr.panelTitle      = titleTxt.GetComponent<TextMeshProUGUI>();
        uiMgr.closeButton     = cls.GetComponent<Button>();
        var ib = canvas.transform.Find("RightIconBar");
        if (ib != null)
        {
            var sb = ib.Find("SeedButton");    if (sb)  uiMgr.seedButton    = sb.GetComponent<Button>();
            var bb = ib.Find("BuildButton");   if (bb)  uiMgr.buildButton   = bb.GetComponent<Button>();
            var ub = ib.Find("UpgradeButton"); if (ub)  uiMgr.upgradeButton = ub.GetComponent<Button>();
        }
        EditorUtility.SetDirty(canvas);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[RebuildPanelFinal] Done - manual RectTransform layout, no LayoutGroup issues.");
    }

    // ── helpers ──────────────────────────────────────────────────────
    static GameObject NewGO(string n, Transform p)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        go.AddComponent<RectTransform>(); return go;
    }
    static void Img(GameObject go, Color c)
    { var i = go.GetComponent<Image>() ?? go.AddComponent<Image>(); i.color = c; }

    /// anchorMin=(ax,ay), anchorMax=(bx,by), offsetMin=(ox0,oy0), offsetMax=(ox1,oy1)
    static void SetRT(GameObject go,
        float ax, float ay, float bx, float by,
        float ox0, float oy0, float ox1, float oy1)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax,ay); rt.anchorMax = new Vector2(bx,by);
        rt.offsetMin = new Vector2(ox0,oy0); rt.offsetMax = new Vector2(ox1,oy1);
    }
    static void Fill(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
    static GameObject Txt(string n, Transform p, string text, float sz, Color c, bool bold)
    {
        var go = new GameObject(n); go.transform.SetParent(p, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = sz; t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.enableWordWrapping = false; t.overflowMode = TextOverflowModes.Truncate;
        return go;
    }
}
