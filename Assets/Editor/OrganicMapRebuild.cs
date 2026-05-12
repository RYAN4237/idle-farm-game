using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Complete organic map rebuild — Stardew-style
/// - Winding river (2-3 tiles wide, not straight)
/// - Organic pond shapes
/// - Grass variation with noise
/// - Water→Grass transition tiles on a third layer
public class OrganicMapRebuild
{
    const string GRASS_PATH = "Assets/Sprout Lands - Sprites - Basic pack/Tilesets/Grass.png";
    const string TILE_DIR   = "Assets/Tiles";

    // Scene bounds
    const int X0 = -3, X1 = 23, Y0 = -3, Y1 = 8;

    // ── Winding river: hand-placed cells ──────────────────────────────────────
    // Each entry is (x, y). River starts at left edge, bends up and down,
    // widens at two spots to feel like it has current/eddies.
    static readonly (int x, int y)[] RIVER = new (int,int)[]
    {
        // Far left approach (narrow, y=2)
        (-3,2),(-2,2),(-1,2),(0,2),
        // Slight bend down
        (1,2),(2,2),(2,1),(3,1),(3,2),
        // Widens to 2 tiles
        (4,1),(4,2),(5,1),(5,2),(5,3),
        // Bends back up, stays wide
        (6,2),(6,3),(7,3),(7,2),(8,2),
        // Narrows, slight upward bulge
        (9,2),(9,3),(10,3),(10,2),(11,2),
        // Dips down
        (12,2),(12,1),(13,1),(13,2),(14,2),
        // Widens again
        (15,2),(15,1),(16,1),(16,2),(17,2),
        // Exit right edge
        (18,2),(19,2),(20,2),(21,2),(22,2),
    };

    // ── Organic pond (lower-left) ─────────────────────────────────────────────
    static readonly (int x, int y)[] POND = new (int,int)[]
    {
        // Core
        (2,-1),(3,-1),(4,-1),(3,-2),(4,-2),(5,-2),
        // Protrusion north-east
        (5,-1),(5,0),(4,0),
        // Protrusion south-west
        (2,-2),(1,-2),(1,-1),
        // Small inlet
        (6,-2),(6,-1),
    };

    // ── Upper pool (upper-right) ──────────────────────────────────────────────
    static readonly (int x, int y)[] POOL = new (int,int)[]
    {
        (16,5),(17,5),(18,5),(19,5),
        (17,6),(18,6),(19,6),(20,6),
        (18,7),(19,7),
        // Protrusion south
        (16,4),(17,4),
    };

    [MenuItem("Tools/Organic Map Rebuild")]
    public static void Execute()
    {
        if (!AssetDatabase.IsValidFolder(TILE_DIR))
            AssetDatabase.CreateFolder("Assets", "Tiles");

        var grassSprites = LoadSprites(GRASS_PATH);
        if (grassSprites.Length == 0) { Debug.LogError("No grass sprites"); return; }

        // Build tile cache — save every needed tile as persistent asset
        var tileCache = BuildTileCache(grassSprites);

        RebuildGrass(tileCache);
        RebuildWater(tileCache);
        PaintTransitionLayer(tileCache);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[OrganicMapRebuild] Done");
    }

    // ── Tile cache ────────────────────────────────────────────────────────────
    static Dictionary<string,Tile> BuildTileCache(Sprite[] grassSprites)
    {
        var cache = new Dictionary<string,Tile>();

        // Interior fill group (y=0 row): Grass_66..76 — solid, no outline
        // Used for main ground fill
        for (int i = 66; i <= 76; i++)
            MakeTile(cache, grassSprites, $"Grass_{i}", $"g{i}");

        // Accent/detail group (y=80 row): Grass_11..21 — darker, more texture
        // Used as noise dots scattered across field
        for (int i = 11; i <= 21; i++)
            MakeTile(cache, grassSprites, $"Grass_{i}", $"g{i}");

        // Water-edge transition (y=64 row): Grass_22..32
        // These are grass tiles with water-facing edge details — paint ON TOP of water border
        for (int i = 22; i <= 32; i++)
            MakeTile(cache, grassSprites, $"Grass_{i}", $"g{i}");

        // Corner transitions (y=48 row): Grass_33..43
        for (int i = 33; i <= 43; i++)
            MakeTile(cache, grassSprites, $"Grass_{i}", $"g{i}");

        // Tuft/isolated detail (y=96 row top): Grass_0..10
        // These are the "clump" shapes — use sparingly as scatter tiles
        for (int i = 0; i <= 10; i++)
            MakeTile(cache, grassSprites, $"Grass_{i}", $"g{i}");

        return cache;
    }

    static void MakeTile(Dictionary<string,Tile> cache, Sprite[] sprites, string spriteName, string key)
    {
        var spr = sprites.FirstOrDefault(s => s.name == spriteName);
        if (spr == null) return;

        string path = $"{TILE_DIR}/OT_{key}.asset";
        AssetDatabase.DeleteAsset(path);
        var t = ScriptableObject.CreateInstance<Tile>();
        t.sprite = spr;
        t.name   = key;
        AssetDatabase.CreateAsset(t, path);
        cache[key] = t;
    }

    // ── Build water set ───────────────────────────────────────────────────────
    static HashSet<(int,int)> BuildWaterSet()
    {
        var set = new HashSet<(int,int)>();
        foreach (var p in RIVER) set.Add(p);
        foreach (var p in POND)  set.Add(p);
        foreach (var p in POOL)  set.Add(p);
        return set;
    }

    // ── Grass layer ───────────────────────────────────────────────────────────
    static void RebuildGrass(Dictionary<string,Tile> cache)
    {
        var grassGO = GameObject.Find("Tilemap/GrassLayer") ?? GameObject.Find("GrassLayer");
        if (grassGO == null) { Debug.LogError("GrassLayer not found"); return; }
        var tm = grassGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();

        var rng   = new System.Random(7331);
        var water = BuildWaterSet();

        // Main fill tiles: weighted from Grass_66..76
        // More variety = less tiling = more natural
        // Distribution: 66(25%) 67(20%) 68(15%) 69(10%) 70(10%) 71..76(20% shared)
        int[] fillIds = { 66,66,66,66,66, 67,67,67,67, 68,68,68, 69,69, 70,70, 71,72,73,74,75,76 };

        // Accent tiles: Grass_11..21 scattered as 1-in-6 chance
        int[] accentIds = { 11,12,13,14,15,16,17,18,19,20,21 };

        for (int x = X0; x < X1; x++)
        for (int y = Y0; y < Y1; y++)
        {
            // Use Perlin noise for accent distribution — breaks grid pattern
            float noise = Mathf.PerlinNoise(x * 0.31f + 0.5f, y * 0.31f + 0.5f);

            int spriteId;
            if (noise > 0.72f)
                // Accent patch (darker texture)
                spriteId = accentIds[rng.Next(accentIds.Length)];
            else
                spriteId = fillIds[rng.Next(fillIds.Length)];

            var key = $"g{spriteId}";
            if (!cache.TryGetValue(key, out var tile)) continue;
            tm.SetTile(new Vector3Int(x, y, 0), tile);
        }

        var renderer = grassGO.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 0;
        EditorUtility.SetDirty(grassGO);
        Debug.Log("[OrganicMapRebuild] GrassLayer rebuilt");
    }

    // ── Water layer ───────────────────────────────────────────────────────────
    static void RebuildWater(Dictionary<string,Tile> cache)
    {
        var waterGO = GameObject.Find("Tilemap/WaterLayer") ?? GameObject.Find("WaterLayer");
        if (waterGO == null) { Debug.LogError("WaterLayer not found"); return; }
        var tm = waterGO.GetComponent<Tilemap>();
        tm.ClearAllTiles();
        tm.color = Color.white;

        // Load the solid water tile we created earlier
        var solidTile = AssetDatabase.LoadAssetAtPath<Tile>($"{TILE_DIR}/WaterSolid.asset");
        if (solidTile == null)
        {
            // Fallback: create inline
            solidTile = ScriptableObject.CreateInstance<Tile>();
            solidTile.color = new Color(0.35f, 0.70f, 0.85f, 1f);
        }

        foreach (var (x,y) in RIVER) tm.SetTile(new Vector3Int(x, y, 0), solidTile);
        foreach (var (x,y) in POND)  tm.SetTile(new Vector3Int(x, y, 0), solidTile);
        foreach (var (x,y) in POOL)  tm.SetTile(new Vector3Int(x, y, 0), solidTile);

        var renderer = waterGO.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 1;
        EditorUtility.SetDirty(waterGO);
        Debug.Log("[OrganicMapRebuild] WaterLayer rebuilt");
    }

    // ── Transition layer: paint grass edge tiles where grass meets water ───────
    static void PaintTransitionLayer(Dictionary<string,Tile> cache)
    {
        // Find or create the transition tilemap
        var transGO = GameObject.Find("Tilemap/TransitionLayer");
        if (transGO == null)
        {
            var gridGO = GameObject.Find("Tilemap");
            transGO = new GameObject("TransitionLayer");
            transGO.transform.SetParent(gridGO.transform, false);
            transGO.AddComponent<Tilemap>();
            var r = transGO.AddComponent<TilemapRenderer>();
            r.sortingOrder = 2; // above water, above grass
        }

        var tm       = transGO.GetComponent<Tilemap>();
        var renderer = transGO.GetComponent<TilemapRenderer>();
        renderer.sortingOrder = 2;
        tm.ClearAllTiles();

        var water = BuildWaterSet();
        var rng   = new System.Random(999);

        // For every water cell, check each 4-directional neighbor.
        // If neighbor is grass (not water), paint a transition tile on that grass cell.
        // Transition tile choice depends on which direction faces water.
        //
        // Grass_22..32 (y=64 row) are the edge tiles in Sprout Lands blob format:
        // These tiles have grass on one side and transparent/water on other side.
        // We pick from this range randomly for natural variety.
        int[] transIds = { 22,23,24,25,26,27,28,29,30,31,32 };
        int[] cornerIds = { 33,34,35,36,37,38,39,40,41,42,43 };

        var painted = new HashSet<(int,int)>();

        foreach (var (wx, wy) in water)
        {
            // Check 4 neighbors
            (int dx, int dy)[] dirs = { (0,1),(0,-1),(1,0),(-1,0) };
            foreach (var (dx, dy) in dirs)
            {
                int nx = wx + dx, ny = wy + dy;
                if (water.Contains((nx, ny))) continue; // neighbor is also water
                if (nx < X0 || nx >= X1 || ny < Y0 || ny >= Y1) continue;
                if (painted.Contains((nx, ny))) continue;

                // Check if diagonal neighbors are also water (corner case)
                bool hasCorner = water.Contains((nx+dy, ny+dx)) || water.Contains((nx-dy, ny-dx));
                int[] pool = hasCorner ? cornerIds : transIds;
                int id = pool[rng.Next(pool.Length)];

                var key = $"g{id}";
                if (cache.TryGetValue(key, out var tile))
                {
                    tm.SetTile(new Vector3Int(nx, ny, 0), tile);
                    painted.Add((nx, ny));
                }
            }
        }

        EditorUtility.SetDirty(transGO);
        Debug.Log($"[OrganicMapRebuild] TransitionLayer: {painted.Count} transition tiles painted");
    }

    static Sprite[] LoadSprites(string path)
        => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
}
