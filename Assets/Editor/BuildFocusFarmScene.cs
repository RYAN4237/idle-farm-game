using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

/// FOCUS FARM scene rebuild v4 — matches sample_UI.png layout precisely.
/// Camera sees x:0..20, y:-3..7 (ortho=5, pos=(10,2,-10))
///
/// Biom sheet sorted indices (top→bottom, left→right):
///   [00]=tree_TL [01]=tree_TR [02]=mushroom_L [03]=mushroom_R
///   [04]=rock_bigL [05]=rock_bigR [06]=stone_sm
///   [09]=tree_BL [10]=tree_BR [11]=bush_bigL [12]=bush_bigR
///   [13]=bush_round [14]=rock_sm [15]=pumpkin
///   [18]=bush_red [19]=flower_yellow [20]=flower_purple
///   [21]=heart_grass [22]=log [23]=log2 [24]=stump
///   [27]=grass_tuft1 [28]=grass_tuft2 [29]=grass_tuft3 [30]=grass_tuft4
///   [31]=flower_red [32]=flower_blue [33]=pebbles [34]=sunflower
///   [36]=clover1 [37]=clover2 [38]=clover3 [39]=clover4
public class BuildFocusFarmScene
{
    const int X_MIN = -2, X_MAX = 22, Y_MIN = -4, Y_MAX = 8;

    static Sprite[] _biom, _water, _grass, _dirt, _plants;
    static Material _mat;

    [MenuItem("Tools/Build Focus Farm Scene (Precise)")]
    public static void Execute()
    {
        _mat = FindUnlitMat();
        LoadSprites();
        RebuildTilemaps();

        var old = GameObject.Find("Decorations");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("Decorations");

        PlaceTrees(root.transform);
        PlaceBridge(root.transform);
        PlaceRocks(root.transform);
        PlaceBushes(root.transform);
        PlaceCropField(root.transform);

        ApplyUnlit();
        EditorUtility.SetDirty(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[FocusFarm] v4 complete.");
    }

    static void LoadSprites()
    {
        _biom   = SortedSprites("Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Grass Biom things 1.png");
        _water  = SortedSprites("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Water.png");
        _grass  = SortedSprites("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png");
        _dirt   = SortedSprites("Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Tilled Dirt.png");
        _plants = SortedSprites("Assets/Sprout Lands - Sprites - Basic pack/Objects/Basic Plants.png");
        Debug.Log($"[FocusFarm] Loaded: biom={_biom.Length} water={_water.Length} grass={_grass.Length} dirt={_dirt.Length} plants={_plants.Length}");
    }

    static Sprite[] SortedSprites(string path) =>
        AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .OrderBy(s => -s.rect.y).ThenBy(s => s.rect.x).ToArray();

    static Sprite B(int i) => i < _biom.Length   ? _biom[i]   : null;
    static Sprite P(int i) => i < _plants.Length ? _plants[i] : null;
    static Sprite W(int i) => i < _water.Length  ? _water[i]  : null;
    static Sprite G(int i) => i < _grass.Length  ? _grass[i]  : null;
    static Sprite D(int i) => i < _dirt.Length   ? _dirt[i]   : null;

    // ── Tilemaps ───────────────────────────────────────────────────────────

    static void RebuildTilemaps()
    {
        var gridGO = GameObject.Find("Tilemap") ?? new GameObject("Tilemap");
        if (gridGO.GetComponent<Grid>() == null) gridGO.AddComponent<Grid>();
        foreach (Transform c in gridGO.transform) Object.DestroyImmediate(c.gameObject);

        var tmWater  = MakeLayer(gridGO, "Map_Water",  "Background", -10);
        var tmGround = MakeLayer(gridGO, "Map_Ground", "Background",   0);
        var tmDirt   = MakeLayer(gridGO, "Map_Decor",  "Default",      5);

        // Full grass
        var tGrass = Tile(G(0));
        if (tGrass != null)
            for (int x = X_MIN; x <= X_MAX; x++)
            for (int y = Y_MIN; y <= Y_MAX; y++)
                tmGround.SetTile(new Vector3Int(x, y, 0), tGrass);

        // River at y=1,2
        var tWater = Tile(W(0));
        if (tWater != null)
            for (int x = X_MIN; x <= X_MAX; x++)
            {
                tmWater.SetTile(new Vector3Int(x, 1, 0), tWater);
                tmWater.SetTile(new Vector3Int(x, 2, 0), tWater);
                tmGround.SetTile(new Vector3Int(x, 1, 0), null);
                tmGround.SetTile(new Vector3Int(x, 2, 0), null);
            }

        // Tilled dirt: right side x=12..15, y=4..6 (matches reference)
        var tDirt = Tile(D(0));
        if (tDirt != null)
            for (int x = 12; x <= 15; x++)
            for (int y = 4;  y <= 6;  y++)
                tmDirt.SetTile(new Vector3Int(x, y, 0), tDirt);
    }

    // ── Trees ──────────────────────────────────────────────────────────────
    // Oak tree = 2×2 cells: TL=B(0), TR=B(1), BL=B(9), BR=B(10)
    // At scale=3: each cell = 3 world units → tree is 6 wide × 6 tall
    // Root (bottom-center) at (cx, cy), top at (cx, cy+6)

    static void PlaceTrees(Transform root)
    {
        // Left large oak: root at x=1, y=3 → tops at y=9 (partially off-screen is fine)
        Oak(root, "OakTree_L", 1f, 5f, 3f, 9);

        // Right large oak: root at x=19, y=3
        Oak(root, "OakTree_R", 19f, 5f, 3f, 9);

        // Right-center fruit tree (slightly smaller): root at x=16, y=4
        Oak(root, "FruitTree", 15.5f, 4.8f, 2.5f, 8);

        // Left second tree (partially behind first)
        Oak(root, "OakTree_L2", -0.5f, 4.5f, 2.5f, 7);
    }

    static void Oak(Transform parent, string id, float cx, float cy, float scale, int order)
    {
        // 2×2 grid. Cell size = 1 world unit × scale.
        // Layout: TL at (-0.5, +0.5)*scale, TR at (+0.5, +0.5)*scale, etc.
        PlaceGroup(parent, id, new (float dx, float dy, Sprite s, int o)[]
        {
            (-0.5f,  0.5f, B(0),  order),
            ( 0.5f,  0.5f, B(1),  order),
            (-0.5f, -0.5f, B(9),  order),
            ( 0.5f, -0.5f, B(10), order),
        }, cx, cy, scale);
    }

    // ── Bridge ─────────────────────────────────────────────────────────────

    static void PlaceBridge(Transform root)
    {
        var deck  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/FFS_Bridge.png");
        var rails = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/FFS_BridgeFull.png");
        // Bridge crosses river at x≈8..11, y=1..2
        if (deck  != null) SR(root, "Bridge_Deck",  deck,  9f, 1.5f, 12, 1f);
        if (rails != null) SR(root, "Bridge_Rails", rails, 9f, 2.5f, 13, 0.7f);
    }

    // ── Rocks ──────────────────────────────────────────────────────────────

    static void PlaceRocks(Transform root)
    {
        // B(4)=rock_bigL, B(5)=rock_bigR, B(14)=rock_sm, B(6)=stone_sm
        // Right side near upper bank, x=16..18, y=3..4
        SR(root, "Rock_A", B(4),  16f,   3.5f, 6, 2f);
        SR(root, "Rock_B", B(5),  17.2f, 3.2f, 6, 2f);
        SR(root, "Rock_C", B(14), 18f,   3.7f, 6, 1.5f);
        SR(root, "Rock_D", B(6),  17f,   4.2f, 6, 1.3f);
    }

    // ── Bushes & Flowers ───────────────────────────────────────────────────

    static void PlaceBushes(Transform root)
    {
        // Upper bank y=3.3: round bushes B(13), left & right of bridge
        SR(root, "Bush_UL1", B(13), 2f,   3.4f, 5, 2f);
        SR(root, "Bush_UL2", B(13), 4.5f, 3.4f, 5, 2f);
        SR(root, "Bush_UR1", B(13), 11f,  3.4f, 5, 2f);
        SR(root, "Bush_UR2", B(13), 19f,  3.4f, 5, 2f);

        // Lower bank y=0.3: small bushes
        SR(root, "Bush_LL1", B(13), 3f,   0.3f, 5, 1.5f);
        SR(root, "Bush_LL2", B(13), 7f,   0.3f, 5, 1.5f);
        SR(root, "Bush_LR1", B(13), 13f,  0.3f, 5, 1.5f);
        SR(root, "Bush_LR2", B(13), 17f,  0.3f, 5, 1.5f);

        // Flowers on upper grass — B(19)=yellow B(20)=purple B(31)=red B(34)=sunflower
        (float x, float y, Sprite s)[] flowers = {
            (3f,   3.8f, B(19)), (5.5f, 3.6f, B(20)), (7f,   3.9f, B(31)),
            (11.5f,3.7f, B(19)), (20f,  3.8f, B(34)),
            (4f,   0.6f, B(32)), (9f,   0.5f, B(31)), (14f,  0.6f, B(20))
        };
        foreach (var (x, y, s) in flowers)
            SR(root, $"Flower_{x}_{y}", s, x, y, 6, 1.5f);

        // Grass tufts along both banks
        for (int i = 0; i < 8; i++)
        {
            SR(root, $"Tuft_U_{i}", B(27 + i % 4), 1f + i * 2.5f, 3.1f, 4, 1.3f);
            SR(root, $"Tuft_L_{i}", B(27 + i % 4), 1.5f + i * 2.5f, 0.1f, 4, 1.3f);
        }
    }

    // ── Crop Field ─────────────────────────────────────────────────────────

    static void PlaceCropField(Transform root)
    {
        // Over tilled dirt x=12..15, y=4..6
        // P(03)=wheat_sm P(04)=wheat_md P(05)=wheat_lg P(11)=tomato
        Sprite[] stages = {
            P(3), P(4), P(5), P(11),
            P(4), P(11),P(3), P(5),
            P(5), P(3), P(11),P(4)
        };
        int ci = 0;
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
        {
            SR(root, $"Crop_{col}_{row}", stages[ci++ % stages.Length],
               12.5f + col, 4.5f + row, 7, 1f);
        }
    }

    // ── Shared Helpers ─────────────────────────────────────────────────────

    static void SR(Transform parent, string id, Sprite spr,
                   float wx, float wy, int order, float scale = 1f)
    {
        if (spr == null) { Debug.LogWarning($"[FocusFarm] null sprite for {id}"); return; }
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
        (float dx, float dy, Sprite s, int order)[] cells, float cx, float cy, float scale)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        foreach (var (dx, dy, s, order) in cells)
        {
            if (s == null) continue;
            var go = new GameObject($"{id}_c");
            go.transform.SetParent(g.transform, false);
            go.transform.localPosition = new Vector3(dx * scale, dy * scale, 0f);
            go.transform.localScale    = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s; sr.sortingOrder = order;
            if (_mat) sr.sharedMaterial = _mat;
        }
        EditorUtility.SetDirty(g);
    }

    static Tilemap MakeLayer(GameObject grid, string name, string sl, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingLayerName = sl; tr.sortingOrder = order;
        if (_mat) tr.sharedMaterial = _mat;
        return tm;
    }

    static Tile Tile(Sprite s)
    {
        if (s == null) return null;
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = s; return t;
    }

    static void ApplyUnlit()
    {
        if (_mat == null) return;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (sr.GetComponentInParent<Canvas>() != null) continue;
            sr.sharedMaterial = _mat;
        }
        foreach (var tr in Object.FindObjectsByType<TilemapRenderer>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            tr.sharedMaterial = _mat;
    }

    static Material FindUnlitMat()
    {
        foreach (var g in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (p.Contains("com.unity.render-pipelines"))
                return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }
}
