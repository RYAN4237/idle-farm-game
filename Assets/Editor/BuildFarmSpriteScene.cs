using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

/// Build Focus Farm scene using Assets/Farm Sprite.png exclusively.
/// Sheet: 1024x1024, 16x16 grid of 64x64px cells, PPU=64.
/// Sorted: idx = row*16+col, row 0=visual top (highest rect.y), col 0=left.
///
/// CONFIRMED tile indices (from visual preview):
///   TREES:    0,1=tree_top_L/R    16,17=tree_bot_L/R   12,13=tree2_top   28,29=tree2_bot
///   GRASS:    20,21,22,23=flat_green_fill (row1 col4-7)
///   GRASS_DK: 4,5,6,7=darker_grass_edge (row0 col4-7)
///   SAND:     80-87=sand_fill (row5 cols0-7)
///   SNOW/ICE: 96-103=snow (row6)
///   WOOD:     88,89=wood_plank (row5 col8-9)    104,105=wood_dark (row6 col8-9)
///   STONE:    90,91=stone (row5 col10-11)        106,107=stone2 (row6 col10-11)
///   WATER:    use snow row6 (light blue) as river — actual deep blue at row11 col0-1 = idx 176,177
///   DIRT:     18,19=brown_dirt (row1 col2-3)
///
/// Camera: ortho=5, pos=(10,2,-10) → visible x:0..20, y:-3..7
public class BuildFarmSpriteScene
{
    const int X_MIN = -2, X_MAX = 22, Y_MIN = -4, Y_MAX = 8;

    static Sprite[] _fs;
    static Material _mat;

    static Sprite T(int row, int col)
    {
        // Direct lookup by rect position (robust, no sort-order dependency)
        float ry = (15 - row) * 64f;
        float rx = col * 64f;
        var s = _fs.FirstOrDefault(sp => Mathf.Approximately(sp.rect.x, rx) && Mathf.Approximately(sp.rect.y, ry));
        if (s == null) Debug.LogWarning($"[FarmScene] T({row},{col}) not found at rect=({rx},{ry})");
        return s;
    }

    static Sprite TByName(int n)
    {
        // Load by sprite name directly: "Farm Sprite_N"
        return _fs.FirstOrDefault(s => s.name == $"Farm Sprite_{n}");
    }

    static Sprite T(int idx) => (idx >= 0 && idx < _fs.Length) ? _fs[idx] : null;

    [MenuItem("Tools/Build Farm Sprite Scene")]
    public static void Execute()
    {
        _mat = FindUnlitMat();
        _fs  = AssetDatabase.LoadAllAssetsAtPath("Assets/Farm Sprite.png")
                   .OfType<Sprite>()
                   .OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x)
                   .ToArray();

        if (_fs.Length == 0) { Debug.LogError("[FarmScene] No sprites sliced in Farm Sprite.png"); return; }
        Debug.Log($"[FarmScene] Loaded {_fs.Length} tiles from Farm Sprite.png");

        var prev = GameObject.Find("_TilePreview");
        if (prev != null) Object.DestroyImmediate(prev);
        var prevG = GameObject.Find("_GrassPreview");
        if (prevG != null) Object.DestroyImmediate(prevG);
        // Remove old Sprout Lands background objects
        foreach (var name in new[]{"SkyBackground","GroundFill","_GrassFillTest","_GrassPreview","_TilePreview"})
        { var o = GameObject.Find(name); if (o != null) Object.DestroyImmediate(o); }

        RebuildTilemaps();

        var old = GameObject.Find("Decorations");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("Decorations");

        PlaceTrees(root.transform);
        PlaceBridge(root.transform);
        PlaceRocks(root.transform);
        PlaceBushesAndFlowers(root.transform);
        PlaceCropField(root.transform);

        ApplyUnlit();
        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FarmScene] Done! Farm Sprite.png scene built.");
    }

    // ── Tilemaps ──────────────────────────────────────────────────────────

    static void RebuildTilemaps()
    {
        var gridGO = GameObject.Find("Tilemap");
        if (gridGO == null) { gridGO = new GameObject("Tilemap"); gridGO.AddComponent<Grid>(); }
        else if (gridGO.GetComponent<Grid>() == null) gridGO.AddComponent<Grid>();
        foreach (Transform c in gridGO.transform) Object.DestroyImmediate(c.gameObject);

        var tmGround = MakeLayer(gridGO, "Map_Ground",  0);
        var tmWater  = MakeLayer(gridGO, "Map_Water",   2);
        var tmEdge   = MakeLayer(gridGO, "Map_Edge",    3);
        var tmDecor  = MakeLayer(gridGO, "Map_Decor",   5);

        // Grass fill: Farm Sprite_0 = rect(0,960) = visual top-left = solid green
        var tGrass = MakeTile(TByName(0));
        if (tGrass == null) Debug.LogError("[FarmScene] Grass tile null!");
        if (tGrass != null)
            for (int x = X_MIN; x <= X_MAX; x++)
            for (int y = Y_MIN; y <= Y_MAX; y++)
                tmGround.SetTile(new Vector3Int(x, y, 0), tGrass);

        // River: Farm Sprite_177 = rect(64,256) = confirmed blue water
        var tWater = MakeTile(TByName(177));
        if (tWater == null) Debug.LogError("[FarmScene] Water tile null!");
        if (tWater != null)
            for (int x = X_MIN; x <= X_MAX; x++)
            {
                tmWater.SetTile(new Vector3Int(x, 1, 0), tWater);
                tmWater.SetTile(new Vector3Int(x, 2, 0), tWater);
                tmGround.SetTile(new Vector3Int(x, 1, 0), null);
                tmGround.SetTile(new Vector3Int(x, 2, 0), null);
            }

        // 3. Bank edges: use same grass fill — no special edge tile needed
        // (remove extra edge layer to keep it clean)

        // 4. Tilled dirt field handled by Decoration GameObjects (PlaceCropField)
        // Skipping tilemap fill to avoid wrong tile display
    }

    // ── Trees ─────────────────────────────────────────────────────────────
    // 2×2 tile assembly at scale 3 → each cell = 3 world units
    // Top pair:    idx  0 (TL),  1 (TR)
    // Bottom pair: idx 16 (BL), 17 (BR)
    // Second tree: idx 12 (TL), 13 (TR), 28 (BL), 29 (BR)

    static void PlaceTrees(Transform root)
    {
        Oak(root, "OakTree_L",  1f,    5.5f, 3f,   9, 0);
        Oak(root, "OakTree_R",  19f,   5.5f, 3f,   9, 0);
        Oak(root, "FruitTree",  15f,   5f,   2.5f, 8, 0);  // use same green tiles
        Oak(root, "OakTree_L2", -0.5f, 4.5f, 2.5f, 7, 0);
    }

    static void Oak(Transform parent, string id, float cx, float cy,
                    float scale, int order, int topLeftIdx)
    {
        PlaceGroup(parent, id, new (float dx, float dy, int idx, int o)[]
        {
            (-0.5f,  0.5f, topLeftIdx,      order),
            ( 0.5f,  0.5f, topLeftIdx + 1,  order),
            (-0.5f, -0.5f, topLeftIdx + 16, order),
            ( 0.5f, -0.5f, topLeftIdx + 17, order),
        }, cx, cy, scale);
    }

    // ── Bridge ────────────────────────────────────────────────────────────
    // Brown wood tiles: right side of sprite sheet, col 12-15
    //   Farm Sprite_12  = rect(768,960) = row0 col12 = brown wood vertical grain
    //   Farm Sprite_13  = rect(832,960) = row0 col13
    //   Farm Sprite_28  = rect(768,832) = row2 col12 = mid-plank
    //   Farm Sprite_44  = rect(768,704) = row4 col12 = darker plank edge

    static void PlaceBridge(Transform root)
    {
        var g = new GameObject("Bridge");
        g.transform.SetParent(root, false);
        g.transform.position = new Vector3(10f, 1.5f, 0f);

        // Brown wood is in lower-right of sheet (rows 7-15 cols 12-15)
        // Farm Sprite_124 = rect(768,448) = row7 col12 = brown wood
        // Farm Sprite_125..127 = col13-15 same row
        var w0 = TByName(124);
        var w1 = TByName(125);
        var w2 = TByName(126);
        var w3 = TByName(127);
        Sprite[] wSprites = { w0, w1, w2, w3 };

        for (int i = 0; i < 5; i++)
        {
            var ws = wSprites[i % 4];
            SRLocal(g.transform, $"DeckBot_{i}", ws, i - 2f, -0.5f, 12);
            SRLocal(g.transform, $"DeckTop_{i}", ws, i - 2f,  0.5f, 12);
            SRLocal(g.transform, $"DeckExt_{i}", ws, i - 2f,  1.5f, 12);
        }

        EditorUtility.SetDirty(g);
    }

    // ── Rocks ─────────────────────────────────────────────────────────────
    // Rock sprites: row8 col6 = idx 134 (small rocks on sand), row9 col5 = idx 149

    static void PlaceRocks(Transform root)
    {
        SR(root, "Rock_A", TByName(134), 16.5f, 4.2f, 6, 1.0f);
        SR(root, "Rock_B", TByName(134), 17.6f, 3.8f, 6, 0.8f);
        SR(root, "Rock_C", TByName(149), 15.8f, 3.6f, 6, 0.9f);
        SR(root, "Rock_D", TByName(134), 6.5f, 0.3f, 5, 0.7f);
        SR(root, "Rock_E", TByName(149), 14f, 0.4f, 5, 0.7f);
    }

    // ── Bushes & Flowers ──────────────────────────────────────────────────
    // Dark grass tiles (idx 4-7) as bush stand-ins on bank edges
    // Sand tufts (idx 117 = sandy-grass tuft) as flowers

    static void PlaceBushesAndFlowers(Transform root)
    {
        // Upper bank bushes: dark grass tile at y=3.5
        int[] bushIdx = { 4, 5, 6, 7 };
        (float x, float y, int bi)[] bushes = {
            (2f,   3.6f, 0), (4.5f, 3.6f, 1), (6.5f, 3.6f, 0),
            (11f,  3.6f, 2), (13.5f,3.6f, 1), (19f,  3.6f, 3)
        };
        foreach (var (x, y, bi) in bushes)
            SR(root, $"Bush_{x}", T(bushIdx[bi]), x, y, 5, 1.8f);

        // Lower bank
        (float x, float y)[] lBushes = { (3f, 0.4f), (7f, 0.4f), (13f, 0.4f), (17f, 0.4f) };
        foreach (var (x, y) in lBushes)
            SR(root, $"BushL_{x}", T(4), x, y, 5, 1.4f);

        // Flowers/plants along banks — use sand tufts idx 117 (sandy grass)
        // and grass variation tiles idx 20-23
        (float x, float y, int idx)[] flowers = {
            (3.5f, 3.9f, 20), (5.5f, 3.8f, 21), (8f,   3.9f, 22), (11.5f, 3.8f, 23),
            (20f,  3.9f, 20), (4f,   0.7f, 21),  (9f,   0.7f, 22), (14f,   0.7f, 23)
        };
        foreach (var (x, y, idx) in flowers)
            SR(root, $"Flower_{x}", T(idx), x, y, 6, 1.4f);
    }

    // ── Crop Field ────────────────────────────────────────────────────────
    // Sand tiles: row7 idx 112-119 = pure golden sand (best for tilled field)
    // Row8 idx 128-135 = sand with grass/rock variants

    static void PlaceCropField(Transform root)
    {
        // Sand tiles: 112-119 = pure golden sand
        int[] sandIdx = { 112, 113, 114, 115, 116, 113, 114, 112 };
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 7; col++)
            SR(root, $"Crop_{col}_{row}", TByName(sandIdx[(row*7+col) % sandIdx.Length]), 9f + col, 4.5f + row, 4);

        // Dry plant tufts: 147,148 = dry grass/plant tufts
        (float x, float y, int n)[] tufts = {
            (10f, 5.0f, 147), (12f, 5.5f, 148), (14f, 4.7f, 147),
            (9.5f, 6.0f, 148), (15f, 5.8f, 147), (11f, 6.3f, 148)
        };
        foreach (var (x, y, n) in tufts)
            SR(root, $"Tuft_{x}", TByName(n), x, y, 6, 0.9f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    static void SRLocal(Transform parent, string id, Sprite spr,
                         float lx, float ly, int order, float scale = 1f)
    {
        if (spr == null) { Debug.LogWarning($"[FarmScene] null sprite '{id}'"); return; }
        var go = new GameObject(id);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(lx, ly, 0f);
        go.transform.localScale    = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
        EditorUtility.SetDirty(go);
    }

    static void SR(Transform parent, string id, Sprite spr,
                   float wx, float wy, int order, float scale = 1f)
    {
        if (spr == null) { Debug.LogWarning($"[FarmScene] null sprite '{id}'"); return; }
        var go = new GameObject(id);
        go.transform.SetParent(parent, false);
        go.transform.position   = new Vector3(wx, wy, 0f);
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
        EditorUtility.SetDirty(go);
    }

    static void PlaceGroup(Transform parent, string id,
        (float dx, float dy, int idx, int order)[] cells,
        float cx, float cy, float scale)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        foreach (var (dx, dy, idx, order) in cells)
        {
            var spr = T(idx);
            if (spr == null) { Debug.LogWarning($"[FarmScene] {id}: null at idx {idx}"); continue; }
            var go = new GameObject($"{id}_{idx}");
            go.transform.SetParent(g.transform, false);
            go.transform.localPosition = new Vector3(dx * scale, dy * scale, 0f);
            go.transform.localScale    = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = spr; sr.sortingOrder = order;
            if (_mat) sr.sharedMaterial = _mat;
        }
        EditorUtility.SetDirty(g);
    }

    static Tilemap MakeLayer(GameObject grid, string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingLayerName = "Default";
        tr.sortingOrder = order;
        if (_mat) tr.sharedMaterial = _mat;
        return tm;
    }

    static UnityEngine.Tilemaps.Tile MakeTile(Sprite s)
    {
        if (s == null) return null;
        string dir = "Assets/Tiles";
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
        string assetPath = $"{dir}/{s.name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(assetPath);
        if (existing != null) { existing.sprite = s; EditorUtility.SetDirty(existing); return existing; }
        var t = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        t.sprite = s;
        t.name = s.name;
        AssetDatabase.CreateAsset(t, assetPath);
        return t;
    }

    static void ApplyUnlit()
    {
        if (_mat == null) return;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
        {
            if (sr.GetComponentInParent<Canvas>() != null) continue;
            sr.sharedMaterial = _mat;
        }
        foreach (var tr in Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include))
            tr.sharedMaterial = _mat;
    }

    static Material FindUnlitMat()
    {
        foreach (var g in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (p.Contains("com.unity.render-pipelines")) return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }
}
