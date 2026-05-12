using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;

/// Rebuilds Farm.unity using IsoFarm_Ground + IsoFarm_Deco sprite sheets.
/// Tile size = 48px, PPU=48. Camera ortho=5, pos=(10,2).
/// IsoFarm_Ground (8x6): row0=grass, row1=soil, row2=water, row3=sand, row4=transitions, row5=bridge
/// IsoFarm_Deco   (8x12): row0-1=oak(2x2), row2-3=fruit(2x2)+bushes, row4=rocks, row5=crops,
///                         row6=fence, row7=tufts, row8=bridge rails, row9=clouds, row10=sky, row11=extras
public class BuildSceneIso
{
    const string GND      = "Assets/Sprites/IsoFarm_Ground.png";
    const string DCO      = "Assets/Sprites/IsoFarm_Deco.png";
    const string TILE_DIR = "Assets/Tiles3";

    static Sprite[] _gnd, _dco;
    static Material _mat;

    static Sprite G(int row, int col) => _gnd.FirstOrDefault(s => s.name == $"IsoGnd_{row*8+col}");
    static Sprite D(int row, int col) => _dco.FirstOrDefault(s => s.name == $"IsoDco_{row*8+col}");

    [MenuItem("Tools/Build Scene Iso")]
    public static void Execute()
    {
        _mat = FindUnlitMat();
        _gnd = AssetDatabase.LoadAllAssetsAtPath(GND).OfType<Sprite>().ToArray();
        _dco = AssetDatabase.LoadAllAssetsAtPath(DCO).OfType<Sprite>().ToArray();
        Debug.Log($"[BuildIso] Ground={_gnd.Length} Deco={_dco.Length} mat={_mat?.name}");

        // Camera: sky blue background
        var cam = Object.FindAnyObjectByType<Camera>();
        if (cam != null) cam.backgroundColor = new Color(0.53f, 0.77f, 0.86f);

        // Clean
        foreach (var n in new[]{ "Decorations","Tilemap","BGGrid","SkyBG",
                                  "_SpritePreview","_SpritePreview2","BankDecor","CropPatch" })
        { var o = GameObject.Find(n); if (o) Object.DestroyImmediate(o); }

        if (!Directory.Exists(TILE_DIR)) Directory.CreateDirectory(TILE_DIR);

        // Sky background sprite (above tree line y≥9, so bottom edge at y=9)
        BuildSky();

        // Tilemap grid — PPU=48 so 1 tile = 1 world unit
        var grid = new GameObject("Tilemap");
        grid.AddComponent<Grid>().cellSize = Vector3.one;
        var tmGnd   = MakeLayer(grid, "Map_Ground", 0);
        var tmWater = MakeLayer(grid, "Map_Water",  1);
        var tmSand  = MakeLayer(grid, "Map_Sand",   2);

        FillGrass(tmGnd);
        FillWater(tmWater);
        FillSand(tmSand);

        // Force refresh so tiles display correctly
        tmGnd.RefreshAllTiles();
        tmWater.RefreshAllTiles();
        tmSand.RefreshAllTiles();

        var root = new GameObject("Decorations");
        PlaceTrees(root.transform);
        PlaceBridge(root.transform);
        PlaceRocks(root.transform);
        PlaceCrops(root.transform);
        PlaceBushes(root.transform);

        ApplyUnlit();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[BuildIso] Complete!");
    }

    // Sky quad covering area above grass line.
    // Camera: ortho=5, pos=(10,2) → top visible edge ≈ y=7.
    // Trees reach ~y=11. Sky starts at y=9 (above trees), center at y=15, height=12 → bounds y=9..21.
    static void BuildSky()
    {
        var go = new GameObject("SkyBG");
        // Center at y=15, height=12 → bottom edge at y=15-6=9, above tree tops
        go.transform.position = new Vector3(10f, 15f, 1f);
        go.transform.localScale = new Vector3(26f, 12f, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = -10;
        var sp = D(10, 0);
        if (sp != null)
            sr.sprite = sp;
        else
        {
            var tex = new Texture2D(4, 4);
            var sky = new Color(0.53f, 0.77f, 0.86f);
            for (int i = 0; i < 16; i++) tex.SetPixel(i % 4, i / 4, sky);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), Vector2.one * 0.5f, 1f);
        }
        if (_mat) sr.sharedMaterial = _mat;
    }

    // Camera: ortho=5, pos=(10,2) → visible x≈0..20, y≈-3..7
    // Layout: lower grass y=-4..-1, river y=0..3, upper grass y=4..12
    static void FillGrass(Tilemap tm)
    {
        for (int x = -2; x <= 22; x++)
        for (int y = -4; y <= 12; y++)
        {
            if (y >= 0 && y <= 3) continue;
            int col = ((x * 7 + y * 13) & 0xFF) % 4;
            var sp = G(0, col);
            if (sp == null) continue;
            tm.SetTile(new Vector3Int(x, y, 0), MakeTile(sp));
        }
    }

    // River: y=0..3, water tiles
    static void FillWater(Tilemap tm)
    {
        for (int x = -2; x <= 22; x++)
        for (int y = 1; y <= 2; y++)
        {
            int col = ((x * 3 + y) & 0x7);
            var sp = G(2, col);
            if (sp != null) tm.SetTile(new Vector3Int(x, y, 0), MakeTile(sp));
        }
    }

    // Sand banks at river edges y=0 (lower) and y=3 (upper)
    static void FillSand(Tilemap tm)
    {
        for (int x = -2; x <= 22; x++)
        {
            int col = ((x * 5) & 0x7);
            var spTop = G(3, col);
            var spBot = G(3, (col + 2) % 8);
            if (spTop != null) tm.SetTile(new Vector3Int(x, 3, 0), MakeTile(spTop));
            if (spBot != null) tm.SetTile(new Vector3Int(x, 0, 0), MakeTile(spBot));
        }
    }

    // ── Trees ──────────────────────────────────────────────────────────────
    static void PlaceTrees(Transform parent)
    {
        // Left: 1 big oak + 1 small accent
        PlaceOak(parent, "OakL1",  1.5f,  9.2f, 4.2f, 0);
        PlaceSmallTree(parent, "SmL2",  5.0f,  6.5f, 1.8f, 4);

        // Right: 1 big oak
        PlaceOak(parent, "OakR1", 20.0f,  9.2f, 4.0f, 2);
        PlaceSmallTree(parent, "SmR2", 17.0f,  6.8f, 1.8f, 5);

        // Fruit tree (right of center)
        PlaceFruitTree(parent, "FruitT", 14.0f, 8.8f, 3.2f);

        // Small accent trees near bank
        PlaceSmallTree(parent, "SmL1",  7.0f, 6.2f, 1.6f, 6);
        PlaceSmallTree(parent, "SmR1", 19.0f, 6.2f, 1.6f, 7);
    }

    static void PlaceOak(Transform parent, string id, float cx, float cy, float sc, int variant)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        int dc = variant;
        PlacePart(g.transform, "TL", D(0, dc),   -sc * 0.5f,  sc * 0.5f, sc, 11);
        PlacePart(g.transform, "TR", D(0, dc + 1), sc * 0.5f,  sc * 0.5f, sc, 11);
        PlacePart(g.transform, "BL", D(1, dc),   -sc * 0.5f, -sc * 0.5f, sc, 11);
        PlacePart(g.transform, "BR", D(1, dc + 1), sc * 0.5f, -sc * 0.5f, sc, 11);
    }

    static void PlaceFruitTree(Transform parent, string id, float cx, float cy, float sc)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        PlacePart(g.transform, "TL", D(2, 0), -sc * 0.5f,  sc * 0.5f, sc, 11);
        PlacePart(g.transform, "TR", D(2, 1),  sc * 0.5f,  sc * 0.5f, sc, 11);
        PlacePart(g.transform, "BL", D(3, 0), -sc * 0.5f, -sc * 0.5f, sc, 11);
        PlacePart(g.transform, "BR", D(3, 1),  sc * 0.5f, -sc * 0.5f, sc, 11);
    }

    static void PlaceSmallTree(Transform parent, string id, float cx, float cy, float sc, int col)
    {
        var g = new GameObject(id);
        g.transform.SetParent(parent, false);
        g.transform.position = new Vector3(cx, cy, 0f);
        PlacePart(g.transform, "C", D(0, col), 0f, 0f, sc, 10);
    }

    // ── Bridge ─────────────────────────────────────────────────────────────
    static void PlaceBridge(Transform parent)
    {
        var g = new GameObject("Bridge");
        g.transform.SetParent(parent, false);

        for (int bx = 0; bx <= 1; bx++)
        for (int by = 0; by <= 3; by++)
        {
            var sp = G(5, (by * 2 + bx) % 8);
            SR(g.transform, $"Pl{bx}{by}", sp, 9.5f + bx, (float)by, 12);
        }
        for (int by = 0; by <= 3; by++)
            SR(g.transform, $"RailL{by}", D(8, 0), 9.0f, (float)by, 13, 0.9f);
        for (int by = 0; by <= 3; by++)
            SR(g.transform, $"RailR{by}", D(8, 3), 11.0f, (float)by, 13, 0.9f);
    }

    // ── Rocks ───────────────────────────────────────────────────────────────
    static void PlaceRocks(Transform parent)
    {
        var g = new GameObject("Rocks");
        g.transform.SetParent(parent, false);

        SR(g.transform, "RkR_LA", D(4, 0), 14.0f, 5.2f, 7, 2.0f);
        SR(g.transform, "RkR_LB", D(4, 1), 15.2f, 5.2f, 7, 2.0f);
        SR(g.transform, "RkR_M1", D(4, 2), 16.2f, 5.0f, 7, 1.6f);
        SR(g.transform, "RkR_M2", D(4, 3), 13.2f, 4.8f, 7, 1.4f);
        SR(g.transform, "RkR_S1", D(4, 4), 17.0f, 4.6f, 7, 1.2f);
        SR(g.transform, "RkR_S2", D(4, 5), 16.8f, 5.5f, 7, 1.0f);

        SR(g.transform, "RkL_M1", D(4, 2),  5.5f, 4.9f, 7, 1.5f);
        SR(g.transform, "RkL_S1", D(4, 4),  6.5f, 5.1f, 7, 1.2f);
        SR(g.transform, "RkL_S2", D(4, 5),  4.8f, 4.6f, 7, 1.1f);
    }

    // ── Crops ───────────────────────────────────────────────────────────────
    static void PlaceCrops(Transform parent)
    {
        var g = new GameObject("Crops");
        g.transform.SetParent(parent, false);
        for (int cx = 0; cx < 3; cx++)
        for (int cy = 0; cy < 2; cy++)
        {
            float wx = 11.5f + cx;
            float wy = 5.5f + cy * 1.0f;
            var sp = D(5, (cx + cy * 2) % 8);
            SR(g.transform, $"Crop{cx}{cy}", sp, wx, wy, 6, 1.0f);
        }
    }

    // ── Bushes along bank ───────────────────────────────────────────────────
    static void PlaceBushes(Transform parent)
    {
        var g = new GameObject("Bushes");
        g.transform.SetParent(parent, false);

        (float x, float y, int v)[] bushes = {
            (2.0f, 4.5f, 2), (5.5f, 4.4f, 3), (7.5f, 4.6f, 4),
            (12.5f, 4.5f, 5), (17.5f, 4.4f, 2), (20.5f, 4.6f, 3),
        };
        foreach (var (x, y, v) in bushes)
            SR(g.transform, $"Bush{x}", D(2, v), x, y, 8, 1.3f);

        (float x, float y)[] tufts = {
            (1.5f, 4.8f), (4.0f, 4.7f), (6.5f, 4.8f), (9.5f, 4.7f),
            (13.5f, 4.8f), (16.0f, 4.7f), (19.0f, 4.8f),
            (3.0f, -0.3f), (7.5f, -0.4f), (12.0f, -0.3f), (17.5f, -0.4f),
        };
        for (int i = 0; i < tufts.Length; i++)
            SR(g.transform, $"Tuft{i}", D(7, i % 8), tufts[i].x, tufts[i].y, 8, 1.0f);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
    static void PlacePart(Transform p, string id, Sprite sp, float lx, float ly, float sc, int order)
    {
        if (!sp) { Debug.LogWarning($"[BuildIso] null sprite {id}"); return; }
        var go = new GameObject(id);
        go.transform.SetParent(p, false);
        go.transform.localPosition = new Vector3(lx, ly, 0);
        go.transform.localScale    = Vector3.one * sc;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.sortingOrder = order;
        if (_mat) sr.sharedMaterial = _mat;
    }

    static void SR(Transform p, string id, Sprite sp, float wx, float wy, int order, float sc = 1f)
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
