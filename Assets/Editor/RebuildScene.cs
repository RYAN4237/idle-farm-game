using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using TMPro;

/// One-click full rebuild: clears scene, builds Rusty Retirement style farm.
/// Farm is a horizontal strip at the bottom ~25% of screen.
/// Grass tilemap background + tilled dirt plots + farmer character.
public class RebuildScene
{
    // ── Sprout Lands paths ──────────────────────────────────────────
    const string SL    = "Assets/Sprout Lands - Sprites - Basic pack/";
    const string SL_UI = "Assets/Sprout Lands - UI Pack - Basic pack/";

    // Tile size in pixels, PPU for world unit
    const int    TILE_PX  = 16;
    const float  PPU       = 16f;       // 16px = 1 world unit

    // Farm grid: 20 cols × 4 rows of tilled dirt plots
    const int    FARM_COLS = 20;
    const int    FARM_ROWS = 4;
    const float  CELL      = 1f;        // 1 world unit per cell (= 16px tile)

    // Camera orthographic height = screen height in world units
    // Window is bottom 25% of 1080p → ~270px tall
    // At PPU=16, 270/16 = 16.875 world units tall
    // orthoSize = halfHeight = 8f
    const float CAM_ORTHO  = 8f;

    [MenuItem("Farm/Rebuild Scene (Rusty Retirement Style)")]
    public static void Execute()
    {
        // 1. Configure all sprite sheets
        ConfigureAllSprites();

        // 2. Clear & rebuild scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 3. Camera
        BuildCamera();

        // 4. Tilemap: grass everywhere + tilled dirt on farm rows
        BuildTilemap();

        // 5. Decorations: flowers/bushes from grass biom
        BuildDecorations();

        // 6. Farmer character
        BuildFarmer();

        // 7. Farm systems
        BuildSystems();

        // 8. UI: Rusty Retirement style bottom bar
        BuildUI();

        // 9. Save
        string path = "Assets/Scenes/DesktopIdleGame.unity";
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RebuildScene] Done! Open DesktopIdleGame.unity to see the result.");
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 1: Configure sprites
    // ═══════════════════════════════════════════════════════════════
    static void ConfigureAllSprites()
    {
        ConfigureGrid(SL + "Tilesets/Grass.png",       16, 16);
        ConfigureGrid(SL + "Tilesets/Tilled_Dirt.png", 16, 16);
        ConfigureGrid(SL + "Objects/Basic Plants.png",  16, 16);
        ConfigureGrid(SL + "Objects/Basic_Grass_Biom_things.png", 16, 16);
        ConfigureGrid(SL + "Characters/Basic Charakter Spritesheet.png", 48, 48);
        ConfigureGrid(SL_UI + "Sprite sheets/Sprite sheet for Basic Pack.png", 16, 16);
        ConfigureGrid(SL_UI + "Sprite sheets/buttons/Square Buttons 26x26.png", 26, 26);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RebuildScene] Sprites configured.");
    }

    static void ConfigureGrid(string path, int tw, int th)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogWarning($"Not found: {path}"); return; }

        bool changed = false;
        if (ti.spriteImportMode    != SpriteImportMode.Multiple)    { ti.spriteImportMode    = SpriteImportMode.Multiple;    changed = true; }
        if (ti.filterMode          != FilterMode.Point)             { ti.filterMode          = FilterMode.Point;             changed = true; }
        if (ti.textureCompression  != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (!ti.alphaIsTransparency)                                { ti.alphaIsTransparency = true;                         changed = true; }
        if (ti.mipmapEnabled)                                       { ti.mipmapEnabled       = false;                        changed = true; }
        if ((int)ti.spritePixelsPerUnit != (int)PPU)                { ti.spritePixelsPerUnit = PPU;                          changed = true; }

        if (changed) ti.SaveAndReimport();

        // Slice into grid
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) return;

        int cols = tex.width  / tw;
        int rows = tex.height / th;
        string baseName = Path.GetFileNameWithoutExtension(path);

        var metas = new List<SpriteMetaData>();
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
        {
            metas.Add(new SpriteMetaData
            {
                name      = $"{baseName}_{r * cols + c}",
                rect      = new Rect(c * tw, tex.height - (r + 1) * th, tw, th),
                alignment = 0,
                pivot     = new Vector2(0.5f, 0.5f)
            });
        }

        ti.spritesheet = metas.ToArray();
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        Debug.Log($"[RebuildScene] Sliced {baseName}: {cols}×{rows}");
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 2: Camera
    // ═══════════════════════════════════════════════════════════════
    static void BuildCamera()
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic    = true;
        cam.orthographicSize = CAM_ORTHO;
        cam.backgroundColor  = new Color(0.47f, 0.71f, 0.34f); // grass green fallback
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.depth            = -1;

        // Center camera on farm: farm goes from y=0 to y=FARM_ROWS*CELL
        // We want to see the farm + a bit of grass above
        float farmCenterX = FARM_COLS * CELL * 0.5f;
        float farmCenterY = FARM_ROWS * CELL * 0.5f + 1f;
        camGO.transform.position = new Vector3(farmCenterX, farmCenterY, -10f);

        camGO.AddComponent<AudioListener>();
        Debug.Log("[RebuildScene] Camera built.");
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 3: Tilemap
    // ═══════════════════════════════════════════════════════════════
    static void BuildTilemap()
    {
        EnsureDir("Assets/Data/Tiles");

        var root = new GameObject("Tilemap");
        var grid = root.AddComponent<Grid>();
        grid.cellSize = new Vector3(CELL, CELL, 0f);
        root.transform.position = Vector3.zero;

        // Layer 1: Grass everywhere (sort -20)
        var grassTM = AddTilemapChild(root, "GrassLayer", -20);
        FillGrass(grassTM);

        // Layer 2: Tilled dirt on farm area (sort -10)
        var dirtTM = AddTilemapChild(root, "DirtLayer", -10);
        FillDirt(dirtTM);

        Debug.Log("[RebuildScene] Tilemap built.");
    }

    static Tilemap AddTilemapChild(GameObject root, string name, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingOrder = sortOrder;
        return tm;
    }

    static void FillGrass(Tilemap tm)
    {
        // Grass_0 is the full center grass tile (top-left of sheet)
        var spr = LoadSprite(SL + "Tilesets/Grass.png", "Grass_0");
        if (spr == null) { Debug.LogError("Grass_0 not found!"); return; }
        var tile = MakeTile("Grass0", spr);

        // Fill a wide area: 4 cols beyond farm on each side, 3 rows above + 2 below
        int x0 = -4, x1 = FARM_COLS + 4;
        int y0 = -2, y1 = FARM_ROWS + 3;
        for (int x = x0; x < x1; x++)
        for (int y = y0; y < y1; y++)
            tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    static void FillDirt(Tilemap tm)
    {
        // Tilled_Dirt_0 is the isolated center dirt tile
        var spr = LoadSprite(SL + "Tilesets/Tilled_Dirt.png", "Tilled_Dirt_0");
        if (spr == null) { Debug.LogError("Tilled_Dirt_0 not found!"); return; }
        var tile = MakeTile("Dirt0", spr);

        for (int x = 0; x < FARM_COLS; x++)
        for (int y = 0; y < FARM_ROWS; y++)
            tm.SetTile(new Vector3Int(x, y, 0), tile);
    }

    static Tile MakeTile(string name, Sprite spr)
    {
        string path = $"Assets/Data/Tiles/{name}.asset";
        var t = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (t != null) { t.sprite = spr; EditorUtility.SetDirty(t); return t; }
        t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = spr;
        t.colliderType = Tile.ColliderType.None;
        AssetDatabase.CreateAsset(t, path);
        return t;
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 4: Decorations (flowers, bushes — Rusty Retirement feel)
    // ═══════════════════════════════════════════════════════════════
    static void BuildDecorations()
    {
        // Basic_Grass_Biom_things: 9×5 grid of 16px sprites
        // index 0=big tree, 4=pink flower, 5=mushroom, 6=rock, 8=small bush etc
        string biomPath = SL + "Objects/Basic_Grass_Biom_things.png";
        int cols = 9;

        // Pick some nice decorative sprites
        var decorIndices = new int[] { 3, 4, 5, 6, 7, 8 }; // flowers, mushrooms, rocks, bush

        var decorRoot = new GameObject("Decorations");

        var rng = new System.Random(42); // deterministic
        // Place decorations in the grass border areas (not on farm)
        for (int i = 0; i < 40; i++)
        {
            float x, y;
            // Either left/right of farm or above farm
            int zone = rng.Next(3);
            if (zone == 0)      { x = rng.Next(-3, 0) + (float)rng.NextDouble(); y = rng.Next(-1, FARM_ROWS + 2) + (float)rng.NextDouble(); }
            else if (zone == 1) { x = rng.Next(FARM_COLS, FARM_COLS + 3) + (float)rng.NextDouble(); y = rng.Next(-1, FARM_ROWS + 2) + (float)rng.NextDouble(); }
            else                { x = rng.Next(-2, FARM_COLS + 2) + (float)rng.NextDouble(); y = rng.Next(FARM_ROWS, FARM_ROWS + 3) + (float)rng.NextDouble(); }

            int sprIdx = decorIndices[rng.Next(decorIndices.Length)];
            var spr = LoadSprite(biomPath, $"Basic_Grass_Biom_things_{sprIdx}");
            if (spr == null) continue;

            var go = new GameObject($"Decor_{i}");
            go.transform.SetParent(decorRoot.transform, false);
            go.transform.position = new Vector3(x, y, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingOrder = Mathf.RoundToInt(-y * 10); // pseudo depth sort
        }

        Debug.Log("[RebuildScene] Decorations placed.");
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 5: Farmer character
    // ═══════════════════════════════════════════════════════════════
    static void BuildFarmer()
    {
        string charPath = SL + "Characters/Basic Charakter Spritesheet.png";
        // 4 cols × 4 rows of 48×48:  row0=down, row1=up, row2=left, row3=right

        var go = new GameObject("Farmer");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 50;

        // Start sprite = first frame facing down
        var startSpr = LoadSprite(charPath, "Basic Charakter Spritesheet_0");
        if (startSpr != null) sr.sprite = startSpr;

        // Scale: 48px character on 16px grid → 3x scale looks right, but we want
        // character to be ~1.5 cells tall → scale = 1.5 * CELL / (48/PPU)
        // 48/16 = 3 world units tall at PPU=16, we want ~1.5 → scale = 0.5
        go.transform.localScale = Vector3.one * 0.7f;
        go.transform.position = new Vector3(FARM_COLS * 0.5f, FARM_ROWS * 0.5f, -0.1f);

        var fc = go.AddComponent<FarmerCharacter>();
        // Inject walk frame arrays directly
        fc.walkDownFrames  = GetCharFrames(charPath, 0, 4, 4);
        fc.walkUpFrames    = GetCharFrames(charPath, 1, 4, 4);
        fc.walkLeftFrames  = GetCharFrames(charPath, 2, 4, 4);
        fc.walkRightFrames = GetCharFrames(charPath, 3, 4, 4);

        EditorUtility.SetDirty(fc);
        Debug.Log("[RebuildScene] Farmer built.");
    }

    static Sprite[] GetCharFrames(string path, int row, int totalCols, int count)
    {
        var list = new List<Sprite>();
        for (int c = 0; c < count; c++)
        {
            int idx = row * totalCols + c;
            var spr = LoadSprite(path, $"Basic Charakter Spritesheet_{idx}");
            if (spr != null) list.Add(spr);
        }
        return list.ToArray();
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 6: Systems GameObjects
    // ═══════════════════════════════════════════════════════════════
    static void BuildSystems()
    {
        var mgr = new GameObject("GameManager");
        var fg = mgr.AddComponent<FarmGrid>();
        fg.cellSize   = CELL;
        fg.gridWidth  = FARM_COLS;
        fg.gridHeight = FARM_ROWS;
        fg.originX    = 0f;
        fg.originY    = 0f;
        // Disable the grid lines visual (Rusty Retirement has no grid lines)
        fg.gridColor = new Color(0, 0, 0, 0);

        mgr.AddComponent<FarmingSystem>();
        mgr.AddComponent<ResourceSystem>();
        mgr.AddComponent<IdleSystem>();
        mgr.AddComponent<PlacementManager>();
        mgr.AddComponent<SaveSystem>();

        // EventSystem
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        // Spawn farm plots on the dirt tiles
        BuildFarmPlots(fg);

        Debug.Log("[RebuildScene] Systems built.");
    }

    static void BuildFarmPlots(FarmGrid fg)
    {
        var plotRoot = new GameObject("FarmPlots");
        for (int x = 0; x < FARM_COLS; x++)
        for (int y = 0; y < FARM_ROWS; y++)
        {
            float wx = fg.originX + x * fg.cellSize + fg.cellSize * 0.5f;
            float wy = fg.originY + y * fg.cellSize + fg.cellSize * 0.5f;
            var plot = PlotFactory.Create(new Vector3(wx, wy, 0f), fg.cellSize);
            plot.transform.SetParent(plotRoot.transform, true);
        }
        Debug.Log($"[RebuildScene] {FARM_COLS * FARM_ROWS} farm plots created.");
    }

    // ═══════════════════════════════════════════════════════════════
    // STEP 7: UI — Rusty Retirement style
    // Bottom overlay: coin counter + seed shop button + progress bars
    // Uses Sprout Lands UI sprite sheet
    // ═══════════════════════════════════════════════════════════════
    static void BuildUI()
    {
        string uiPath = SL_UI + "Sprite sheets/Sprite sheet for Basic Pack.png";
        // 56 cols × 15 rows of 16px sprites
        // Row 8 = wide brown panel/bar sprites (index 448+)
        // Row 9 cols 0-3 = brown rounded panels
        // Row 12 col 0-2 = PLAY buttons

        // Canvas
        var canvasGO = new GameObject("UICanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 270); // 16:9 bottom strip
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.Shrink;
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<UIManager>();

        var canvasRT = canvasGO.GetComponent<RectTransform>();

        // ── Bottom bar background ──────────────────────────────────
        // Uses a wide panel sprite from UI sheet
        // Sprite_sheet_for_Basic_Pack row 8 (index 8*56=448) = left panel cap
        // But easiest: use a solid brown color matching Sprout Lands palette
        var barGO = new GameObject("BottomBar");
        barGO.transform.SetParent(canvasGO.transform, false);
        var barRT = barGO.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0);
        barRT.anchorMax = new Vector2(1, 1);
        barRT.offsetMin = barRT.offsetMax = Vector2.zero;

        // Sprout Lands brown panel (row 0 col 0 = square panel bg)
        var panelSpr = LoadSprite(uiPath, "Sprite sheet for Basic Pack_0");
        var barImg = barGO.AddComponent<Image>();
        if (panelSpr != null)
        {
            barImg.sprite = panelSpr;
            barImg.type   = Image.Type.Sliced;
            barImg.color  = new Color(0.85f, 0.73f, 0.52f, 0.95f);
        }
        else
        {
            barImg.color = new Color(0.85f, 0.73f, 0.52f, 0.95f);
        }

        // ── Coin/FP display (left) ─────────────────────────────────
        var fpGO = new GameObject("FPDisplay");
        fpGO.transform.SetParent(barGO.transform, false);
        var fpRT = fpGO.AddComponent<RectTransform>();
        fpRT.anchorMin = new Vector2(0f, 0.1f);
        fpRT.anchorMax = new Vector2(0.12f, 0.9f);
        fpRT.offsetMin = new Vector2(8, 0);
        fpRT.offsetMax = Vector2.zero;

        // Coin icon (row 5 col 10 = gold coin)
        var coinSpr = LoadSprite(uiPath, "Sprite sheet for Basic Pack_290");
        if (coinSpr != null)
        {
            var coinIconGO = new GameObject("CoinIcon");
            coinIconGO.transform.SetParent(fpGO.transform, false);
            var coinRT = coinIconGO.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0, 0.15f);
            coinRT.anchorMax = new Vector2(0.35f, 0.85f);
            coinRT.offsetMin = coinRT.offsetMax = Vector2.zero;
            var coinImg = coinIconGO.AddComponent<Image>();
            coinImg.sprite = coinSpr;
            coinImg.preserveAspect = true;
        }

        var fpTxtGO = new GameObject("FPText");
        fpTxtGO.transform.SetParent(fpGO.transform, false);
        var fpTxtRT = fpTxtGO.AddComponent<RectTransform>();
        fpTxtRT.anchorMin = new Vector2(0.35f, 0);
        fpTxtRT.anchorMax = new Vector2(1f, 1f);
        fpTxtRT.offsetMin = fpTxtRT.offsetMax = Vector2.zero;
        var fpTM = fpTxtGO.AddComponent<TextMeshProUGUI>();
        fpTM.text      = "0";
        fpTM.fontSize  = 28;
        fpTM.fontStyle = FontStyles.Bold;
        fpTM.color     = new Color(0.22f, 0.12f, 0.02f);
        fpTM.alignment = TextAlignmentOptions.MidlineLeft;

        var topBarFP = fpGO.AddComponent<TopBarFP>();
        topBarFP.valueText = fpTM;

        // ── Seed Shop button (right side) ──────────────────────────
        var shopBtnGO = new GameObject("ShopButton");
        shopBtnGO.transform.SetParent(barGO.transform, false);
        var shopRT = shopBtnGO.AddComponent<RectTransform>();
        shopRT.anchorMin = new Vector2(0.88f, 0.1f);
        shopRT.anchorMax = new Vector2(0.99f, 0.9f);
        shopRT.offsetMin = shopRT.offsetMax = Vector2.zero;

        // Play button sprite (row 12)
        var playNormal  = LoadSprite(uiPath, "Sprite sheet for Basic Pack_672");
        var playPressed = LoadSprite(uiPath, "Sprite sheet for Basic Pack_728");
        var shopImg = shopBtnGO.AddComponent<Image>();
        if (playNormal != null) { shopImg.sprite = playNormal; shopImg.type = Image.Type.Sliced; }
        else shopImg.color = new Color(0.55f, 0.78f, 0.35f);

        var shopBtn = shopBtnGO.AddComponent<Button>();
        if (playNormal != null && playPressed != null)
        {
            var colors = shopBtn.colors;
            colors.highlightedColor = new Color(1f, 1f, 0.8f);
            colors.pressedColor     = new Color(0.7f, 0.7f, 0.5f);
            shopBtn.colors = colors;
        }

        var shopLblGO = new GameObject("Label");
        shopLblGO.transform.SetParent(shopBtnGO.transform, false);
        shopLblGO.AddComponent<RectTransform>().anchorMin = Vector2.zero;
        shopLblGO.GetComponent<RectTransform>().anchorMax = Vector2.one;
        shopLblGO.GetComponent<RectTransform>().offsetMin = shopLblGO.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        var shopTM = shopLblGO.AddComponent<TextMeshProUGUI>();
        shopTM.text      = "SEEDS";
        shopTM.fontSize  = 24;
        shopTM.fontStyle = FontStyles.Bold;
        shopTM.color     = new Color(0.22f, 0.12f, 0.02f);
        shopTM.alignment = TextAlignmentOptions.Center;

        // Wire UIManager
        var uiMgr = canvasGO.GetComponent<UIManager>();
        if (uiMgr != null)
        {
            uiMgr.seedButton = shopBtn;
        }

        Debug.Log("[RebuildScene] UI built.");
    }

    // ═══════════════════════════════════════════════════════════════
    // Utilities
    // ═══════════════════════════════════════════════════════════════
    static void EnsureDir(string path)
    {
        var parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    static Sprite LoadSprite(string path, string name)
    {
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            if (obj is Sprite s && (name == null || s.name == name)) return s;
        return null;
    }
}
