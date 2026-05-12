using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

/// Rebuilds background of Farm.unity to match sample_UI.png reference.
/// Confirmed sprite indices from visual inspection:
///   idx 0-9:   green grass tiles (row 0)
///   idx 16-25: green grass variants with flowers (row 1)
///   idx 32-39: grass-to-dirt edge tiles (row 2) -- great for "tree canopy edge"
///   idx 48-55: dirt/soil tiles (row 3)
///   idx 112-119: sandy soil (row 7)
///   idx 124-127: brown wood planks (row 7 cols 12-15)
///   idx 128-137: sandy (row 8 cols 0-9), gray rocks cols 11+ 
///   idx 139-143: gray rocks (row 8 cols 11-15)
///   idx 144-153: sandy+plants (row 9), idx 148=CACTUS, idx 149=dry plant
///   idx 155-159: gray rocks (row 9 cols 11-15)
///   idx 176-185: blue water (row 11 cols 0-9)
///   idx 192-194: blue water (row 12 cols 0-2)
public class BuildSceneBG
{
    const string SHEET = "Assets/Farm Sprite.png";
    const string TILE_DIR = "Assets/Tiles";
    const int X_MIN = -2, X_MAX = 22, Y_MIN = -3, Y_MAX = 10;

    static Sprite[] _fs;
    static Material _mat;
    static Sprite N(int idx) => _fs.FirstOrDefault(s => s.name == $"Farm Sprite_{idx}");

    [MenuItem("Tools/Build Scene Background")]
    public static void Execute()
    {
        _mat = FindUnlitMat();
        _fs = AssetDatabase.LoadAllAssetsAtPath(SHEET).OfType<Sprite>().ToArray();
        if (_fs.Length == 0) { Debug.LogError("[BG] No sprites loaded"); return; }

        if (!Directory.Exists(TILE_DIR)) Directory.CreateDirectory(TILE_DIR);

        // Clean old objects
        foreach (var n in new[] { "Decorations", "Tilemap", "SkyBG", "BGGrid",
                                   "_TilePreview", "_GrassPreview", "_SpritePreview",
                                   "_SpritePreview2", "EventSystem" })
        {
            var o = GameObject.Find(n);
            if (o != null) Object.DestroyImmediate(o);
        }

        BuildSkyBG();

        var gridGO = new GameObject("Tilemap");
        gridGO.AddComponent<Grid>();
        var tmGround = MakeLayer(gridGO, "Map_Ground", 0);
        var tmWater  = MakeLayer(gridGO, "Map_Water",  1);

        // Grass tiles
        var tG = new[] { MakeTile(N(0)), MakeTile(N(1)), MakeTile(N(2)),
                         MakeTile(N(16)), MakeTile(N(17)), MakeTile(N(20)) };

        for (int x = X_MIN; x <= X_MAX; x++)
        for (int y = Y_MIN; y <= Y_MAX; y++)
        {
            int v = ((x * 7 + y * 13) & 0x7);
            bool upper = y >= 3;
            tmGround.SetTile(new Vector3Int(x, y, 0),
                tG[upper ? (v % 3 + 3) : (v % 3)]);
        }

        // Water river: y=0..3 (4 tiles wide)
        var tW = new[] { MakeTile(N(176)), MakeTile(N(177)), MakeTile(N(192)) };
        for (int x = X_MIN; x <= X_MAX; x++)
        {
            int v = ((x * 3) & 3);
            tmWater.SetTile(new Vector3Int(x, 0, 0), tW[v % 3]);
            tmWater.SetTile(new Vector3Int(x, 1, 0), tW[(v + 1) % 3]);
            tmWater.SetTile(new Vector3Int(x, 2, 0), tW[v % 3]);
            tmWater.SetTile(new Vector3Int(x, 3, 0), tW[(v + 2) % 3]);
        }

        var root = new GameObject("Decorations");

        // Large trees LEFT
        PlaceLargeTree(root.transform, "TreeL1",  0.5f,  8.5f);
        PlaceLargeTree(root.transform, "TreeL2", -1.5f,  7.0f);
        PlaceLargeTree(root.transform, "TreeL3",  3.0f,  7.8f);
        PlaceLargeTree(root.transform, "TreeL4",  1.5f,  6.2f);

        // Large trees RIGHT
        PlaceLargeTree(root.transform, "TreeR1", 19.5f,  8.5f);
        PlaceLargeTree(root.transform, "TreeR2", 21.5f,  7.0f);
        PlaceLargeTree(root.transform, "TreeR3", 17.5f,  7.8f);

        // Fruit tree RIGHT-CENTER
        PlaceFruitTree(root.transform, "FruitTree", 15.5f, 7.2f);

        PlaceBridge(root.transform);
        PlaceRocks(root.transform);
        PlaceCropPatch(root.transform);
        PlaceRiverBankDecor(root.transform);

        ApplyUnlit();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[BG] Done!");
    }

    static void BuildSkyBG()
    {
        var go = new GameObject("SkyBG");
        go.transform.position = new Vector3(10f, 7f, 2f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -100;
        var tex = new Texture2D(4, 4);
        Color sky = new Color(0.59f, 0.82f, 0.93f);
        for (int i = 0; i < 16; i++) tex.SetPixel(i % 4, i / 4, sky);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);
        go.transform.localScale = new Vector3(34f, 22f, 1f);
        if (_mat) sr.sharedMaterial = _mat;
    }

    // Large tree: uses grass-edge tiles (idx 32-39) which have green top and brown soil bottom
    // Assembled 2x2 with large scale creates "leafy tree canopy" effect
    // Trunk: soil tile (idx 48) narrowed below canopy
    static void PlaceLargeTree(Transform parent, string id, float cx, float cy)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        float sc = 3.2f;
        // 2x2 canopy using grass-edge tiles (different variants for variety)
        PlacePart(g.transform, "TL", N(32), -sc*0.5f,  sc*0.5f, sc, 9);
        PlacePart(g.transform, "TR", N(35),  sc*0.5f,  sc*0.5f, sc, 9);
        PlacePart(g.transform, "BL", N(36), -sc*0.5f, -sc*0.5f, sc, 9);
        PlacePart(g.transform, "BR", N(39),  sc*0.5f, -sc*0.5f, sc, 9);
        // Trunk: use brown wood plank tile (idx 124 = confirmed brown)
        PlacePart(g.transform, "T1",  N(124),  0f, -sc*1.0f, sc*0.4f, 8);
    }

    // Smaller fruit tree: green canopy + warm-toned center piece
    static void PlaceFruitTree(Transform parent, string id, float cx, float cy)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        float sc = 2.4f;
        // Green canopy tiles (grass variants with slight flower pattern = looks like fruit)
        PlacePart(g.transform, "TL", N(20), -sc*0.5f,  sc*0.5f, sc, 9);
        PlacePart(g.transform, "TR", N(21),  sc*0.5f,  sc*0.5f, sc, 9);
        PlacePart(g.transform, "BL", N(22), -sc*0.5f, -sc*0.5f, sc, 9);
        PlacePart(g.transform, "BR", N(23),  sc*0.5f, -sc*0.5f, sc, 9);
        PlacePart(g.transform, "T1",  N(125),  0f, -sc*1.1f, sc*0.35f, 8);
    }

    // Bridge: crosses river y=0..3, centered at x=10
    static void PlaceBridge(Transform parent)
    {
        var g = new GameObject("Bridge");
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(10f, 1.5f, 0f);
        Sprite[] ws = { N(124), N(125), N(126), N(127) };
        for (int col = 0; col < 5; col++)
        for (int row = 0; row < 5; row++)
            SRLocal(g.transform, $"P{col}_{row}", ws[(col + row) % 4],
                (col - 2) * 1.0f, (row - 2.0f) * 1.0f, 12);
    }

    // Rocks: gray stone tiles (row 8-9 cols 11-15 = idx 139-143, 155-159)
    static void PlaceRocks(Transform parent)
    {
        int[] ri = { 139, 140, 141, 155, 156, 157 };
        (float x, float y, float sc, int rIdx)[] rocks = {
            (13.8f, 4.8f, 1.5f, 0), (14.8f, 5.3f, 2.0f, 1), (15.8f, 4.6f, 1.3f, 2),
            (13.3f, 4.3f, 1.1f, 3), (16.5f, 5.1f, 1.2f, 4), (15.3f, 3.9f, 0.9f, 5),
            ( 6.8f, 3.8f, 0.9f, 1), ( 7.5f, 4.3f, 1.2f, 2),
        };
        foreach (var (x, y, sc, rIdx) in rocks)
            SR(parent, $"Rock_{x}", N(ri[rIdx]), x, y, 6, sc);
    }

    // Crop patch: rows 2-3 have grass-to-soil edge tiles that look like tilled crops
    // idx 32-39 = grass/soil edges; idx 128-137 = sandy soil = crop rows
    // For actual crop plants: idx 144-147 (row 9 cols 0-3: dry grass/plant tufts)
    static void PlaceCropPatch(Transform parent)
    {
        var g = new GameObject("CropPatch");
        g.transform.SetParent(parent, false);

        // Crop rows: tilled soil (idx 52-56, brown dirt) + dry grass tufts on top (idx 163-167)
        int[] soilIdx  = { 52, 53, 54, 55, 56 };
        int[] plantIdx = { 163, 164, 165, 166, 167 };
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 5; col++)
        {
            SR(g.transform, $"Soil_{col}_{row}",  N(soilIdx[col % soilIdx.Length]),
                9.0f + col * 1.0f, 5.5f + row * 0.9f, 4, 1.0f);
            SR(g.transform, $"Plant_{col}_{row}", N(plantIdx[(col + row) % plantIdx.Length]),
                9.0f + col * 1.0f, 5.6f + row * 0.9f, 5, 0.95f);
        }
    }

    // River bank decorations
    static void PlaceRiverBankDecor(Transform parent)
    {
        var g = new GameObject("BankDecor");
        g.transform.SetParent(parent, false);

        // Upper bank bushes: use grass-edge tiles (idx 32-35) as bush clumps
        (float x, float y, int idx, float sc)[] upper = {
            (1.5f, 4.5f, 32, 1.5f), (3.5f, 4.3f, 33, 1.3f),
            (6.0f, 4.6f, 34, 1.6f), (12.5f, 4.4f, 35, 1.4f),
            (17.5f, 4.5f, 32, 1.5f), (20.0f, 4.3f, 33, 1.3f),
        };
        foreach (var (x, y, idx, sc) in upper)
            SR(g.transform, $"UBush_{x}", N(idx), x, y, 5, sc);

        // Lower bank: small grass tufts
        (float x, float y)[] lower = {
            (2.0f, -0.5f), (5.5f, -0.4f), (8.5f, -0.5f),
            (12.0f, -0.4f), (15.5f, -0.5f), (19.0f, -0.4f),
        };
        foreach (var (x, y) in lower)
            SR(g.transform, $"LBush_{x}", N(1), x, y, 4, 1.2f);

        // Flowers on upper grass
        int[] fi = { 20, 21, 22, 23 };
        (float x, float y)[] fpos = {
            (4.0f,6.0f),(7.5f,5.5f),(11.0f,6.5f),(13.5f,5.8f),(18.0f,6.2f),(2.5f,7.5f)
        };
        for (int i = 0; i < fpos.Length; i++)
            SR(g.transform, $"Fl_{i}", N(fi[i % 4]), fpos[i].x, fpos[i].y, 6, 1.2f);
    }

    static void PlacePart(Transform p, string id, Sprite spr, float lx, float ly, float sc, int order)
    {
        if (spr == null) { Debug.LogWarning($"[BG] null sprite {id}"); return; }
        var go = new GameObject(id);
        go.transform.SetParent(p, false);
        go.transform.localPosition = new Vector3(lx, ly, 0f);
        go.transform.localScale    = Vector3.one * sc;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
    }

    static void SR(Transform p, string id, Sprite spr, float wx, float wy, int order, float scale = 1f)
    {
        if (spr == null) return;
        var go = new GameObject(id);
        go.transform.SetParent(p, false);
        go.transform.position   = new Vector3(wx, wy, 0f);
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
    }

    static void SRLocal(Transform p, string id, Sprite spr, float lx, float ly, int order, float scale = 1f)
    {
        if (spr == null) return;
        var go = new GameObject(id);
        go.transform.SetParent(p, false);
        go.transform.localPosition = new Vector3(lx, ly, 0f);
        go.transform.localScale    = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spr; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
    }

    static Tilemap MakeLayer(GameObject grid, string name, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(grid.transform, false);
        var tm = go.AddComponent<Tilemap>();
        var tr = go.AddComponent<TilemapRenderer>();
        tr.sortingLayerName = "Default"; tr.sortingOrder = order;
        if (_mat) tr.sharedMaterial = _mat;
        return tm;
    }

    static UnityEngine.Tilemaps.Tile MakeTile(Sprite s)
    {
        if (s == null) return null;
        string path = $"{TILE_DIR}/{s.name}.asset";
        var ex = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(path);
        if (ex != null) { ex.sprite = s; EditorUtility.SetDirty(ex); return ex; }
        var t = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        t.sprite = s; t.name = s.name; AssetDatabase.CreateAsset(t, path); return t;
    }

    static void ApplyUnlit()
    {
        if (_mat == null) return;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
            if (sr.GetComponentInParent<Canvas>() == null) sr.sharedMaterial = _mat;
        foreach (var tr in Object.FindObjectsByType<TilemapRenderer>(FindObjectsInactive.Include))
            tr.sharedMaterial = _mat;
    }

    static Material FindUnlitMat()
    {
        foreach (var guid in AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material"))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.Contains("com.unity.render-pipelines")) return AssetDatabase.LoadAssetAtPath<Material>(p);
        }
        return null;
    }
}
