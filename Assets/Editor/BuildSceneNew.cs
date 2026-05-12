using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

/// Rebuilds Farm.unity background using FarmBG_Ground + FarmBG_Deco sprite sheets.
/// Verified tile contents:
///   Ground row0: GRASS (cols 0-4, 6-7; skip col5=purple)
///   Ground row1: WOOD/SOIL (all cols)
///   Ground row2: WATER (all cols)
///   Ground row3: water-edge/sand mix
///   Ground row5: WOOD bridge planks
///   Deco row0-1: GRASS = tree canopy (green blocks, assembled 2x2)
///   Deco row2:   ROCK
///   Deco row3:   WOOD+GRASS = crops
///   Deco row5:   WOOD col0,1 = fence rails
public class BuildSceneNew
{
    const string GND      = "Assets/Sprites/FarmBG_Ground.png";
    const string DECO     = "Assets/Sprites/FarmBG_Deco.png";
    const string TILE_DIR = "Assets/Tiles2";

    static Sprite[] _gnd, _dco;
    static Material _mat;

    static Sprite G(int row, int col) => _gnd.FirstOrDefault(s => s.name == $"FarmBG_Ground_{row*8+col}");
    static Sprite D(int row, int col) => _dco.FirstOrDefault(s => s.name == $"FarmBG_Deco_{row*8+col}");

    [MenuItem("Tools/Build Scene New")]
    public static void Execute()
    {
        _mat = FindUnlitMat();
        _gnd = AssetDatabase.LoadAllAssetsAtPath(GND).OfType<Sprite>().ToArray();
        _dco = AssetDatabase.LoadAllAssetsAtPath(DECO).OfType<Sprite>().ToArray();
        Debug.Log($"[BuildSceneNew] Ground={_gnd.Length} Deco={_dco.Length} mat={_mat?.name}");

        // Camera background = sky blue
        var cam = Object.FindAnyObjectByType<Camera>();
        if (cam != null) cam.backgroundColor = new Color(0.53f, 0.80f, 0.92f);

        // Clean old objects
        foreach (var n in new[]{ "Decorations","Tilemap","BGGrid","SkyBG",
                                  "_SpritePreview","_SpritePreview2","_SPSpecific","BankDecor","CropPatch" })
        { var o = GameObject.Find(n); if (o) Object.DestroyImmediate(o); }

        if (!Directory.Exists(TILE_DIR)) Directory.CreateDirectory(TILE_DIR);

        // Grid setup
        var grid = new GameObject("Tilemap");
        grid.AddComponent<Grid>().cellSize = Vector3.one;
        var tmGnd   = MakeLayer(grid, "Map_Ground", 0);
        var tmWater = MakeLayer(grid, "Map_Water",  1);

        FillGrass(tmGnd);
        FillWater(tmWater);

        var root = new GameObject("Decorations");
        PlaceTrees(root.transform);
        PlaceBridge(root.transform);
        PlaceRocks(root.transform);
        PlaceCrops(root.transform);

        ApplyUnlit();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[BuildSceneNew] Complete!");
    }

    // Camera center (10,2), ortho size 5 → visible x:0..20, y:-3..7
    // River at y=0..3. Upper grass y=4..10. Lower grass y=-4..-1.
    static void FillGrass(Tilemap tm)
    {
        // Use only cols 0-3 (plain grass, no flower variants)
        for (int x = -2; x <= 22; x++)
        for (int y = -4; y <= 12; y++)
        {
            if (y >= 0 && y <= 3) continue; // river zone
            int col = ((x * 7 + y * 13) & 0xFF) % 4; // 0-3 only
            var sp = G(0, col);
            if (sp == null) continue;
            tm.SetTile(new Vector3Int(x, y, 0), MakeTile(sp));
        }
    }

    // River: y=0 lower-bank, y=1-2 main water, y=3 upper-bank
    static void FillWater(Tilemap tm)
    {
        for (int x = -2; x <= 22; x++)
        {
            // Lower bank (sandy): row3 col2-3
            tm.SetTile(new Vector3Int(x, 0, 0), MakeTile(G(3, 2 + (x&1))));
            // Water main body: row2
            int wv = (x*3) & 0x7;
            tm.SetTile(new Vector3Int(x, 1, 0), MakeTile(G(2, wv)));
            tm.SetTile(new Vector3Int(x, 2, 0), MakeTile(G(2, (wv+3)%8)));
            // Upper bank (sandy): row3 col0-1
            tm.SetTile(new Vector3Int(x, 3, 0), MakeTile(G(3, x&1)));
        }
    }

    // ── Trees ─────────────────────────────────────────────────────────────
    // Reference: large oaks left cluster, 1-2 oaks right, fruit tree right-center
    // Trees sit on upper grass (y≥4), assembled from Deco row0+row1 (2x2)
    static void PlaceTrees(Transform parent)
    {
        // Left cluster
        PlaceTree(parent, "OakL1",  1.5f,  9.0f, 3.5f, false);
        PlaceTree(parent, "OakL2", -0.5f,  7.5f, 2.6f, false);
        PlaceTree(parent, "OakL3",  4.5f,  8.5f, 2.8f, false);

        // Right cluster
        PlaceTree(parent, "OakR1", 19.5f,  9.0f, 3.5f, false);
        PlaceTree(parent, "OakR2", 21.5f,  7.5f, 2.6f, false);

        // Mid-right trees
        PlaceTree(parent, "OakMR", 17.0f,  7.5f, 2.4f, false);

        // Fruit tree: use Deco cols 2-3 (same green but identified as fruit in gen script)
        PlaceTree(parent, "Fruit", 14.5f,  7.5f, 2.2f, true);
    }

    static void PlaceTree(Transform parent, string id, float cx, float cy, float sc, bool fruit)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        int dc = fruit ? 2 : 0;

        // 2x2 canopy from Deco row0/row1
        PlacePart(g.transform, "TL", D(0, dc),   -sc*0.5f,  sc*0.5f, sc, 10);
        PlacePart(g.transform, "TR", D(0, dc+1),  sc*0.5f,  sc*0.5f, sc, 10);
        PlacePart(g.transform, "BL", D(1, dc),   -sc*0.5f, -sc*0.5f, sc, 10);
        PlacePart(g.transform, "BR", D(1, dc+1),  sc*0.5f, -sc*0.5f, sc, 10);

        // Trunk: use a WOOD tile (Ground row1 col0) below canopy
        PlacePart(g.transform, "Trunk", G(1, 0), 0f, -sc*1.1f, sc*0.45f, 9);
    }

    // ── Bridge ────────────────────────────────────────────────────────────
    // Bridge crosses river horizontally at x≈9. River y=0..3 (4 tiles tall).
    // Planks: Ground row5 (WOOD) tiled across the river span (y=0..3) at a fixed x column.
    // Scale planks wider (scaleX>1) to look like horizontal planks crossing the river.
    static void PlaceBridge(Transform parent)
    {
        var g = new GameObject("Bridge");
        g.transform.SetParent(parent, false);

        // Bridge deck: 3-tile wide column of planks at x=9, spanning river y=0..3
        for (int bx = -1; bx <= 1; bx++)
        for (int by = 0; by <= 3; by++)
        {
            var sp = G(5, (by) % 4);
            if (sp == null) continue;
            var go = new GameObject($"Pl_{bx}_{by}");
            go.transform.SetParent(g.transform, false);
            go.transform.position   = new Vector3(10f + bx, (float)by, 0);
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp; sr.sortingOrder = 12;
            if (_mat) sr.sharedMaterial = _mat;
        }
        // Fence posts left and right sides of bridge (x=8.5 and x=11.5, spanning y=0..3)
        for (int by = 0; by <= 3; by++)
        {
            var sp = D(5, 0);
            SR(g.transform, $"PostL{by}", sp, 8.4f, (float)by, 13, 0.8f);
            SR(g.transform, $"PostR{by}", sp, 11.6f, (float)by, 13, 0.8f);
        }
    }

    // ── Rocks ─────────────────────────────────────────────────────────────
    // Deco row2: all ROCK (cols 0-5 have content)
    static void PlaceRocks(Transform parent)
    {
        (float x, float y, int c, float sc)[] rocks = {
            (14.5f, 5.0f, 0, 1.8f), (15.5f, 5.4f, 1, 2.0f), (16.5f, 4.9f, 2, 1.5f),
            (13.5f, 4.8f, 3, 1.4f), (17.2f, 4.6f, 4, 1.3f),
            ( 5.5f, 4.8f, 0, 1.4f), ( 6.5f, 5.1f, 1, 1.7f),
        };
        foreach (var (x, y, c, sc) in rocks)
            SR(parent, $"Rock_{x}", D(2, c % 6), x, y, 6, sc);
    }

    // ── Crops ─────────────────────────────────────────────────────────────
    // Deco row3: WOOD+GRASS = crop plants. Ground row1 = soil base.
    static void PlaceCrops(Transform parent)
    {
        var g = new GameObject("Crops");
        g.transform.SetParent(parent, false);
        for (int cx = 0; cx < 5; cx++)
        for (int cy = 0; cy < 3; cy++)
        {
            float wx = 11.0f + cx;
            float wy = 5.3f + cy * 0.9f;
            SR(g.transform, $"Soil_{cx}_{cy}", G(1, cx%4), wx, wy, 4, 1.0f);
            SR(g.transform, $"Plant_{cx}_{cy}", D(3, (cx+cy*2)%8), wx, wy, 5, 1.0f);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────
    static void PlacePart(Transform p, string id, Sprite sp, float lx, float ly, float sc, int order)
    {
        if (!sp) { Debug.LogWarning($"[BuildSceneNew] null {id}"); return; }
        var go = new GameObject(id);
        go.transform.SetParent(p, false);
        go.transform.localPosition = new Vector3(lx, ly, 0);
        go.transform.localScale    = Vector3.one * sc;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
    }

    static void SR(Transform p, string id, Sprite sp, float wx, float wy, int order, float sc=1f)
    {
        if (!sp) return;
        var go = new GameObject(id);
        go.transform.SetParent(p, false);
        go.transform.position   = new Vector3(wx, wy, 0);
        go.transform.localScale = Vector3.one * sc;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
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
        if (!s) return null;
        string path = $"{TILE_DIR}/{s.name}.asset";
        var ex = AssetDatabase.LoadAssetAtPath<UnityEngine.Tilemaps.Tile>(path);
        if (ex != null) { ex.sprite = s; EditorUtility.SetDirty(ex); return ex; }
        var t = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
        t.sprite = s; t.name = s.name;
        AssetDatabase.CreateAsset(t, path);
        return t;
    }

    static void ApplyUnlit()
    {
        if (_mat == null) return;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
            if (!sr.GetComponentInParent<Canvas>()) sr.sharedMaterial = _mat;
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
