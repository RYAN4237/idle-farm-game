using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// Rebuilds the entire UI using Sprout Lands UI Pack assets.
/// Run via menu: Farm > Apply Sprout Lands UI
public class ApplySproutLandsUI
{
    const string UI_BASE   = "Assets/Sprout Lands - UI Pack - Basic pack/Sprite sheets/";
    const string OBJ_BASE  = "Assets/Sprout Lands - Sprites - Basic pack/Objects/";
    const string CHAR_BASE = "Assets/Sprout Lands - Sprites - Basic pack/Characters/";

    static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out Color c); return c; }

    // ─────────────────────────────────────────────────────────────────
    [MenuItem("Farm/Apply Sprout Lands UI")]
    public static void Execute()
    {
        var canvas = GameObject.Find("UICanvas");
        if (canvas == null) { Debug.LogError("UICanvas not found in scene."); return; }

        DestroyChildren(canvas.transform, "RightIconBar", "ExpandablePanel", "TopBar");

        BuildTopBar(canvas);
        BuildIconBar(canvas);
        BuildPanel(canvas);

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[ApplySproutLandsUI] Done!");
    }

    // ── Helpers ──────────────────────────────────────────────────────
    static void DestroyChildren(Transform parent, params string[] names)
    {
        foreach (var n in names)
        {
            var t = parent.Find(n);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }
    }

    static GameObject GO(string n, Transform p)
    {
        var g = new GameObject(n);
        g.transform.SetParent(p, false);
        g.AddComponent<RectTransform>();
        return g;
    }

    static void RT(GameObject g, float ax, float ay, float bx, float by,
                   float l=0, float b=0, float r=0, float t=0)
    {
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(ax, ay); rt.anchorMax = new Vector2(bx, by);
        rt.offsetMin = new Vector2(l, b);   rt.offsetMax  = new Vector2(r, t);
    }

    static void Fill(GameObject g)
    {
        var rt = g.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    // Load a sub-sprite by name from a sprite sheet
    static Sprite Sub(string assetPath, string spriteName)
    {
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            if (obj is Sprite s && s.name == spriteName) return s;
        // Fallback: return first sprite
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            if (obj is Sprite s) return s;
        Debug.LogWarning($"Sprite not found: {spriteName} in {assetPath}");
        return null;
    }

    // Set Image with 9-slice sprite
    static Image NineSlice(GameObject g, Sprite spr, int border = 6)
    {
        var img = g.GetComponent<Image>() ?? g.AddComponent<Image>();
        img.sprite = spr;
        img.type   = Image.Type.Sliced;
        img.color  = Color.white;
        if (spr != null && spr.border == Vector4.zero)
        {
            // Manually set border on the sprite importer if needed
        }
        return img;
    }

    // Set Image with simple sprite
    static Image Img(GameObject g, Sprite spr, Color? tint = null)
    {
        var img = g.GetComponent<Image>() ?? g.AddComponent<Image>();
        img.sprite = spr;
        img.color  = tint ?? Color.white;
        img.preserveAspect = true;
        return img;
    }

    static Image SolidImg(GameObject g, Color c)
    {
        var img = g.GetComponent<Image>() ?? g.AddComponent<Image>();
        img.color = c;
        return img;
    }

    static TextMeshProUGUI Label(string name, Transform parent, string text,
                                  float size, Color col, TextAlignmentOptions align =
                                  TextAlignmentOptions.Center, bool bold = false)
    {
        var g  = GO(name, parent);
        Fill(g);
        var tm = g.AddComponent<TextMeshProUGUI>();
        tm.text      = text; tm.fontSize = size; tm.color = col;
        tm.alignment = align; tm.enableWordWrapping = false;
        tm.overflowMode = TextOverflowModes.Truncate;
        tm.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        return tm;
    }

    // ── Load key Sprout Lands UI sprites ─────────────────────────────
    // "Setting menu.png" = 256x144, left half (0,0,128,144) = panel with title bar
    //                                right half (128,0,128,144) = blank panel
    // "Square Buttons 26x26.png" = 96x192, 3 cols x ~7 rows of 26x26 buttons
    // "dialog box big.png" = 176x48, single dialog box used as wide bottom bar
    // "UI Big Play Button.png" = 192x64, 2 cols x 2 rows: normal + pressed variants

    static Sprite PanelSprite()
    {
        // Use "Setting menu.png" right panel (plain, no title) — index 1
        var path = UI_BASE + "Setting menu.png";
        // Try named sub-sprite first
        var s = Sub(path, "Setting menu_1");
        if (s == null) s = Sub(path, "Setting menu_0");
        return s;
    }

    static Sprite ButtonSprite(int variant = 0)
    {
        // Square Buttons 26x26: row 0 = light, row 1 = tan, row 2 = brown, etc.
        // 3 cols x many rows; variant 0=normal, 1=pressed (darker)
        var path = UI_BASE + "buttons/Square Buttons 26x26.png";
        string name = $"Square Buttons 26x26_{variant}";
        var s = Sub(path, name);
        if (s == null) s = Sub(path, "Square Buttons 26x26_3"); // tan button fallback
        return s;
    }

    static Sprite WideButtonSprite(bool pressed = false)
    {
        var path = UI_BASE + "UI Big Play Button.png";
        // Row 0 = normal buttons, row 1 = pressed
        string name = pressed ? "UI Big Play Button_2" : "UI Big Play Button_0";
        var s = Sub(path, name);
        if (s == null) s = Sub(path, "UI Big Play Button_0");
        return s;
    }

    static Sprite DialogBoxSprite()
    {
        return Sub(UI_BASE + "Dialouge UI/dialog box big.png", null) ??
               AssetDatabase.LoadAssetAtPath<Sprite>(UI_BASE + "Dialouge UI/dialog box big.png");
    }

    static Sprite PlantSprite(int row, int col)
    {
        string name = $"Basic Plants_{row}_{col}";
        return Sub(OBJ_BASE + "Basic Plants.png", name);
    }

    // ── Top Bar ───────────────────────────────────────────────────────
    // Small bar at very top showing FP counter + game title
    static void BuildTopBar(GameObject canvas)
    {
        var bar = GO("TopBar", canvas.transform);
        RT(bar, 0, 1, 1, 1, 0, -24, 0, 0);

        // Sprout Lands tan panel background
        var bgSpr = PanelSprite();
        var img = bar.GetComponent<Image>() ?? bar.AddComponent<Image>();
        if (bgSpr != null) { img.sprite = bgSpr; img.type = Image.Type.Sliced; }
        else img.color = Hex("#c8ae7d");

        // Left: game title
        var title = GO("Title", bar.transform);
        RT(title, 0, 0, 0.35f, 1);
        var tm = title.AddComponent<TextMeshProUGUI>();
        tm.text = "✦ Farm Idle"; tm.fontSize = 10; tm.color = Hex("#3a2008");
        tm.fontStyle = FontStyles.Bold; tm.alignment = TextAlignmentOptions.MidlineLeft;
        tm.margin = new Vector4(6, 0, 0, 0);

        // Right: FP value
        var fpGO = GO("FPDisplay", bar.transform);
        RT(fpGO, 0.65f, 0, 1, 1);
        var fp = fpGO.AddComponent<TextMeshProUGUI>();
        fp.text = "0 FP"; fp.fontSize = 10; fp.color = Hex("#7a4c00");
        fp.fontStyle = FontStyles.Bold; fp.alignment = TextAlignmentOptions.MidlineRight;
        fp.margin = new Vector4(0, 0, 6, 0);

        var topFP = fpGO.AddComponent<TopBarFP>();
        topFP.valueText = fp;
    }

    // ── Right Icon Bar ────────────────────────────────────────────────
    static void BuildIconBar(GameObject canvas)
    {
        var bar = GO("RightIconBar", canvas.transform);

        // Anchor right side, vertically centered in bottom 3/4
        var bRT = bar.GetComponent<RectTransform>();
        bRT.anchorMin = new Vector2(1, 0.08f);
        bRT.anchorMax = new Vector2(1, 0.85f);
        bRT.pivot     = new Vector2(1, 0.5f);
        bRT.sizeDelta = new Vector2(36, 0);
        bRT.anchoredPosition = Vector2.zero;

        // Sprout Lands panel as background
        var bgSpr = PanelSprite();
        var img = bar.GetComponent<Image>() ?? bar.AddComponent<Image>();
        if (bgSpr != null) { img.sprite = bgSpr; img.type = Image.Type.Sliced; img.color = Color.white; }
        else img.color = Hex("#b8945a");

        var vlg = bar.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment      = TextAnchor.MiddleCenter;
        vlg.spacing             = 4;
        vlg.padding             = new RectOffset(3, 3, 6, 6);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = false;

        // 3 icon buttons: Seeds, Build, Upgrades
        var btnDefs = new (string name, string label, int plantRow, int plantCol, string btnVar)[]
        {
            ("SeedButton",    "🌱", 0, 0, "3"),   // seed icon
            ("BuildButton",   "🔨", 0, 2, "6"),   // tool icon
            ("UpgradeButton", "⭐", 0, 4, "9"),   // star icon
        };

        foreach (var (bName, icon, pr, pc, bVar) in btnDefs)
        {
            var btnGO = GO(bName, bar.transform);
            var csf = btnGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rt = btnGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0, 30);

            // Button background: Sprout Lands square button
            var btnSpr = ButtonSprite(int.TryParse(bVar, out int bv) ? bv : 3);
            var bi = btnGO.GetComponent<Image>() ?? btnGO.AddComponent<Image>();
            if (btnSpr != null) { bi.sprite = btnSpr; bi.type = Image.Type.Sliced; }
            else bi.color = Hex("#c8a060");

            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(1, 1, 0.75f);
            colors.pressedColor     = new Color(0.7f, 0.55f, 0.3f);
            btn.colors = colors;

            // Icon (plant sprite)
            var icoGO = GO("Ico", btnGO.transform);
            var irt = icoGO.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.1f, 0.1f); irt.anchorMax = new Vector2(0.9f, 0.9f);
            irt.offsetMin = irt.offsetMax = Vector2.zero;
            var plantSpr = PlantSprite(pr, pc);
            if (plantSpr != null)
            {
                var ii = icoGO.AddComponent<Image>();
                ii.sprite = plantSpr; ii.preserveAspect = true;
            }
            else
            {
                var tm = icoGO.AddComponent<TextMeshProUGUI>();
                tm.text = icon; tm.fontSize = 14; tm.alignment = TextAlignmentOptions.Center;
                tm.color = Hex("#3a2008");
            }
        }

        // Wire UIManager buttons
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        uiMgr.seedButton    = bar.transform.Find("SeedButton")?.GetComponent<Button>();
        uiMgr.buildButton   = bar.transform.Find("BuildButton")?.GetComponent<Button>();
        uiMgr.upgradeButton = bar.transform.Find("UpgradeButton")?.GetComponent<Button>();
        EditorUtility.SetDirty(bar);
    }

    // ── Expandable Panel ──────────────────────────────────────────────
    static void BuildPanel(GameObject canvas)
    {
        var panel = GO("ExpandablePanel", canvas.transform);
        var pRT   = panel.GetComponent<RectTransform>();

        // Right side, slides out horizontally
        pRT.anchorMin = new Vector2(1, 0.06f);
        pRT.anchorMax = new Vector2(1, 0.94f);
        pRT.pivot     = new Vector2(1, 0.5f);
        pRT.sizeDelta = new Vector2(200, 0);
        pRT.anchoredPosition = new Vector2(205, 0); // hidden to start

        // Sprout Lands panel background
        var bgSpr = PanelSprite();
        var pImg = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        if (bgSpr != null) { pImg.sprite = bgSpr; pImg.type = Image.Type.Sliced; pImg.color = Color.white; }
        else pImg.color = Hex("#c8ae7d");

        // ── Title bar at top ──
        var titleBar = GO("TitleBar", panel.transform);
        RT(titleBar, 0, 1, 1, 1, 6, -28, -6, -4);
        SolidImg(titleBar, Hex("#3a7a10"));
        var titleTxt = GO("TitleTxt", titleBar.transform);
        Fill(titleTxt);
        var ttm = titleTxt.AddComponent<TextMeshProUGUI>();
        ttm.text = "SEEDS"; ttm.fontSize = 10; ttm.color = Color.white;
        ttm.fontStyle = FontStyles.Bold; ttm.alignment = TextAlignmentOptions.Center;

        // ── Close button (X) ──
        var closeGO = GO("CloseBtn", titleBar.transform);
        var cRT = closeGO.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(1, 0); cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(1, 0.5f); cRT.sizeDelta = new Vector2(22, 0);
        cRT.offsetMin = new Vector2(cRT.offsetMin.x, 2);
        cRT.offsetMax = new Vector2(-2, -2);
        var closeSpr = ButtonSprite(6);
        var ci = closeGO.GetComponent<Image>() ?? closeGO.AddComponent<Image>();
        if (closeSpr != null) { ci.sprite = closeSpr; ci.type = Image.Type.Sliced; }
        else ci.color = Hex("#9a2010");
        var closeBtn = closeGO.AddComponent<Button>();
        var closeTxt = GO("X", closeGO.transform);
        Fill(closeTxt);
        var ctm = closeTxt.AddComponent<TextMeshProUGUI>();
        ctm.text = "✕"; ctm.fontSize = 10; ctm.color = Color.white;
        ctm.alignment = TextAlignmentOptions.Center; ctm.fontStyle = FontStyles.Bold;

        // ── Content area (between title and bottom) ──
        var content = GO("Content", panel.transform);
        RT(content, 0, 0, 1, 1, 4, 4, -4, -32);

        // Scrollable seed grid
        BuildSeedGrid(content.transform);

        // ── Wire UIManager ──
        var uiMgr = canvas.GetComponent<UIManager>() ?? canvas.AddComponent<UIManager>();
        uiMgr.expandablePanel = pRT;
        uiMgr.closeButton     = closeBtn;
        uiMgr.panelTitle      = ttm;

        var ib = canvas.transform.Find("RightIconBar");
        if (ib != null)
        {
            uiMgr.seedButton    = ib.Find("SeedButton")?.GetComponent<Button>();
            uiMgr.buildButton   = ib.Find("BuildButton")?.GetComponent<Button>();
            uiMgr.upgradeButton = ib.Find("UpgradeButton")?.GetComponent<Button>();
        }
        EditorUtility.SetDirty(panel);
    }

    // ── Seed Grid ─────────────────────────────────────────────────────
    static void BuildSeedGrid(Transform parent)
    {
        // Scroll view
        var sv = GO("ScrollView", parent);
        Fill(sv);
        var scroll = sv.AddComponent<ScrollRect>();
        scroll.horizontal = false;

        var viewport = GO("Viewport", sv.transform);
        Fill(viewport);
        viewport.AddComponent<RectMask2D>();
        scroll.viewport = viewport.GetComponent<RectTransform>();

        var gridGO = GO("SeedGrid", viewport.transform);
        var gRT = gridGO.GetComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0, 1); gRT.anchorMax = new Vector2(1, 1);
        gRT.pivot     = new Vector2(0.5f, 1);
        gRT.offsetMin = gRT.offsetMax = Vector2.zero;
        scroll.content = gRT;

        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.cellSize       = new Vector2(56, 60);
        glg.spacing        = new Vector2(3, 3);
        glg.constraint     = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount= 3;
        glg.childAlignment = TextAnchor.UpperCenter;
        glg.padding        = new RectOffset(3, 3, 3, 3);

        var csf = gridGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Seed entries
        var seeds = new (string name, int cost, int growSec, float reward, bool locked, int plantRow, int plantCol)[]
        {
            ("Wheat",   10,  20,  30f, false, 0, 0),
            ("Carrot",  15,  30,  45f, false, 0, 1),
            ("Beet",    20,  45,  65f, false, 0, 2),
            ("Turnip",  18,  35,  55f, false, 0, 3),
            ("Pumpkin", 40,  60,  90f, false, 0, 4),
            ("Corn",    30,  50,  80f, false, 0, 5),
            ("Wheat+",  50,  15, 120f, false, 1, 0),
            ("Carrot+", 60,  25, 150f, false, 1, 1),
            ("Beet+",   80,  40, 200f, false, 1, 2),
            ("Turnip+", 120, 30, 250f, false, 1, 3),
            ("Shroom",  200, 90, 400f, true,  1, 4),
            ("Dragon",  500, 120,800f, true,  1, 5),
        };

        // Use Sprout Lands "Inventory_Blocks_Spritesheet" as cell background
        // 144x144 = 3x3 blocks of 48x48; first = dark, second = medium, third = light
        var invPath = "Assets/Sprout Lands - UI Pack - Basic pack/emojis-free/emoji style ui/Inventory_Blocks_Spritesheet.png";
        var cellSpr  = Sub(invPath, "Inventory_Blocks_Spritesheet_0");
        var cellSprLk= Sub(invPath, "Inventory_Blocks_Spritesheet_6"); // darker for locked

        foreach (var (name, cost, growSec, reward, locked, pr, pc) in seeds)
        {
            var cell = GO("Cell_" + name, gridGO.transform);

            // Cell background: Sprout Lands inventory block
            var ci = cell.GetComponent<Image>() ?? cell.AddComponent<Image>();
            var useSpr = locked ? cellSprLk : cellSpr;
            if (useSpr != null) { ci.sprite = useSpr; ci.type = Image.Type.Sliced; ci.color = Color.white; }
            else ci.color = locked ? Hex("#9a8060") : Hex("#e0c890");

            var scb = cell.AddComponent<SeedCellButton>();
            scb.seedName = name; scb.seedCost = cost; scb.isLocked = locked;

            // Plant icon (top 55% of cell)
            var icoGO = GO("Ico", cell.transform);
            var irt = icoGO.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0.1f, 0.42f); irt.anchorMax = new Vector2(0.9f, 0.88f);
            irt.offsetMin = irt.offsetMax = Vector2.zero;
            var plantSpr = PlantSprite(pr, pc);
            if (plantSpr != null && !locked)
            {
                var ii = icoGO.AddComponent<Image>();
                ii.sprite = plantSpr; ii.preserveAspect = true;
            }
            else if (locked)
            {
                var lt = icoGO.AddComponent<TextMeshProUGUI>();
                lt.text = "🔒"; lt.fontSize = 18; lt.alignment = TextAlignmentOptions.Center;
                lt.color = Hex("#60503a");
            }

            // Name label
            var nmGO = GO("Name", cell.transform);
            var nrt = nmGO.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 0.26f); nrt.anchorMax = new Vector2(1, 0.44f);
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            var ntm = nmGO.AddComponent<TextMeshProUGUI>();
            ntm.text = name; ntm.fontSize = 7; ntm.fontStyle = FontStyles.Bold;
            ntm.color = locked ? Hex("#706050") : Hex("#3a2808");
            ntm.alignment = TextAlignmentOptions.Center; ntm.enableWordWrapping = false;
            ntm.overflowMode = TextOverflowModes.Truncate;

            // Cost label
            var costGO = GO("Cost", cell.transform);
            var crt = costGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0, 0.04f); crt.anchorMax = new Vector2(1, 0.28f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var ctm = costGO.AddComponent<TextMeshProUGUI>();
            ctm.text = locked ? $"🔒{cost}" : $"${cost}"; ctm.fontSize = 6;
            ctm.color = locked ? Hex("#807060") : Hex("#7a4c10");
            ctm.alignment = TextAlignmentOptions.Center; ctm.enableWordWrapping = false;
            ctm.overflowMode = TextOverflowModes.Truncate;
        }
    }
}
