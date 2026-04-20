using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Nuclear rebuild: clear everything on UICanvas and rebuild from scratch
/// with absolutely correct RectTransform values for 1280x720 canvas.
public class NuclearRebuildUI
{
    static readonly string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";
    static Color C(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    static Sprite Spr(string sheet, int r, int col)
    {
        string k = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + r + "_" + col;
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (o is Sprite s && s.name == k) return s;
        return null;
    }

    // Create GO with RectTransform, parented
    static GameObject G(string n, Transform p)
    {
        var g = new GameObject(n);
        g.transform.SetParent(p, false);
        g.AddComponent<RectTransform>();
        return g;
    }

    // Set rect using anchor + offset (all in canvas units)
    // ax,ay = anchorMin  bx,by = anchorMax
    // ol,ob = offsetMin  or,ot = offsetMax
    static RectTransform R(GameObject g,
        float ax, float ay, float bx, float by,
        float ol, float ob, float or2, float ot)
    {
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay);
        rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(ol, ob);
        rt.offsetMax = new Vector2(or2, ot);
        return rt;
    }

    static Image Img(GameObject g, Color c)
    {
        var i = g.GetComponent<Image>() ?? g.AddComponent<Image>();
        i.color = c; return i;
    }

    static TextMeshProUGUI Txt(string n, Transform p, string text,
                                float sz, Color c, bool bold = false)
    {
        var g = G(n, p);
        R(g, 0, 0, 1, 1, 0, 0, 0, 0);
        var t = g.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = sz; t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Truncate;
        return t;
    }

    // ─────────────────────────────────────────────────────────────────
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // Fix CanvasScaler: 1280x720, match height
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 1f;

        // Nuke everything except EventSystem-related
        var keep = new[] { "EventSystem" };
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            var ch = canvas.transform.GetChild(i).gameObject;
            bool skip = System.Array.Exists(keep, k => ch.name == k);
            if (!skip) Object.DestroyImmediate(ch);
        }

        BuildTopBar(canvas);
        BuildIconBar(canvas);
        BuildPanel(canvas);

        // Wire UIManager
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        uiMgr.expandablePanel = canvas.transform.Find("ExpandablePanel").GetComponent<RectTransform>();
        uiMgr.closeButton     = canvas.transform.Find("ExpandablePanel/BottomBar/CloseBtn").GetComponent<Button>();
        uiMgr.seedButton      = canvas.transform.Find("RightIconBar/SeedButton").GetComponent<Button>();
        uiMgr.buildButton     = canvas.transform.Find("RightIconBar/BuildButton").GetComponent<Button>();
        uiMgr.upgradeButton   = canvas.transform.Find("RightIconBar/UpgradeButton").GetComponent<Button>();
        EditorUtility.SetDirty(canvas);

        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[NuclearRebuildUI] Done!");
    }

    // ── TopBar: full-width strip at top, 32px tall ────────────────────
    static void BuildTopBar(GameObject canvas)
    {
        var bar = G("TopBar", canvas.transform);
        R(bar, 0,1, 1,1, 0,-32, 0,0);
        Img(bar, C("#1a2a10"));
        var ol = bar.AddComponent<Outline>();
        ol.effectColor = C("#3a5a10"); ol.effectDistance = new Vector2(0,-1);

        var label = G("FPDisplay", bar.transform);
        R(label, 0,0, 0.5f,1, 8,0,0,0);
        Txt("Text", label.transform, "Focus Points:", 12, C("#c8e890"), true);

        var val = G("Value", bar.transform);
        R(val, 0.15f,0, 0.35f,1, 0,0,0,0);
        Txt("Text", val.transform, "0", 13, Color.white, true);
    }

    // ── RightIconBar: full-height strip on right, 52px wide ───────────
    static void BuildIconBar(GameObject canvas)
    {
        var bar = G("RightIconBar", canvas.transform);
        // Stretch full height, right-anchored, 52px wide, starts below TopBar
        R(bar, 1,0, 1,1, -52,32, 0,0);
        Img(bar, C("#1e1008"));

        var defs = new (string name, string sheet, int r, int c, Color bg)[]
        {
            ("SeedButton",    "Basic Plants.png",              0, 0, C("#1e5008")),
            ("BuildButton",   "Basic tools and meterials.png", 0, 2, C("#503010")),
            ("UpgradeButton", "Basic Grass Biom things 1.png", 0, 3, C("#102038")),
        };

        for (int i = 0; i < defs.Length; i++)
        {
            var (name, sheet, r, c, bg) = defs[i];
            var btn = G(name, bar.transform);
            // Each button: 44px tall, full width, anchored to top
            var bRT = btn.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0, 1);
            bRT.anchorMax = new Vector2(1, 1);
            bRT.pivot     = new Vector2(0.5f, 1);
            bRT.sizeDelta = new Vector2(0, 44);
            bRT.anchoredPosition = new Vector2(0, -(6 + i * 48f));

            var bImg = Img(btn, bg);
            var button = btn.AddComponent<Button>();
            button.targetGraphic = bImg;
            var cs = button.colors;
            cs.highlightedColor = new Color(1,1,0.7f); button.colors = cs;

            var bOL = btn.AddComponent<Outline>();
            bOL.effectColor = C("#080402"); bOL.effectDistance = new Vector2(1,-1);

            var spr = Spr(sheet, r, c);
            if (spr != null)
            {
                var ico = G("Icon", btn.transform);
                R(ico, 0.1f,0.1f, 0.9f,0.9f, 2,2,-2,-2);
                var ii = ico.AddComponent<Image>();
                ii.sprite = spr; ii.preserveAspect = true; ii.raycastTarget = false;
            }

            Debug.Log($"  {name}: sprite={(Spr(sheet,r,c)?.name ?? "NULL")}");
        }
    }

    // ── ExpandablePanel: full-height, 230px wide, slides from right ───
    static void BuildPanel(GameObject canvas)
    {
        var panel = G("ExpandablePanel", canvas.transform);
        // Anchored right, full height below topbar, 230px wide
        R(panel, 1,0, 1,1, -230,32, 0,0);
        var pRT = panel.GetComponent<RectTransform>();
        pRT.anchoredPosition = new Vector2(235, 0); // hidden off screen
        Img(panel, C("#c0aa80"));
        var polOL = panel.AddComponent<Outline>();
        polOL.effectColor = C("#5a3a18"); polOL.effectDistance = new Vector2(2,-2);

        // Title: top 26px
        var title = G("TitleBar", panel.transform);
        R(title, 0,1, 1,1, 0,-26, 0,0);
        Img(title, C("#2e6e10"));
        Txt("TitleText", title.transform, "SEEDS SHOP", 11, Color.white, true);

        // Bottom bar: bottom 26px (always visible, contains close button)
        var btm = G("BottomBar", panel.transform);
        R(btm, 0,0, 1,0, 0,0, 0,26);
        Img(btm, C("#4a2e10"));

        var r1 = G("R1", btm.transform); R(r1,0f,0,0.36f,1,4,0,0,0);
        Txt("T",r1.transform,"$ 4100",8,C("#ffe060"),true);
        var r2 = G("R2", btm.transform); R(r2,0.36f,0,0.62f,1,0,0,0,0);
        Txt("T",r2.transform,"♦ 3",8,C("#ffe060"),true);
        var r3 = G("R3", btm.transform); R(r3,0.62f,0,0.82f,1,0,0,0,0);
        Txt("T",r3.transform,"FP 0",8,C("#a0f060"),true);

        var cls = G("CloseBtn", btm.transform);
        R(cls, 0.82f,0.08f, 1f,0.92f, 2,0,-2,0);
        Img(cls, C("#901010")); cls.AddComponent<Button>();
        var clsOL = cls.AddComponent<Outline>();
        clsOL.effectColor = C("#400808"); clsOL.effectDistance = new Vector2(1,-1);
        Txt("T", cls.transform, "X", 11, Color.white, true);

        // Cat bar: left 34px between title and bottom
        var cat = G("CatBar", panel.transform);
        R(cat, 0,0, 0,1, 4,26, 38,-26);
        Img(cat, new Color(0,0,0,0));

        var catDefs = new (string sh, int r, int c, Color bg)[]
        {
            ("Basic Plants.png",              0,0, C("#1e5008")),
            ("Basic tools and meterials.png", 0,2, C("#503010")),
            ("Basic Grass Biom things 1.png", 0,3, C("#102038")),
            ("Basic Furniture.png",           0,0, C("#402818")),
        };
        for (int i = 0; i < catDefs.Length; i++)
        {
            var (sh,cr,cc,bg) = catDefs[i];
            var b = G("Cat"+i, cat.transform);
            float yMax = 1f - i*0.26f, yMin = yMax - 0.23f;
            R(b, 0,yMin, 1,yMax, 1,1,-1,-1);
            Img(b, bg); b.AddComponent<Button>();
            var spr = Spr(sh, cr, cc);
            if (spr != null)
            {
                var ico = G("Ico",b.transform);
                R(ico, 0.08f,0.08f, 0.92f,0.92f, 1,1,-1,-1);
                var ii = ico.AddComponent<Image>(); ii.sprite=spr; ii.preserveAspect=true;
            }
        }

        // ScrollView: right of catbar, between title and bottom
        var scrollGO = G("ScrollView", panel.transform);
        R(scrollGO, 0,0, 1,1, 38,26, 0,-26);
        Img(scrollGO, new Color(0,0,0,0));
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false; scroll.vertical = true;
        scroll.scrollSensitivity = 25f; scroll.movementType = ScrollRect.MovementType.Clamped;

        var vp = G("Viewport", scrollGO.transform);
        R(vp, 0,0, 1,1, 0,0, 0,0);
        Img(vp, new Color(0,0,0,0));
        vp.AddComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vp.GetComponent<RectTransform>();

        var content = G("Content", vp.transform);
        var cRT = content.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0,1); cRT.anchorMax = new Vector2(1,1);
        cRT.pivot = new Vector2(0.5f,1);
        cRT.sizeDelta = new Vector2(0, 0);
        Img(content, new Color(0,0,0,0));
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = cRT;

        // Vertical layout on content
        var cvlg = content.AddComponent<VerticalLayoutGroup>();
        cvlg.padding = new RectOffset(2,2,2,2); cvlg.spacing = 0;
        cvlg.childForceExpandWidth = true; cvlg.childForceExpandHeight = false;
        cvlg.childControlWidth = true; cvlg.childControlHeight = true;

        // Seed grid inside content
        var grid = G("SeedGrid", content.transform);
        Img(grid, new Color(0,0,0,0));
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(56,60); glg.spacing = new Vector2(3,3);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.childAlignment = TextAnchor.UpperLeft;
        glg.padding = new RectOffset(2,2,2,2);
        var gcsf = grid.AddComponent<ContentSizeFitter>();
        gcsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var gle = grid.AddComponent<LayoutElement>();
        gle.flexibleWidth = 1;

        // Seeds
        var plantIcons = new (string sh, int r, int c)[]
        {
            ("Basic Plants.png",0,0),("Basic Plants.png",0,1),("Basic Plants.png",0,2),
            ("Basic Plants.png",0,3),("Basic Plants.png",0,4),("Basic Plants.png",0,5),
            ("Basic Plants.png",1,0),("Basic Plants.png",1,1),("Basic Plants.png",1,2),
            ("Basic Plants.png",1,3),("Basic Plants.png",1,4),("Basic Plants.png",1,5),
        };
        var seeds = new (string n, int cost, int cnt, bool lk)[]
        {
            ("Wheat",10,99,false),("Carrot",15,8,false),("Beet",20,5,false),
            ("Turnip",18,3,false),("Pumpkin",40,2,false),("Corn",30,4,false),
            ("Wheat+",50,1,false),("Carrot+",60,0,false),("Beet+",80,99,false),
            ("Turnip+",120,4,false),("Shroom",200,0,true),("Dragon",500,0,true),
        };

        for (int i = 0; i < seeds.Length; i++)
        {
            var (n, cost, cnt, lk) = seeds[i];
            var cell = G("Cell_"+n, grid.transform);
            Img(cell, lk ? C("#807060") : C("#ddc898"));
            var ol2 = cell.AddComponent<Outline>();
            ol2.effectColor = C("#8a6830"); ol2.effectDistance = new Vector2(1,-1);
            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName = n; scb.seedCost = cost; scb.isLocked = lk;

            // Count badge top-right
            var bdg = G("Badge", cell.transform);
            var brt = bdg.GetComponent<RectTransform>();
            brt.anchorMin=new Vector2(1,1); brt.anchorMax=new Vector2(1,1);
            brt.pivot=new Vector2(1,1); brt.sizeDelta=new Vector2(16,11);
            brt.anchoredPosition=new Vector2(-1,-1);
            Img(bdg, lk?C("#605040"):C("#287010"));
            Txt("T",bdg.transform,cnt.ToString(),7,Color.white,true);

            // Sprite icon
            if (i < plantIcons.Length && !lk)
            {
                var spr = Spr(plantIcons[i].sh, plantIcons[i].r, plantIcons[i].c);
                if (spr != null)
                {
                    var ico = G("Ico",cell.transform);
                    var irt = ico.GetComponent<RectTransform>();
                    irt.anchorMin=new Vector2(0.08f,0.38f);
                    irt.anchorMax=new Vector2(0.92f,0.83f);
                    irt.offsetMin=irt.offsetMax=Vector2.zero;
                    var ii=ico.AddComponent<Image>(); ii.sprite=spr; ii.preserveAspect=true;
                }
            }
            else if (lk)
            {
                var lkGO=G("Lock",cell.transform);
                R(lkGO,0,0,1,1,0,0,0,0);
                Img(lkGO,new Color(0,0,0,0.35f));
                Txt("T",lkGO.transform,"LOCK",8,Color.white,true);
            }

            // Name
            var nm=G("Name",cell.transform);
            R(nm,0,0.19f,1,0.40f,0,0,0,0);
            Txt("T",nm.transform,n,6,lk?C("#706050"):C("#3a2808"),true);

            // Cost
            var cs2=G("Cost",cell.transform);
            R(cs2,0,0.02f,1,0.21f,0,0,0,0);
            Txt("T",cs2.transform,"$"+cost,6,lk?C("#706050"):C("#7a4c10"),false);
        }

        // Info bar at bottom of content
        var info = G("InfoBar", content.transform);
        Img(info, C("#1e4808"));
        var ile = info.AddComponent<LayoutElement>(); ile.preferredHeight = 20;
        Txt("T",info.transform,"Select a seed  ·  click to plant",8,C("#a0e860"),false);
    }
}
