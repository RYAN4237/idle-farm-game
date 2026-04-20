using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Full UI restructure:
/// 1. Canvas: 1280x720 reference resolution
/// 2. TopBar: thin strip at top (Focus Points)  
/// 3. Farm area: fills everything below TopBar (no UI overlap)
/// 4. RightIconBar: slim icon column on right edge
/// 5. ExpandablePanel: full-height scrollable panel, slides from right
/// 6. PomoWidget: REMOVED from UICanvas - lives in its own separate Canvas
public class RestructureUI
{
    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found"); return; }

        // ── 1. Fix CanvasScaler to proper 16:9 resolution ─────────────
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f; // match height
            EditorUtility.SetDirty(canvas);
            Debug.Log("CanvasScaler: 1280x720");
        }

        // ── 2. Delete PomoWidget from UICanvas (will be its own canvas) ─
        var pomoGO = canvas.transform.Find("PomoWidget")?.gameObject;
        if (pomoGO != null) { Object.DestroyImmediate(pomoGO); Debug.Log("Removed PomoWidget from UICanvas"); }

        // Also clean old scattered pomo elements
        string[] oldEls = { "TimerText","ButtonBar","ProgressRing","PomoBG","DecorationPanel" };
        foreach (var n in oldEls)
        {
            var g = canvas.transform.Find(n)?.gameObject;
            if (g != null) { Object.DestroyImmediate(g); }
        }

        // ── 3. Fix TopBar: thin strip at very top ─────────────────────
        var topBar = canvas.transform.Find("TopBar")?.gameObject;
        if (topBar != null)
        {
            var rt = topBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(0, -32);
            rt.offsetMax = new Vector2(0, 0);
            EditorUtility.SetDirty(topBar);
        }

        // ── 4. Fix RightIconBar ────────────────────────────────────────
        var iconBar = canvas.transform.Find("RightIconBar")?.gameObject;
        if (iconBar != null)
        {
            var rt = iconBar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(1, 0.5f);
            rt.offsetMin = new Vector2(-52, 32); // below topbar
            rt.offsetMax = new Vector2(0, 0);
            EditorUtility.SetDirty(iconBar);
            Debug.Log("RightIconBar: full height right strip");
        }

        // ── 5. Rebuild ExpandablePanel with ScrollRect ─────────────────
        RebuildExpandablePanel(canvas);

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[RestructureUI] Done!");
    }

    static void RebuildExpandablePanel(GameObject canvas)
    {
        var old = canvas.transform.Find("ExpandablePanel");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        // Root panel - slides from right, full height (below topbar)
        var panel = GO("ExpandablePanel", canvas.transform);
        var pRT   = panel.GetComponent<RectTransform>();
        pRT.anchorMin = new Vector2(1, 0);
        pRT.anchorMax = new Vector2(1, 1);
        pRT.pivot     = new Vector2(1, 0.5f);
        pRT.offsetMin = new Vector2(-230, 32); // 230px wide, below topbar
        pRT.offsetMax = new Vector2(0, 0);
        pRT.anchoredPosition = new Vector2(235, 0); // hidden
        Bg(panel, Hex("#c0aa80"));
        var panelOL = panel.AddComponent<Outline>();
        panelOL.effectColor = Hex("#5a3a18"); panelOL.effectDistance = new Vector2(2, -2);

        // Title bar (top 28px)
        var title = GO("TitleBar", panel.transform);
        RT(title, 0,1, 1,1, 0,-28, 0,0);
        Bg(title, Hex("#3a7a10"));
        Lbl("TitleText", title.transform, "SEEDS SHOP", 11, Color.white, true);

        // Bottom bar (bottom 28px) - always visible above scroll
        var btm = GO("BottomBar", panel.transform);
        RT(btm, 0,0, 1,0, 0,0, 0,28);
        Bg(btm, Hex("#5a3a18"));
        // Resources
        var r1 = GO("R1", btm.transform); RT(r1,0f,0,0.38f,1, 4,0,0,0); Lbl("T",r1.transform,"$ 4100",8,Hex("#ffe060"),true);
        var r2 = GO("R2", btm.transform); RT(r2,0.38f,0,0.64f,1,0,0,0,0); Lbl("T",r2.transform,"♦ 3",8,Hex("#ffe060"),true);
        var r3 = GO("R3", btm.transform); RT(r3,0.64f,0,0.82f,1,0,0,0,0); Lbl("T",r3.transform,"⚡ 88",8,Hex("#ffe060"),true);
        var cls = GO("CloseBtn", btm.transform);
        RT(cls, 0.82f,0.08f, 1f,0.92f, 2,0,-2,0);
        Bg(cls, Hex("#a01818")); cls.AddComponent<Button>();
        Lbl("T", cls.transform, "X", 11, Color.white, true);

        // Category bar (left 34px, between title and bottom)
        var cat = GO("CatBar", panel.transform);
        RT(cat, 0,0, 0,1, 4,28, 38,-28);
        Bg(cat, new Color(0,0,0,0));

        string[] catSheets = {
            "Basic Plants.png", "Basic tools and meterials.png",
            "Basic Grass Biom things 1.png", "Basic Furniture.png"
        };
        int[] catRows = {0,0,0,0}, catCols = {0,2,3,0};
        Color[] catBgs = { Hex("#3a7a10"), Hex("#7a5010"), Hex("#104870"), Hex("#604030") };

        for (int i = 0; i < 4; i++)
        {
            var b = GO("Cat"+i, cat.transform);
            float yMax = 1f - i*0.26f, yMin = yMax-0.23f;
            RT(b, 0,yMin, 1,yMax, 1,1,-1,-1);
            Bg(b, catBgs[i]); b.AddComponent<Button>();
            var spr = LoadSprite(catSheets[i], catRows[i], catCols[i]);
            if (spr != null)
            {
                var ico = GO("Ico",b.transform);
                RT(ico,0.05f,0.05f,0.95f,0.95f,2,2,-2,-2);
                var ii = ico.AddComponent<Image>(); ii.sprite=spr; ii.preserveAspect=true;
            }
        }

        // ── ScrollRect for seed grid (between title and bottom, right of catbar)
        var scrollGO = GO("ScrollView", panel.transform);
        RT(scrollGO, 0,0, 1,1, 38,28, 0,-28);
        Bg(scrollGO, new Color(0,0,0,0));
        var scroll = scrollGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical   = true;
        scroll.scrollSensitivity = 20f;

        // Viewport (mask)
        var viewport = GO("Viewport", scrollGO.transform);
        RT(viewport, 0,0, 1,1, 0,0, 0,0);
        Bg(viewport, new Color(0,0,0,0));
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = viewport.GetComponent<RectTransform>();

        // Content (expands vertically)
        var content = GO("Content", viewport.transform);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0,1);
        contentRT.anchorMax = new Vector2(1,1);
        contentRT.pivot     = new Vector2(0.5f,1);
        contentRT.offsetMin = new Vector2(0,-800); // tall enough for all cells
        contentRT.offsetMax = new Vector2(0,0);
        Bg(content, new Color(0,0,0,0));
        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content  = contentRT;

        // Info bar (inside content, at bottom)
        // Grid layout for seeds
        var grid = GO("SeedGrid", content.transform);
        var gridRT = grid.GetComponent<RectTransform>();
        gridRT.anchorMin = new Vector2(0,1); gridRT.anchorMax = new Vector2(1,1);
        gridRT.pivot     = new Vector2(0.5f,1);
        gridRT.offsetMin = new Vector2(2,-600); gridRT.offsetMax = new Vector2(-2,0);
        Bg(grid, new Color(0,0,0,0));
        var glg = grid.AddComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(55,58); glg.spacing = new Vector2(3,3);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3;
        glg.childAlignment = TextAnchor.UpperLeft;
        glg.padding = new RectOffset(2,2,2,2);
        var gridCSF = grid.AddComponent<ContentSizeFitter>();
        gridCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Seed data
        string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";
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
            var (name,cost,cnt,lk) = seeds[i];
            var cell = GO("Cell_"+name, grid.transform);
            Bg(cell, lk ? Hex("#908070") : Hex("#e0cfa8"));
            var cOL = cell.AddComponent<Outline>();
            cOL.effectColor = Hex("#8b7040"); cOL.effectDistance = new Vector2(1,-1);
            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName = name; scb.seedCost = cost; scb.isLocked = lk;

            // Badge
            var bdg = GO("Badge", cell.transform);
            var brt = bdg.GetComponent<RectTransform>();
            brt.anchorMin=new Vector2(1,1); brt.anchorMax=new Vector2(1,1);
            brt.pivot=new Vector2(1,1); brt.sizeDelta=new Vector2(16,11);
            brt.anchoredPosition=new Vector2(-1,-1);
            Bg(bdg, lk?Hex("#706050"):Hex("#3a8010"));
            Lbl("T",bdg.transform,cnt.ToString(),7,Color.white,true);

            // Sprite
            if (i < plantIcons.Length && !lk)
            {
                var spr = LoadSprite(plantIcons[i].sh, plantIcons[i].r, plantIcons[i].c);
                if (spr != null)
                {
                    var ico = GO("Ico",cell.transform);
                    var irt = ico.GetComponent<RectTransform>();
                    irt.anchorMin=new Vector2(0.08f,0.38f); irt.anchorMax=new Vector2(0.92f,0.82f);
                    irt.offsetMin=irt.offsetMax=Vector2.zero;
                    var ii=ico.AddComponent<Image>(); ii.sprite=spr; ii.preserveAspect=true;
                }
            }
            else if (lk)
            {
                var lkGO = GO("Lock",cell.transform);
                var lrt = lkGO.GetComponent<RectTransform>();
                lrt.anchorMin=Vector2.zero; lrt.anchorMax=Vector2.one;
                lrt.offsetMin=lrt.offsetMax=Vector2.zero;
                Bg(lkGO,new Color(0,0,0,0.3f));
                Lbl("T",lkGO.transform,"LOCK",8,Color.white,true);
            }

            var nm = GO("Name",cell.transform);
            var nrt = nm.GetComponent<RectTransform>();
            nrt.anchorMin=new Vector2(0,0.19f); nrt.anchorMax=new Vector2(1,0.39f);
            nrt.offsetMin=nrt.offsetMax=Vector2.zero;
            Lbl("T",nm.transform,name,6,lk?Hex("#807060"):Hex("#3a2808"),true);

            var cs2 = GO("Cost",cell.transform);
            var crt = cs2.GetComponent<RectTransform>();
            crt.anchorMin=new Vector2(0,0.02f); crt.anchorMax=new Vector2(1,0.21f);
            crt.offsetMin=crt.offsetMax=Vector2.zero;
            Lbl("T",cs2.transform,"$"+cost,6,lk?Hex("#807060"):Hex("#7a4c10"),false);
        }

        // Info bar after grid
        var info = GO("InfoBar", content.transform);
        var irt2 = info.GetComponent<RectTransform>();
        irt2.anchorMin=new Vector2(0,1); irt2.anchorMax=new Vector2(1,1);
        irt2.pivot=new Vector2(0.5f,1);
        irt2.offsetMin=new Vector2(2,-620); irt2.offsetMax=new Vector2(-2,-600);
        Bg(info, Hex("#2a5c08"));
        Lbl("InfoTxt",info.transform,"Select a seed · click to plant",7,Hex("#b8f880"),false);

        // Wire UIManager
        var uiMgr = canvas.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.expandablePanel = pRT;
            uiMgr.closeButton = cls.GetComponent<Button>();
            EditorUtility.SetDirty(canvas);
        }

        Debug.Log("ExpandablePanel rebuilt with ScrollRect");
    }

    static Sprite LoadSprite(string sheet, int row, int col)
    {
        string OBJ = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";
        string key = System.IO.Path.GetFileNameWithoutExtension(sheet) + "_" + row + "_" + col;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(OBJ + sheet))
            if (obj is Sprite s && s.name == key) return s;
        return null;
    }

    static GameObject GO(string n, Transform p)
    { var g=new GameObject(n); g.transform.SetParent(p,false); g.AddComponent<RectTransform>(); return g; }
    static void Bg(GameObject g, Color c)
    { var i=g.GetComponent<Image>()??g.AddComponent<Image>(); i.color=c; }
    static void RT(GameObject g,float ax,float ay,float bx,float by,float l,float b,float r,float t)
    { var rt=g.GetComponent<RectTransform>(); rt.anchorMin=new Vector2(ax,ay); rt.anchorMax=new Vector2(bx,by); rt.offsetMin=new Vector2(l,b); rt.offsetMax=new Vector2(r,t); }
    static TextMeshProUGUI Lbl(string n,Transform p,string text,float sz,Color c,bool bold)
    { var g=GO(n,p); var rt=g.GetComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=rt.offsetMax=Vector2.zero;
      var tm=g.AddComponent<TextMeshProUGUI>(); tm.text=text; tm.fontSize=sz; tm.color=c; tm.alignment=TextAlignmentOptions.Center; tm.fontStyle=bold?FontStyles.Bold:FontStyles.Normal; tm.enableWordWrapping=false; tm.overflowMode=TextOverflowModes.Truncate; return tm; }
}
