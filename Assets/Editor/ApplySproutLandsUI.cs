using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ApplySproutLandsUI
{
    static readonly string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    static Sprite GetSprite(string sheet, int row, int col)
    {
        string key = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + row + "_" + col;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (obj is Sprite s && s.name == key) return s;
        Debug.LogWarning("Sprite not found: " + key);
        return null;
    }

    static GameObject GO(string n, Transform p)
    { var g = new GameObject(n); g.transform.SetParent(p, false); g.AddComponent<RectTransform>(); return g; }

    static void Bg(GameObject g, Color c)
    { var i = g.GetComponent<Image>() ?? g.AddComponent<Image>(); i.color = c; }

    static void RT(GameObject g, float ax, float ay, float bx, float by,
                   float l, float b, float r, float t)
    {
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay); rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(l, b);   rt.offsetMax  = new Vector2(r, t);
    }

    static void Fill(GameObject g)
    { var rt = g.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; }
    static void Fill(Component c) => Fill(c.gameObject);

    static TextMeshProUGUI Label(string n, Transform p, string text, float sz, Color c, bool bold = false)
    {
        var g  = GO(n, p);
        var tm = g.AddComponent<TextMeshProUGUI>();
        tm.text = text; tm.fontSize = sz; tm.color = c;
        tm.alignment = TextAlignmentOptions.Center;
        tm.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tm.enableWordWrapping = false;
        tm.overflowMode = TextOverflowModes.Truncate;
        Fill(g);
        return tm;
    }

    static Image SpriteImg(string n, Transform p, Sprite spr)
    {
        var g  = GO(n, p);
        var im = g.AddComponent<Image>();
        im.sprite = spr; im.preserveAspect = true;
        return im;
    }

    // ─────────────────────────────────────────────────────────────────
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }
        BuildIconBar(canvas);
        BuildPanel(canvas);
        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[ApplySproutLandsUI] Done!");
    }

    // ── RightIconBar ─────────────────────────────────────────────────
    static void BuildIconBar(GameObject canvas)
    {
        var bar = canvas.transform.Find("RightIconBar")?.gameObject;
        if (bar == null) { Debug.LogWarning("RightIconBar not found"); return; }

        for (int i = bar.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(bar.transform.GetChild(i).gameObject);

        // Slim bar: 46px wide, centered vertically
        var bRT = bar.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(1, 0.5f); bRT.anchorMax = new Vector2(1, 0.5f);
        bRT.pivot = new Vector2(1, 0.5f);
        bRT.sizeDelta = new Vector2(46, 160);
        bRT.anchoredPosition = Vector2.zero;
        Bg(bar, Hex("#5a3a18"));

        // 3 icon buttons stacked vertically
        var defs = new (string name, string sheet, int r, int c, Color bg)[]
        {
            ("SeedButton",    "Basic Plants.png",               0, 0, Hex("#3a7a10")),
            ("BuildButton",   "Basic tools and meterials.png",  0, 2, Hex("#7a5010")),
            ("UpgradeButton", "Basic Grass Biom things 1.png",  0, 3, Hex("#104870")),
        };

        float sz = 42f, gap = 3f;
        float totalH = defs.Length * sz + (defs.Length - 1) * gap;
        float startY = totalH / 2f;

        for (int i = 0; i < defs.Length; i++)
        {
            var (name, sheet, r, c, bg) = defs[i];
            var btn = GO(name, bar.transform);
            var brt = btn.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 0.5f); brt.anchorMax = new Vector2(1, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(0, sz);
            brt.anchoredPosition = new Vector2(0, startY - i * (sz + gap) - sz * 0.5f);
            Bg(btn, bg);

            var button = btn.AddComponent<Button>();
            var cs = button.colors;
            cs.highlightedColor = new Color(1, 1, 0.6f); button.colors = cs;

            // Add outline
            var ol = btn.AddComponent<Outline>();
            ol.effectColor = Hex("#2a1a08"); ol.effectDistance = new Vector2(1, -1);

            // Sprite icon centered
            var spr = GetSprite(sheet, r, c);
            if (spr != null)
            {
                var ico = GO("Icon", btn.transform);
                RT(ico, 0.1f, 0.1f, 0.9f, 0.9f, 2, 2, -2, -2);
                var ii = ico.AddComponent<Image>();
                ii.sprite = spr; ii.preserveAspect = true;
            }
            EditorUtility.SetDirty(btn);
        }
        EditorUtility.SetDirty(bar);
        Debug.Log("IconBar done");
    }

    // ── Panel ────────────────────────────────────────────────────────
    static void BuildPanel(GameObject canvas)
    {
        var old = canvas.transform.Find("ExpandablePanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var panel = GO("ExpandablePanel", canvas.transform);
        var pRT   = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(1, 0.08f); pRT.anchorMax = new Vector2(1, 0.92f);
        pRT.pivot = new Vector2(1, 0.5f);
        pRT.sizeDelta = new Vector2(215, 0);
        pRT.anchoredPosition = new Vector2(220, 0); // hidden
        Bg(panel, Hex("#c8b89a"));
        var polOL = panel.AddComponent<Outline>();
        polOL.effectColor = Hex("#5a3a18"); polOL.effectDistance = new Vector2(2, -2);

        // Title strip (top 26px)
        var title = GO("TitleBar", panel.transform);
        RT(title, 0, 1, 1, 1, 4, -26, -4, -2);
        Bg(title, Hex("#3a7a10"));
        Label("T", title.transform, "SEEDS SHOP", 10, Color.white, true);

        // Bottom bar (bottom 26px)
        var btm = GO("BottomBar", panel.transform);
        RT(btm, 0, 0, 1, 0, 4, 2, -4, 26);
        Bg(btm, Hex("#5a3a18"));

        // Resources
        var res1 = GO("R1", btm.transform); RT(res1, 0f, 0, 0.38f, 1, 4, 0, 0, 0);
        Label("T", res1.transform, "$ 4100", 8, Hex("#ffe060"), true);
        var res2 = GO("R2", btm.transform); RT(res2, 0.38f, 0, 0.64f, 1, 0, 0, 0, 0);
        Label("T", res2.transform, "♦ 3", 8, Hex("#ffe060"), true);
        var res3 = GO("R3", btm.transform); RT(res3, 0.64f, 0, 0.82f, 1, 0, 0, 0, 0);
        Label("T", res3.transform, "⚡ 88", 8, Hex("#ffe060"), true);

        var cls = GO("CloseBtn", btm.transform);
        RT(cls, 0.82f, 0.1f, 1f, 0.9f, 2, 2, -2, -2);
        Bg(cls, Hex("#a01818")); cls.AddComponent<Button>();
        Label("T", cls.transform, "X", 10, Color.white, true);

        // Category bar (left 32px, between title and bottom)
        var cat = GO("CatBar", panel.transform);
        RT(cat, 0, 0, 0, 1, 4, 28, 36, -28);
        Bg(cat, new Color(0, 0, 0, 0));

        var catDefs = new (string sheet, int r, int c, Color bg)[]
        {
            ("Basic Plants.png",               0, 0, Hex("#3a7a10")),
            ("Basic tools and meterials.png",  0, 2, Hex("#7a5010")),
            ("Basic Grass Biom things 1.png",  0, 3, Hex("#104870")),
            ("Basic Furniture.png",            0, 0, Hex("#604030")),
        };
        for (int i = 0; i < catDefs.Length; i++)
        {
            var (sh, r, c, bg) = catDefs[i];
            var b = GO("Cat" + i, cat.transform);
            float yMax = 1f - i * 0.26f, yMin = yMax - 0.23f;
            RT(b, 0, yMin, 1, yMax, 1, 1, -1, -1);
            Bg(b, bg); b.AddComponent<Button>();
            var ol2 = b.AddComponent<Outline>();
            ol2.effectColor = Hex("#2a1a08"); ol2.effectDistance = new Vector2(1, -1);
            var spr = GetSprite(sh, r, c);
            if (spr != null)
            {
                var ico = GO("Ico", b.transform);
                RT(ico, 0.05f, 0.05f, 0.95f, 0.95f, 2, 2, -2, -2);
                var ii = ico.AddComponent<Image>(); ii.sprite = spr; ii.preserveAspect = true;
            }
        }

        // Grid area (right of catbar, between title and bottom)
        var gArea = GO("GridArea", panel.transform);
        RT(gArea, 0, 0, 1, 1, 38, 28, -4, -28);
        Bg(gArea, new Color(0, 0, 0, 0));

        // Info bar (bottom 18px of grid area)
        var info = GO("InfoBar", gArea.transform);
        RT(info, 0, 0, 1, 0, 0, 0, 0, 18);
        Bg(info, Hex("#2a5c08"));
        Label("InfoTxt", info.transform, "Select a seed · click to plant", 7, Hex("#b8f880"), false);

        // Seed grid (rest of grid area)
        var grid = GO("SeedGrid", gArea.transform);
        RT(grid, 0, 0, 1, 1, 1, 18, -1, 0);
        Bg(grid, new Color(0, 0, 0, 0));
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(52, 54);
        glg.spacing  = new Vector2(3, 3);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.childAlignment = TextAnchor.UpperLeft;
        glg.padding = new RectOffset(2, 2, 2, 2);

        // Seed data + sprite mapping
        var plantIcons = new (string sh, int r, int c)[]
        {
            ("Basic Plants.png",0,0),("Basic Plants.png",0,1),("Basic Plants.png",0,2),
            ("Basic Plants.png",0,3),("Basic Plants.png",0,4),("Basic Plants.png",0,5),
            ("Basic Plants.png",1,0),("Basic Plants.png",1,1),("Basic Plants.png",1,2),
            ("Basic Plants.png",1,3),("Basic Plants.png",1,4),("Basic Plants.png",1,5),
        };
        var seeds = new (string n, int cost, int cnt, bool lk)[]
        {
            ("Wheat",  10, 99, false), ("Carrot", 15, 8,  false), ("Beet",  20, 5,  false),
            ("Turnip", 18,  3, false), ("Pumpkin",40, 2,  false), ("Corn",  30, 4,  false),
            ("Wheat+", 50,  1, false), ("Carrot+",60, 0,  false), ("Beet+", 80, 99, false),
            ("Turnip+",120, 4, false), ("Shroom", 200,0,  true),  ("Dragon",500, 0, true),
        };

        for (int i = 0; i < seeds.Length; i++)
        {
            var (name, cost, cnt, lk) = seeds[i];
            var cell = GO("Cell_" + name, grid.transform);
            Bg(cell, lk ? Hex("#908070") : Hex("#e0cfa8"));
            var cOL = cell.AddComponent<Outline>();
            cOL.effectColor = Hex("#8b7040"); cOL.effectDistance = new Vector2(1, -1);
            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName = name; scb.seedCost = cost; scb.isLocked = lk;

            // Count badge
            var bdg = GO("Badge", cell.transform);
            var brt = bdg.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(1, 1); brt.anchorMax = new Vector2(1, 1);
            brt.pivot = new Vector2(1, 1); brt.sizeDelta = new Vector2(16, 11);
            brt.anchoredPosition = new Vector2(-1, -1);
            Bg(bdg, lk ? Hex("#706050") : Hex("#3a8010"));
            Label("T", bdg.transform, cnt.ToString(), 7, Color.white, true);

            // Sprite icon
            if (i < plantIcons.Length && !lk)
            {
                var spr = GetSprite(plantIcons[i].sh, plantIcons[i].r, plantIcons[i].c);
                if (spr != null)
                {
                    var ico = GO("Ico", cell.transform);
                    var irt = ico.GetComponent<RectTransform>();
                    irt.anchorMin = new Vector2(0.08f, 0.35f);
                    irt.anchorMax = new Vector2(0.92f, 0.82f);
                    irt.offsetMin = irt.offsetMax = Vector2.zero;
                    var ii = ico.AddComponent<Image>();
                    ii.sprite = spr; ii.preserveAspect = true;
                }
            }
            else if (lk)
            {
                var lockGO = GO("Lock", cell.transform);
                var lrt = lockGO.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
                Bg(lockGO, new Color(0, 0, 0, 0.3f));
                Label("T", lockGO.transform, "LOCK", 8, Color.white, true);
            }

            // Name
            var nm = GO("Name", cell.transform);
            var nrt = nm.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 0.20f); nrt.anchorMax = new Vector2(1, 0.40f);
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            Label("T", nm.transform, name, 6, lk ? Hex("#807060") : Hex("#3a2808"), true);

            // Cost
            var cs2 = GO("Cost", cell.transform);
            var crt = cs2.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0.02f); crt.anchorMax = new Vector2(1, 0.22f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            Label("T", cs2.transform, "$" + cost, 6, lk ? Hex("#807060") : Hex("#7a4c10"), false);
        }

        // Wire UIManager
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        uiMgr.expandablePanel = pRT;
        uiMgr.closeButton = cls.GetComponent<Button>();
        var ib = canvas.transform.Find("RightIconBar");
        if (ib != null)
        {
            var sb = ib.Find("SeedButton");     if (sb) uiMgr.seedButton    = sb.GetComponent<Button>();
            var bb = ib.Find("BuildButton");    if (bb) uiMgr.buildButton   = bb.GetComponent<Button>();
            var ub = ib.Find("UpgradeButton");  if (ub) uiMgr.upgradeButton = ub.GetComponent<Button>();
        }
        Debug.Log("Panel done");
    }
}
